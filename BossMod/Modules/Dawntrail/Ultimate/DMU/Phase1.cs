namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class GravitationalWave(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeRect rect = new(40f, 20f);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == (uint)Animations.PulseOrbStart)
        {
            _aoe = [new(rect, Arena.Center.Quantized(), (actor.OID == (uint)OID.YellowOrb ? 1f : -1f) * 90.Degrees())];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.GravitationalWave) or ((uint)AID.IntemperateWill))
        {
            ++NumCasts;
            _aoe = [];
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;
}

sealed class TeleTrouncing(BossModule module) : BossComponent(module)
{
    public int NumCasts = 0;
    private (Direction direction, DateTime activation)? Debuff1;
    private (Direction direction, DateTime activation)? Debuff2;
    private readonly List<WPos> hints = [];
    private enum Direction { UP, DOWN, LEFT, RIGHT }
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1)
        {
            NumCasts++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var player = Raid.FindSlot(actor.InstanceID);
        if (player is not (>= 0 and PartyState.PlayerSlot))
        {
            return;
        }

        Direction? dir = status.ID switch
        {
            (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2 => Direction.UP,
            (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2 => Direction.DOWN,
            (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2 => Direction.LEFT,
            (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => null
        };

        if (dir == null)
        {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8)
        {
            Debuff2 = (dir.Value, status.ExpireAt);
        }
        else
        {
            Debuff1 = (dir.Value, status.ExpireAt);
        }

        if (Debuff1 == null || Debuff2 == null)
        {
            return;
        }

        // Case 1: Both debuffs are in the same direction
        if (Debuff1.Value.direction == Debuff2.Value.direction)
        {
            if (Debuff1.Value.direction == Direction.DOWN)
            { // A waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(87.750f, 88.030f));
                    hints.Add(new WPos(87.750f, 93.570f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(112.000f, 94.000f));
                    hints.Add(new WPos(112.000f, 100.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.LEFT)
            { // B waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(112.135f, 87.993f));
                    hints.Add(new WPos(106.579f, 87.922f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(106.000f, 112.000f));
                    hints.Add(new WPos(100.000f, 112.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.UP)
            { // C waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(111.989f, 112.003f));
                    hints.Add(new WPos(112.125f, 106.306f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(88.000f, 106.000f));
                    hints.Add(new WPos(88.000f, 100.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.RIGHT)
            { // D waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
                {
                    hints.Add(new WPos(88.069f, 112.037f));
                    hints.Add(new WPos(93.798f, 112.161f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
                {
                    hints.Add(new WPos(94.000f, 88.000f));
                    hints.Add(new WPos(100.000f, 88.000f));
                }
            }

            return;
        }

        // Case 2: Both debuffs are in different directions
        var debuff1First = Debuff1.Value.activation <= Debuff2.Value.activation;

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.LEFT))
        {

            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(93.781f, 93.593f)); // 1 waymark
                    hints.Add(new WPos(93.576f, 88.051f)); // non-waymark
                }
                else
                {
                    hints.Add(new WPos(93.576f, 88.051f)); // non-waymark
                    hints.Add(new WPos(93.781f, 93.593f)); // 1 waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(88.000f, 112.000f));
                    hints.Add(new WPos(94.000f, 112.000f));
                }
                else
                {
                    hints.Add(new WPos(94.000f, 112.000f));
                    hints.Add(new WPos(88.000f, 112.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.RIGHT))
        {
            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(111.955f, 93.877f)); // non-waymark
                    hints.Add(new WPos(106.422f, 93.756f)); // 2 waymark
                }
                else
                {
                    hints.Add(new WPos(106.422f, 93.756f)); // 2 waymark
                    hints.Add(new WPos(111.955f, 93.877f)); // non-waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (upFirst)
                {
                    hints.Add(new WPos(88.000f, 94.000f));
                    hints.Add(new WPos(88.000f, 88.000f));
                }
                else
                {
                    hints.Add(new WPos(88.000f, 88.000f));
                    hints.Add(new WPos(88.000f, 94.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.RIGHT))
        {
            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(106.413f, 106.444f)); // 3 waymark
                    hints.Add(new WPos(106.337f, 112.135f)); // 3 non-waymark
                }
                else
                {
                    hints.Add(new WPos(106.337f, 112.135f)); // 3 non-waymark
                    hints.Add(new WPos(106.413f, 106.444f)); // 3 waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(112.000f, 88.000f));
                    hints.Add(new WPos(106.000f, 88.000f));
                }
                else
                {
                    hints.Add(new WPos(106.000f, 88.000f));
                    hints.Add(new WPos(112.000f, 88.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.LEFT))
        {
            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(88.103f, 106.377f)); // 4 non-waymark
                    hints.Add(new WPos(93.685f, 106.316f)); // 4 waymark
                }
                else
                {
                    hints.Add(new WPos(93.685f, 106.316f)); // 4 waymark
                    hints.Add(new WPos(88.103f, 106.377f)); // 4 non-waymark
                }
            }
            else if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow)
            {
                if (downFirst)
                {
                    hints.Add(new WPos(112.000f, 106.000f));
                    hints.Add(new WPos(112.000f, 112.000f));
                }
                else
                {
                    hints.Add(new WPos(112.000f, 112.000f));
                    hints.Add(new WPos(112.000f, 106.000f));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2
            or (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2
            or (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2
            or (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2)
        {

            var player = Raid.FindSlot(actor.InstanceID);
            if (player != PartyState.PlayerSlot)
            {
                return;
            }

            if (hints.Count != 0)
            {
                hints.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts == 16)
        {
            return;
        }

        if (Debuff1 == null || Debuff2 == null)
        {
            return;
        }
        var count = hints.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(hints[i], 1.0f, i == 0 ? Colors.Safe : default, 2f);
        }
    }
}

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
            _eye = [new Eye(actor.Position, WorldState.FutureTime(9.9d), inverted: actor.OID == (uint)OID.StatueYellowEye)];
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
