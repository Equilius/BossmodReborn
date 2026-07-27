namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO come up with a better way of storing data positions
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
