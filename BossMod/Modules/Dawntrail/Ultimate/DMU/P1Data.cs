namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO come up with a better way of storing data positions
// TODO function should be able to just pull data positions on strategy so no if loops inside module code - design pattern?

// TODO consider adding a way for pre-positions + actual positions to have a timer with them if needed
//  default -> use the component timer
//  timer while pulling the coordinate use that instead, could be helpful with pre-positions

// TODO add pre-position to the config - players may not like the pre-positions could be annoying
// TODO double check over pre-position positions

// TODO check over the positions for melee uptime to ensure they keep within range

// TODO add AI settings for pre-positions spots
// TODO add AI settings for P1GravenImage1 of where support roles go -> this will change the safe spots so most likely not needed

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
