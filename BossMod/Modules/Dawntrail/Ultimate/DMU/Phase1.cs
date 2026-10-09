namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class GravenImage3(BossModule module) : Components.UniformStackSpread(module, 5f, 5f, 1, 1)
{
    private static readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private static readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    private enum TetherGroup { Support, DPS }
    private TetherGroup? tetherSleepGroup = null;
    private TetherGroup? tetherConfusionGroup = null;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        if (source.Position.AlmostEqual(new(95.000f, 25.000f), 5f))
        {
            tetherConfusionGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
        }
        else if (source.Position.AlmostEqual(new(107.000f, 43.000f), 5f))
        {
            tetherSleepGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
            AddSpread(target, WorldState.FutureTime(6.5f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IdyllicWill)
        {
            Spreads.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        // Static strategy - keep separate to make it easier to manage
        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo && dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                Arena.ZoneCircleOutline(new WPos(93.636f, 96.500f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                Arena.ZoneCircleOutline(new WPos(104.000f, 93.636f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                Arena.ZoneCircleOutline(new WPos(90.500f, 106.364f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                Arena.ZoneCircleOutline(new WPos(106.364f, 109.500f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                Arena.ZoneCircleOutline(new WPos(96.500f, 106.364f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                Arena.ZoneCircleOutline(new WPos(106.364f, 104.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                Arena.ZoneCircleOutline(new WPos(93.636f, 91.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                Arena.ZoneCircleOutline(new WPos(109.500f, 93.636f), 1.0f, Colors.Safe, 2);
            }
        }

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo && !dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 96.500f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 91.000f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 96.500f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(93.636f, 91.000f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(104.000f, 93.636f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(109.500f, 93.636f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(104.000f, 93.636f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(109.500f, 93.636f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(96.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(90.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(96.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(90.500f, 106.364f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 104.000f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 109.500f), 1.0f, Colors.Safe, 2);
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 104.000f), 1.0f, Colors.Safe, 2);
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(106.364f, 109.500f), 1.0f, Colors.Safe, 2);
                }
            }
        }

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow && dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 93.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 116.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                Arena.ZoneCircleOutline(new WPos(116.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 108.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                Arena.ZoneCircleOutline(new WPos(108.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                Arena.ZoneCircleOutline(new WPos(100.000f, 84.000f), 1.0f, Colors.Safe, 2);
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                Arena.ZoneCircleOutline(new WPos(84.000f, 100.000f), 1.0f, Colors.Safe, 2);
            }
        }

        if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow && !dmuConfig.P1GravenImage3Static)
        {
            if (assignment == PartyRolesConfig.Assignment.MT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 93.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 84.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 93.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 84.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.OT)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(84.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.R2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(93.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(84.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H1)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 108.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 116.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M1)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 108.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(100.000f, 116.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.H2)
            {
                if (tetherSleepGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(108.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.Support)
                {
                    Arena.ZoneCircleOutline(new WPos(116.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }

            if (assignment == PartyRolesConfig.Assignment.M2)
            {
                if (tetherSleepGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(108.000f, 100.000f), 1.0f, Colors.Safe, 2); // Inwards
                }

                if (tetherConfusionGroup == TetherGroup.DPS)
                {
                    Arena.ZoneCircleOutline(new WPos(116.000f, 100.000f), 1.0f, Colors.Safe, 2); // Outwards
                }
            }
        }
    }
}

sealed class Gaze(BossModule module) : Components.GenericGaze(module)
{
    private Eye[] _eye = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)Animations.EyeStart)
        {
            var pos = actor.Position.Quantized();
            _eye = [new Eye(pos, WorldState.FutureTime(9.9d), inverted: actor.OID == (uint)OID.StatueYellowEye, eyeCenter: IndicatorWorldPos(pos))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.IndolentWill) or ((uint)AID.AveMaria))
        {
            ++NumCasts;
            _eye = [];
        }
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => _eye;
}
