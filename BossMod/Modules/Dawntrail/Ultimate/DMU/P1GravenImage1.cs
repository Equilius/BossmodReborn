namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO
//  1. Melee uptime for the knockback - Have a small time to adjust to the correct spot afterwards
//  2. Improve spreads / stack adjustments - After every has resolved ~0.9 seconds until spreads / stack resolve, have time to adjust if needed
//      Currently spreads will cover up their safe spot, but could be made bigger since nothing else is going off, just need to keep them around that point
//      Will also make stacks work correctly if someone is not within range, but it means in prog parties it will look strange

sealed class PulseWave(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave) {
    public bool active = false;
    private DateTime activation;
    public const float KnockbackDistance = 13.0f;
    public BitMask affectedPlayers;
    public Actor? tetherSource = null;

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.GravenImageTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0) {
            tetherSource = source;
            affectedPlayers[slot] = true;
            activation = WorldState.FutureTime(5.0f);
            active = true;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.GravenImageTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0) {
            affectedPlayers[slot] = false;
            NumCasts++;

            if (NumCasts == 4) {
                active = false;
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        if (affectedPlayers[slot] && tetherSource != null) {
            return new Knockback[1] { new(tetherSource.Position, KnockbackDistance, activation, ignoreImmunes: true) };
        }

        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (!active) {
            return;
        }

        // Case: Non-tethered players will stand middle of their side
        if (!affectedPlayers[slot]) {
            if (actor.Role is Role.Tank or Role.Healer) {
                hints.GoalZones.Add(p => p.InCircle(new WPos(96.000f, 100.000f), 2.0f) ? 1.5f : 0.0f);
            }

            if (actor.Role is Role.Melee or Role.Ranged) {
                hints.GoalZones.Add(p => p.InCircle(new WPos(104.000f, 100.000f), 2.0f) ? 1.5f : 0.0f);
            }
        }

        // Case: Tethered players will stand middle of their side, but slightly north
        if (affectedPlayers[slot]) {
            if (actor.Role is Role.Tank or Role.Healer) {
                hints.GoalZones.Add(p => p.InCircle(new WPos(96.000f, 94.000f), 2.0f) ? 1.5f : 0.0f);
            }

            if (actor.Role is Role.Melee or Role.Ranged) {
                hints.GoalZones.Add(p => p.InCircle(new WPos(104.000f, 94.000f), 2.0f) ? 1.5f : 0.0f);
            }
        }
    }
}

// Custom version specially for GravenImage1 to make the player avoid going to the incorrect side as the pathfinder will override everything when the cast
// has <1.0f left, so this is needed to ensure the player move to their side only (left/right depending on the role)
sealed class BlizzardIIIBlowoutGraven1 : BlizzardIIIBlowout {
    public bool? supportNorth = null;
    public bool? dpsNorth = null;

    public BlizzardIIIBlowoutGraven1(BossModule module) : base(module) {}

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == (uint)AID.BlizzardIIIBlowout || spell.Action.ID == (uint)AID.BlizzardIIIBlowout1) {
            supportNorth = !Casters.Exists(c => c.Check(new WPos(89.000f, 89.000f)));
            dpsNorth = !Casters.Exists(c => c.Check(new WPos(111.000f, 89.000f)));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (enabledHints) {
            // Case: support players will avoid right side completely
            if (actor.Role is Role.Tank or Role.Healer) {
                hints.AddForbiddenZone(new SDCone(new WPos(100.0f, 100.0f), 100.0f, Angle.AnglesCardinals[2].ToDirection().OrthoR().ToAngle(), 90.0f.Degrees()));
            }

            // Case: damage dealers will avoid left side completely
            if (actor.Role is Role.Melee or Role.Ranged) {
                hints.AddForbiddenZone(new SDCone(new WPos(100.0f, 100.0f), 100.0f, Angle.AnglesCardinals[2].ToDirection().OrthoL().ToAngle(), 90.0f.Degrees()));
            }
        }
    }
}

