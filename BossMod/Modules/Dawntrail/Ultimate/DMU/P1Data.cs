namespace BossMod.Dawntrail.Ultimate.DMU;

// Used for weights for the pathfinder
public static class PositionWeights {
    public const float PRE_POSITION = 5.0f; // 1.5f to make the player move to the spot, but allow for positionals still
    public const float MECHANIC = 100.0f;
}

// Used for green circle on the foreground / background
public static class PositionDrawSize {
    public const float PRECISE = 0.75f;
    public const float NORMAL = 1.0f;
}

// Used for the radius the player can stand around that point given
public static class PositionAIRadius {
    public const float SUPER_PRECISE = 0.75f;
    public const float PRECISE = 1.0f;
    public const float SEMI_PRECISE = 1.5f;
    public const float NORMAL = 2.0f;
    public const float GENERAL = 5.0f;
}

public class P1GravenImage1Data {
    public static IReadOnlyDictionary<Role, (WPos tethered, WPos normal)> PulseWavePrePositions => pulseWavePrePositions;
    public static IReadOnlyDictionary<PartyRolesConfig.Assignment, (WPos north, WPos south)> SpreadSafeSpots => spreadSafeSpots;
    public static IReadOnlyDictionary<Role, (WPos north, WPos south)> StackSafeSpots => stackSafeSpots;

    private static readonly Dictionary<Role, (WPos tethered, WPos normal)> pulseWavePrePositions = new() {
        // Melee stand slightly inside the hitbox
        [Role.Melee] = (new WPos(104.000f, 93.000f), new WPos(104.000f, 100.000f)),
        [Role.Tank] = (new WPos(96.000f, 93.000f), new WPos(96.000f, 100.000f)),

        // Range stand on the hitbox
        [Role.Ranged] = (new WPos(106.000f, 93.000f), new WPos(106.000f, 100.000f)),
        [Role.Healer] = (new WPos(94.000f, 93.000f), new WPos(94.000f, 100.000f)),
    };

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
    public static readonly WPos[] safeSpots = [
        new WPos(81.0f, 97.0f),
        new WPos(87.0f, 98.0f),
        new WPos(92.0f, 99.0f),
        new WPos(97.0f, 100.0f),
        new WPos(103.0f, 100.0f),
        new WPos(108.0f, 99.0f),
        new WPos(113.0f, 98.0f),
        new WPos(119.0f, 97.0f),
    ];
}

public class P1DoubleTroubleKnockBackData {
    public static IReadOnlyDictionary<Role, (WPos stackDebuff, WPos stackHelper)> StackSafeSpots => stackSafeSpots;

    private static readonly Dictionary<Role, (WPos stackDebuff, WPos stackHelper)> stackSafeSpots = new() {
        [Role.Melee] = (new WPos(108.000f, 100.000f), new  WPos(105.000f, 100.000f)),
        [Role.Ranged] = (new WPos(108.000f, 100.000f), new  WPos(105.000f, 100.000f)),
        [Role.Healer] = (new WPos(92.000f, 100.000f), new  WPos(95.000f, 100.000f)),
        [Role.Tank] = (new WPos(92.000f, 100.000f), new  WPos(95.000f, 100.000f)),
    };
}

public class P1GravitasData {
    public enum Side { NONE, NORTH, SOUTH } // TODO move to component class?

    public static IReadOnlyDictionary<Side, WPos> PuddlesSpots => puddleSpots;
    public static IReadOnlyDictionary<PartyRolesConfig.Assignment, (WPos north, WPos south)> SpreadSafeSpots => spreadSafeSpots;

    private static readonly Dictionary<Side, WPos> puddleSpots = new() {
        [Side.NORTH] = new WPos(100.000f, 91.5f),
        [Side.SOUTH] = new WPos(100.000f, 108.5f),
    };

    private static readonly Dictionary<PartyRolesConfig.Assignment, (WPos north, WPos south)> spreadSafeSpots = new() {
        [PartyRolesConfig.Assignment.M1] = (new WPos(108.500f, 100.000f), new WPos(91.500f, 100.000f)),
        [PartyRolesConfig.Assignment.MT] = (new WPos(108.500f, 100.000f), new WPos(91.500f, 100.000f)),
        [PartyRolesConfig.Assignment.M2] = (new WPos(91.500f, 100.000f), new WPos(108.500f, 100.000f)),
        [PartyRolesConfig.Assignment.OT] = (new WPos(91.500f, 100.000f), new WPos(108.500f, 100.000f)),
        [PartyRolesConfig.Assignment.R1] = (new WPos(113.500f, 87.500f), new WPos(86.500f, 112.500f)),
        [PartyRolesConfig.Assignment.H1] = (new WPos(113.500f, 87.500f), new WPos(86.500f, 112.500f)),
        [PartyRolesConfig.Assignment.R2] = (new WPos(86.500f, 87.500f), new WPos(113.500f, 112.500f)),
        [PartyRolesConfig.Assignment.H2] = (new WPos(86.500f, 87.500f), new WPos(113.500f, 112.500f)),
    };
}

