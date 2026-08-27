namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO think of a way to move spreads a bit further in for range? - First one is fine, but second one has a half cleave which could be dangerous
sealed class Gravitas(BossModule module) : Components.UniformStackSpread(module, 5, 5, 4, 4) {
    private readonly WPos stackSource = new WPos(102.500f, 22.500f);
    private readonly WPos spreadSource = new WPos(126.000f, 41.500f);
    private readonly List<Spread> spreadsIncoming = [];
    private BitMask spreadTethers = default;
    public P1GravitasData.Side side = P1GravitasData.Side.NONE;
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly BlizzardIIIBlowout? blizzardIIIBlowout = module.FindComponent<BlizzardIIIBlowout>();

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID != (uint)TetherID.GravenImageTether || Raid.FindSlot(tether.Target) is var slot && slot < 0) {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null) {
            return;
        }

        if (source.Position.AlmostEqual(stackSource, 5.0f)) {
            AddStack(target, WorldState.FutureTime(6.5f));
            return;
        }

        if (source.Position.AlmostEqual(spreadSource, 5.0f)) {
            spreadsIncoming.Add(new(target, 5.0f, WorldState.FutureTime(10.6f)));
            spreadTethers.Set(slot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Gravitas && Stacks.Count > 0) {
            Stacks.RemoveAll(actor => actor.Target.InstanceID == spell.MainTargetID);

            if (Stacks.Count == 0) {
                Spreads.AddRange(spreadsIncoming);
                spreadsIncoming.Clear();
            }

            return;
        }

        if (spell.Action.ID == (uint)AID.Vitrophyre && Spreads.Count > 0) {
            Spreads.RemoveAll(actor => actor.Target.InstanceID ==  spell.MainTargetID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2None) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (Stacks.Count != 0) {
            var safeSpot = P1GravitasData.PuddlesSpots.GetValueOrDefault(side);
            if (safeSpot == default) {
                return;
            }

            // If a blizzard blowout is being cast, we move the safeSpot 0.5f away from the aoe zone
            if (blizzardIIIBlowout != null) {
                foreach (var caster in blizzardIIIBlowout.Casters) {
                    var tempSafeSpot = safeSpot + new WDir(-0.5f, 0);
                    safeSpot = caster.Check(tempSafeSpot) ? safeSpot + new WDir(0.5f, 0) : tempSafeSpot;
                }
            }

            Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
            return;
        }

        if (Spreads.Count != 0) {
            if (spreadTethers[pcSlot]) {
                var safeSpot = side == P1GravitasData.Side.NORTH
                    ? P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).north
                    : P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).south;
                if (safeSpot == default) {
                    return;
                }

                Arena.ZoneCircleOutline(safeSpot, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
                return;
            }

            Arena.ZoneCircleOutline(Module.Center, PositionDrawSize.NORMAL, Colors.Safe, 2.0f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (dmuConfig.P1GravenImage2 == DMUConfig.P1GravenImage2Strategy.GravenImage2None) {
            return;
        }

        // Pre-position for stack
        if (!Active) {
            var safeSpot = P1GravitasData.PuddlesSpots.GetValueOrDefault(side);
            if (safeSpot == default) {
                return;
            }

            hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, PositionAIRadius.NORMAL, PositionWeights.PRE_POSITION));
            return;
        }

        // Position for stack
        if (Stacks.Count != 0) {
            var safeSpot = P1GravitasData.PuddlesSpots.GetValueOrDefault(side);
            if (safeSpot == default) {
                return;
            }

            // If a blizzard blowout is being cast, we move the safeSpot 0.5f away from the aoe zone
            if (blizzardIIIBlowout != null) {
                foreach (var caster in blizzardIIIBlowout.Casters) {
                    var tempSafeSpot = safeSpot + new WDir(-0.5f, 0);
                    safeSpot = caster.Check(tempSafeSpot) ? safeSpot + new WDir(0.5f, 0) : tempSafeSpot;
                }
            }

            hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.PRECISE), Stacks[0].Activation);
            return;
        }

        // Position for spreads
        if (Spreads.Count == 0) {
            return;
        }

        // Micro adjustment for spreads
        var remaining = (Spreads[0].Activation - WorldState.CurrentTime).TotalSeconds;
        if (remaining <= 1.0f) {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        if (spreadTethers[slot]) {
            var safeSpot = side == P1GravitasData.Side.NORTH
                ? P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).north
                : P1GravitasData.SpreadSafeSpots.GetValueOrDefault(assignment).south;
            if (safeSpot == default) {
                return;
            }

            var puddles = Module.Enemies((uint)OID.PurplePuddles).ToList();
            foreach (var puddle in puddles) {
                hints.AddForbiddenZone(new AOEShapeCircle(5.0f + SpreadRadius + ExtraAISpreadThreshold), puddle.Position);
            }

            // If micro adjustment is active, then the zone will become a goal zone rather than a forbidden zone, so the pathfinder can move anywhere
            // not forbidden if needed - this is mainly here if the puddles are placed slightly incorrect and still close to the boss, so the AI can still
            // try and adjust
            if (remaining <= 1.0f) {
                hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, PositionAIRadius.PRECISE, PositionWeights.MECHANIC));
                return;
            }

            hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, PositionAIRadius.NORMAL), Spreads[0].Activation);
            return;
        }

        // Players without a spread stand directly middle under boss
        hints.GoalZones.Add(AIHints.GoalProximity(Module.Center, PositionAIRadius.PRECISE, PositionWeights.MECHANIC));
    }
}