sealed class FlagrantFire(BossModule module) : Components.UniformStackSpread(module, 6.0f, 5.0f, 4, 4) {
    private enum SpreadStack { None, Spread, Stack }
    private enum TellingTheTruth { Unknown, Yes, No}
    private SpreadStack mechanic = SpreadStack.None;
    private TellingTheTruth tellingTheTruth = TellingTheTruth.Unknown;
    private readonly PulseWave? PulseWave = module.FindComponent<PulseWave>();
    private readonly BlizzardIIIBlowoutGraven1? blizzardSafeSpots = module.FindComponent<BlizzardIIIBlowoutGraven1>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        switch ((IconID)iconID) {
            case IconID.P1SpreadIcon:
                mechanic = SpreadStack.Spread;
                SolveMechanic();
                break;
            case IconID.P1StackIcon:
                mechanic = SpreadStack.Stack;
                SolveMechanic();
                break;
            case IconID.FireRingBlueOrb:
                tellingTheTruth = TellingTheTruth.Yes;
                SolveMechanic();
                break;
            case IconID.FireRingQuestionMark:
                tellingTheTruth = TellingTheTruth.No;
                SolveMechanic();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.FlagrantFireIIISpread or (uint)AID.FlagrantFireIIIStack) {
            if (Stacks.Count > 0 || Spreads.Count > 0) {
                Stacks.Clear();
                Spreads.Clear();
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (PulseWave == null || PulseWave.tetherSource == null) {
            return;
        }

        var safeSpot = SafeSpot(pcSlot, pc);
        if (safeSpot == default) {
            return;
        }

        switch (PulseWave.affectedPlayers[pcSlot]) {
            case true when dmuConfig.P1GravenImage1KnockbackAdditionalHints:
                Arena.ZoneCircleOutline(GetKnockbackPosition(PulseWave.tetherSource.Position, safeSpot), 1.0f, Colors.Safe, 2.0f);
                Arena.ZoneCircleOutline(safeSpot, 1.0f, Colors.Danger, 2.0f);
                break;
            case true when !dmuConfig.P1GravenImage1KnockbackAdditionalHints:
                Arena.ZoneCircleOutline(safeSpot, 1.0f, Colors.Danger, 2.0f);
                break;
            default:
                Arena.ZoneCircleOutline(safeSpot, 1.0f, Colors.Safe, 2.0f);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (PulseWave == null || PulseWave.tetherSource == null) {
            return;
        }

        var safeSpot = SafeSpot(slot, actor);
        if (safeSpot == default) {
            return;
        }

        if (PulseWave.affectedPlayers[slot]) {
            hints.GoalZones.Add(p => p.InCircle(GetKnockbackPosition(PulseWave.tetherSource.Position, safeSpot), 1.0f) ? 50.0f : 0.0f);
        } else {
            hints.GoalZones.Add(p => p.InCircle(safeSpot, 1.0f) ? 50.0f : 0.0f);
        }
    }

    // Pulls the data spot for the player
    private WPos SafeSpot(int pcSlot, Actor pc) {
        if (dmuConfig.P1GravenImage1 == DMUConfig.P1GravenImage1Strategy.GravenImage1None) {
            return default;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return default;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (blizzardSafeSpots == null || blizzardSafeSpots.dpsNorth == null || blizzardSafeSpots.supportNorth == null) {
            return default;
        }

        var spots = mechanic switch {
            SpreadStack.Spread => P1GravenImage1Data.SpreadSafeSpots.GetValueOrDefault(assignment),
            SpreadStack.Stack => P1GravenImage1Data.StackSafeSpots.GetValueOrDefault(pc.Role),
            _ => default,
        };

        if (spots == default) {
            return default;
        }

        var northSafe = pc.Role is Role.Tank or Role.Healer ? blizzardSafeSpots.supportNorth : blizzardSafeSpots.dpsNorth;
        return northSafe.Value ? spots.north : spots.south;
    }

    // Used to correctly assign what the mechanic is out of all the possible cases
    private void SolveMechanic() {
        if (mechanic == SpreadStack.None || tellingTheTruth == TellingTheTruth.Unknown) {
            return;
        }

        switch (mechanic, tellingTheTruth) {
            case (SpreadStack.Spread, TellingTheTruth.Yes):
            case (SpreadStack.Stack, TellingTheTruth.No):
                mechanic = SpreadStack.Spread;
                break;
            case (SpreadStack.Stack, TellingTheTruth.Yes):
            case (SpreadStack.Spread, TellingTheTruth.No):
                mechanic = SpreadStack.Stack;
                break;
        }

        SetupSpreadStacks();
    }

    // Sets up the spread or stack baits
    private void SetupSpreadStacks() {
       if (mechanic == SpreadStack.Spread) {
           AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(5.8f));
           return;
       }

       if (mechanic == SpreadStack.Stack) {
           var party = Raid.WithSlot(true, true, true);
           BitMask allowedSupports = default;
           BitMask allowedDDs = default;

           for (int i = 0; i < party.Length; i++) {
               ref var p = ref party[i];

               if (p.Item2.Role is Role.Tank or Role.Healer) {
                   allowedSupports.Set(p.Item1);
               }

               if (p.Item2.Role is Role.Melee or Role.Ranged) {
                   allowedDDs.Set(p.Item1);
               }
           }

           var addedSupport = false;
           var addedDD = false;

           for (int i = 0; i < party.Length; i++) {
               ref var player = ref party[i];
               var p = player.Item2;

               if (p.IsDead) {
                   continue;
               }

               if (p.Role is Role.Tank or Role.Healer) {
                   if (!addedSupport) {
                       AddStack(p, WorldState.FutureTime(5.8f), ~allowedSupports);
                       addedSupport = true;
                   }
               }

               if (p.Role is Role.Melee or Role.Ranged) {
                   if (!addedDD) {
                       AddStack(p, WorldState.FutureTime(5.8f), ~allowedDDs);
                       addedDD = true;
                   }
               }
           }
       }
    }

    // Calculates the knockback position for the player to land as close as possible to the safe spot for the player
    private WPos GetKnockbackPosition(WPos tetherPosition, WPos safePosition) {
        var distance = safePosition - tetherPosition;
        var length = distance.Length();
        var position = length > PulseWave.KnockbackDistance ? tetherPosition + (length - PulseWave.KnockbackDistance) * distance.Normalized() : safePosition;
        var safetyRadius = Arena.Bounds.Radius - 1.0f; // 1.0f away from the edge of the map for safety
        return (position - Arena.Center).LengthSq() > safetyRadius * safetyRadius ? Arena.Center + safetyRadius * (position - Arena.Center).Normalized() : position;
    }
}