public class P1TeleTrouncingData {
    public enum Direction { NONE, UP, DOWN, LEFT, RIGHT }
    public readonly record struct arrowSpot(Direction direction, WPos safeSpot);
    public readonly record struct arrowPair(arrowSpot arrow1, arrowSpot arrow2);
    private static (Direction, Direction) Normalize(Direction a, Direction b) => a <= b ? (a, b) : (b, a);

    public static bool TryGetSafeSpots(DMUConfig.P1TeleTrouncingStrategy strategy, Direction a, Direction b, out arrowPair pair) {
        var strategyPicked = strategy switch {
            DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow => freakyArrowSafeSpots,
            DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo => modifiedXoloSafeSpots,
            _ => null
        };

        if (strategyPicked == null) {
            pair = default;
            return false;
        }

        return strategyPicked.TryGetValue(Normalize(a, b), out pair);
    }

    private static readonly Dictionary<(Direction, Direction), arrowPair> freakyArrowSafeSpots = new() {
        [Normalize(Direction.DOWN, Direction.DOWN)] = new(new(Direction.DOWN, new WPos(112.000f, 94.000f)), new(Direction.DOWN, new WPos(112.000f, 100.000f))),
        [Normalize(Direction.LEFT, Direction.LEFT)] = new(new(Direction.LEFT, new WPos(106.000f, 112.000f)), new(Direction.LEFT, new WPos(100.000f, 112.000f))),
        [Normalize(Direction.UP, Direction.UP)] = new(new(Direction.UP, new WPos(88.000f, 106.000f)), new(Direction.UP, new WPos(88.000f, 100.000f))),
        [Normalize(Direction.RIGHT, Direction.RIGHT)] = new(new(Direction.RIGHT, new WPos(94.000f, 88.000f)), new(Direction.RIGHT, new WPos(100.000f, 88.000f))),
        [Normalize(Direction.UP, Direction.LEFT)] = new(new(Direction.UP, new WPos(88.000f, 112.000f)), new(Direction.LEFT, new WPos(94.000f, 112.000f))),
        [Normalize(Direction.UP, Direction.RIGHT)] = new(new(Direction.UP, new WPos(88.000f, 94.000f)), new(Direction.RIGHT, new WPos(88.000f, 88.000f))),
        [Normalize(Direction.DOWN, Direction.RIGHT)] = new(new(Direction.DOWN, new WPos(112.000f, 88.000f)), new(Direction.RIGHT, new WPos(106.000f, 88.000f))),
        [Normalize(Direction.DOWN, Direction.LEFT)] = new(new(Direction.DOWN, new WPos(112.000f, 106.000f)), new(Direction.LEFT, new WPos(112.000f, 112.000f))),
    };

    private static readonly Dictionary<(Direction, Direction), arrowPair> modifiedXoloSafeSpots = new() {
        [Normalize(Direction.DOWN, Direction.DOWN)] = new(new(Direction.DOWN, new WPos(87.750f, 88.030f)), new(Direction.DOWN, new WPos(87.750f, 93.570f))),
        [Normalize(Direction.LEFT, Direction.LEFT)] = new(new(Direction.LEFT, new WPos(112.135f, 87.993f)), new(Direction.LEFT, new WPos(106.579f, 87.922f))),
        [Normalize(Direction.UP, Direction.UP)] = new(new(Direction.UP, new WPos(111.989f, 112.003f)), new(Direction.UP, new WPos(112.125f, 106.306f))),
        [Normalize(Direction.RIGHT, Direction.RIGHT)] = new(new(Direction.RIGHT, new WPos(88.069f, 112.037f)), new(Direction.RIGHT, new WPos(93.798f, 112.161f))),
        [Normalize(Direction.UP, Direction.LEFT)] = new(new(Direction.UP, new WPos(93.781f, 93.593f)), new(Direction.LEFT, new WPos(93.576f, 88.051f))),
        [Normalize(Direction.UP, Direction.RIGHT)] = new(new(Direction.UP, new WPos(111.955f, 93.877f)), new(Direction.RIGHT, new WPos(106.422f, 93.756f))),
        [Normalize(Direction.DOWN, Direction.RIGHT)] = new(new(Direction.DOWN, new WPos(106.413f, 106.444f)), new(Direction.RIGHT, new WPos(106.337f, 112.135f))),
        [Normalize(Direction.DOWN, Direction.LEFT)] = new(new(Direction.DOWN, new WPos(88.103f, 106.377f)), new(Direction.LEFT, new WPos(93.685f, 106.316f))),
    };
}