// Puddles are voidzones, but custom component is needed to make them disappear when they're actually soaked instead of using eventState != 7 or isDead
// TODO
//  1. Fix the AI logic for people without spreads when the puddles spawn - most likely in the component above
//  2. Setup AI logic during Knockbacks - AIHints should become available after KB cast has happened for soaking - check the cast for it maybe
sealed class GravitasPuddles(BossModule module) : BossComponent(module) {
    private readonly AOEShapeCircle shape = new(5.0f);
    private readonly List<Actor> puddles = [];
    private bool inverted = false;

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.PurplePuddles) {
            puddles.Add(actor);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.PurplePuddles && state == (uint)Animations.PuddleSoakReady) {
            inverted = true;
        }

        if (actor.OID == (uint)OID.PurplePuddles && state == (uint)Animations.PuddleExplosion) {
            if (puddles.Count > 0) {
                puddles.Remove(actor);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (puddles.Count == 0) {
            return;
        }

        var colour = inverted ? Colors.SafeFromAOE : Colors.AOE;
        foreach (var puddle in puddles) {
            shape.Draw(Arena, puddle.Position, puddle.Rotation, colour);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (puddles.Count == 0) {
            return;
        }

        var inVoidzone = false;

        foreach (var puddle in puddles) {
            if (shape.Check(actor.Position, puddle.Position)) {
                inVoidzone = true;
                break;
            }
        }

        if (inverted) {
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        } else if (inVoidzone) {
            hints.Add("GTFO from voidzone!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (puddles.Count == 0) {
            return;
        }

        var shapes = new List<ShapeDistance>();

        foreach (var puddle in puddles) {
            var shapeInstance = shape.Distance(puddle.Position.Quantized(), puddle.Rotation);
            shapes.Add(shapeInstance);
        }

        if (shapes.Count == 0) {
            return;
        }

        hints.AddForbiddenZone(inverted ? new SDInvertedUnion([.. shapes]) : new SDUnion([.. shapes]), WorldState.FutureTime(5.0f));
    }
}

// TODO enable TB and set them up correctly - might need to setup activation for TB
//  this is so they don't walk by the puddle since it would have the same foreground weight, and they have to be different weights
//  Potentially might be better to make it 45 degree either side of the tank now with a radius of like 10 instead of the full part of north
