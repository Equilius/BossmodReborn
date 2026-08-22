namespace BossMod.Dawntrail.Ultimate.DMU;

// Used for weights for the pathfinder
public static class PositionWeights {
    public const float PRE_POSITION = 1.5f; // 1.5f to make the player move to the spot, but allow for positionals still
}

// Used for green circle on the foreground / background
public static class PositionDrawSize {
    public const float PRECISE = 0.75f;
    public const float NORMAL = 1.0f;
}

// Used for the radius the player can stand around that point given
public static class PositionAIRadius {
    public const float PRECISE = 1.0f;
    public const float SEMI_PRECISE = 1.5f;
    public const float NORMAL = 2.0f;
    public const float GENERAL = 5.0f;
}

public class P1GravenImage1Data {
    public static IReadOnlyDictionary<PartyRolesConfig.Assignment, (WPos north, WPos south)> SpreadSafeSpots => spreadSafeSpots;
    public static IReadOnlyDictionary<Role, (WPos north, WPos south)> StackSafeSpots => stackSafeSpots;

    private static readonly Dictionary<PartyRolesConfig.Assignment, (WPos north, WPos south)> spreadSafeSpots = new() {
        [PartyRolesConfig.Assignment.M1] = (new WPos(101.000f, 93.500f), new WPos(101.000f, 106.500f)),
        [PartyRolesConfig.Assignment.M2] = (new WPos(106.500f, 99.000f), new WPos(106.500f, 101.000f)),
        [PartyRolesConfig.Assignment.R1] = (new WPos(111.000f, 89.000f), new WPos(111.000f, 111.000f)),
        [PartyRolesConfig.Assignment.R2] = (new WPos(119.000f, 99.000f), new WPos(119.000f, 101.000f)),
        [PartyRolesConfig.Assignment.MT] = (new WPos(99.000f, 93.500f), new WPos(99.000f, 106.500f)),
        [PartyRolesConfig.Assignment.OT] = (new WPos(93.500f, 99.000f), new WPos(93.500f, 101.000f)),
        [PartyRolesConfig.Assignment.H1] = (new WPos(89.000f, 89.000f), new WPos(89.000f, 111.000f)),
        [PartyRolesConfig.Assignment.H2] = (new WPos(81.000f, 99.000f), new WPos(81.000f, 101.000f)),
    };

    private static readonly Dictionary<Role, (WPos north, WPos south)> stackSafeSpots = new() {
        [Role.Melee] = (new WPos(105.000f, 96.000f), new  WPos(105.000f, 104.000f)),
        [Role.Ranged] = (new WPos(105.000f, 96.000f), new  WPos(105.000f, 104.000f)),
        [Role.Tank] = (new WPos(95.000f, 96.000f), new WPos(95.000f, 104.000f)),
        [Role.Healer] = (new WPos(95.000f, 96.000f), new WPos(95.000f, 104.000f)),
    };
}

public class P1WaveCannonData {
    public static IReadOnlyDictionary<PartyRolesConfig.Assignment, WPos> Safespots => safeSpots;

    private static readonly Dictionary<PartyRolesConfig.Assignment, WPos> safeSpots = new() {
        [PartyRolesConfig.Assignment.H1] = new WPos(87.0f, 98.0f),
        [PartyRolesConfig.Assignment.H2] = new WPos(81.0f, 97.0f),
        [PartyRolesConfig.Assignment.OT] = new WPos(92.0f, 99.0f),
        [PartyRolesConfig.Assignment.MT] = new WPos(97.0f, 100.0f),
        [PartyRolesConfig.Assignment.M1] = new WPos(103.0f, 100.0f),
        [PartyRolesConfig.Assignment.M2] = new WPos(108.0f, 99.0f),
        [PartyRolesConfig.Assignment.R1] = new WPos(113.0f, 98.0f),
        [PartyRolesConfig.Assignment.R2] = new WPos(119.0f, 97.0f),
    };
}

public class P1DoubleTroubleKnockBackData {
    public static IReadOnlyDictionary<Role, (WPos stackDebuff, WPos stackHelper)> StackSafeSpots => stackSafeSpots;

    private static readonly Dictionary<Role, (WPos stackDebuff, WPos stackHelper)> stackSafeSpots = new() {
        [Role.Melee] = (new WPos(107.500f, 100.000f), new  WPos(105.000f, 100.000f)),
        [Role.Ranged] = (new WPos(107.500f, 100.000f), new  WPos(105.000f, 100.000f)),
        [Role.Healer] = (new WPos(92.500f, 100.000f), new  WPos(95.000f, 100.000f)),
        [Role.Tank] = (new WPos(92.500f, 100.000f), new  WPos(95.000f, 100.000f)),
    };
}




