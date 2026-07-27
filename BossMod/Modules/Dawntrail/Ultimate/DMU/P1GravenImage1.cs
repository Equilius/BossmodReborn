namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO improvements: Melee uptime for the KB? - Have a small time to adjust to the correct spot afterwards
//  What happens if supports & dps swap strat in the future? - BlizzardSafeSpots & FlagrantFire should consider this

// TODO add AI hints
sealed class PulseWave(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave) {
    private DateTime activation;
    public const float KnockbackDistance = 13.0f;
    public BitMask affectedPlayers;
    public Actor? tetherSource = null;

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.GravenImageTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0) {
            tetherSource = source;
            affectedPlayers[slot] = true;
            activation = WorldState.FutureTime(5.0f);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.GravenImageTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0) {
            affectedPlayers[slot] = false;
            NumCasts++;
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        if (affectedPlayers[slot] && tetherSource != null) {
            return new Knockback[1] { new(tetherSource.Position, KnockbackDistance, activation, ignoreImmunes: true) };
        }

        return [];
    }
}

// TODO update name at some point - will need to check over P4 as well
// TODO add AI hints?
sealed class BlizzardSafeSpots(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlizzardIIIBlowout, (uint)AID.BlizzardIIIBlowout1],
    new AOEShapeCone(40f, 45f.Degrees())) {
    public bool? supportNorth = null;
    public bool? dpsNorth = null;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == (uint)AID.BlizzardIIIBlowout || spell.Action.ID == (uint)AID.BlizzardIIIBlowout1) {
            supportNorth = !Casters.Exists(c => c.Check(new WPos(89.000f, 89.000f)));
            dpsNorth = !Casters.Exists(c => c.Check(new WPos(111.000f, 89.000f)));
        }
    }
}

// TODO add AI hints?
sealed class FlagrantFire(BossModule module) : Components.UniformStackSpread(module, 6.0f, 5.0f, 4, 4) {
    private enum SpreadStack { None, Spread, Stack }
    private enum TellingTheTruth { Unknown, Yes, No}
    private SpreadStack mechanic = SpreadStack.None;
    private TellingTheTruth tellingTheTruth = TellingTheTruth.Unknown;
    private readonly PulseWave? PulseWave = module.FindComponent<PulseWave>();
    private readonly BlizzardSafeSpots? blizzardSafeSpots = module.FindComponent<BlizzardSafeSpots>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (dmuConfig.P1GravenImage1 == DMUConfig.P1GravenImage1Strategy.GravenImage1None) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (PulseWave == null || PulseWave.tetherSource == null) {
            return;
        }

        if (blizzardSafeSpots == null || blizzardSafeSpots.dpsNorth == null || blizzardSafeSpots.supportNorth == null) {
            return;
        }

        if (mechanic == SpreadStack.None) {
            return;
        }

        var northSafe = pc.Role == Role.Tank || pc.Role == Role.Healer ? blizzardSafeSpots.supportNorth : blizzardSafeSpots.dpsNorth;

        if (mechanic == SpreadStack.Spread) {
            if (!P1GravenImage1Data.SpreadSafeSpots.TryGetValue(assignment, out var spots)) {
                return;
            }

            var safeSpot = northSafe.Value ? spots.north : spots.south;

            if (PulseWave.affectedPlayers[pcSlot] && dmuConfig.P1GravenImage1KnockbackAdditionalHints) {
                Arena.AddCircle(GetKnockbackPosition(PulseWave.tetherSource.Position, safeSpot), 1.0f, Colors.Safe, 2.0f);
                Arena.AddCircle(safeSpot, 1.0f, Colors.Danger, 2.0f);
            } else {
                Arena.AddCircle(safeSpot, 1.0f, Colors.Safe, 2.0f);
            }
        }

        if (mechanic == SpreadStack.Stack) {
            if (!P1GravenImage1Data.StackSafeSpots.TryGetValue(pc.Role, out var spots)) {
                return;
            }

            var safeSpot = northSafe.Value ? spots.north : spots.south;

            if (PulseWave.affectedPlayers[pcSlot] && dmuConfig.P1GravenImage1KnockbackAdditionalHints) {
                Arena.AddCircle(GetKnockbackPosition(PulseWave.tetherSource.Position, safeSpot), 1.0f, Colors.Safe, 2.0f);
                Arena.AddCircle(safeSpot, 1.0f, Colors.Danger, 2.0f);
            } else {
                Arena.AddCircle(safeSpot, 1.0f, Colors.Safe, 2.0f);
            }
        }
    }

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
