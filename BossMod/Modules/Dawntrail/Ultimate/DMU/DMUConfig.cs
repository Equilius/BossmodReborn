namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
[SkipLocalsInit]
public sealed class DMUConfig : ConfigNode {

    // Strategy settings
    // TODO maybe this should be off by default?
    [PropertyDisplay("Module warning if party roles assignment are not configured!", tooltip: "The warning will only be displayed before combat starts")]
    public bool partyRolesWarning = true;

    public enum P1GravenImage1Strategy {
        [PropertyDisplay("None")]
        GravenImage1None,

        [PropertyDisplay("LPDU Graven Image 1")]
        GravenImage1LPDU,
    }

    [PropertyDisplay("P1 Graven Image 1 strategy")]
    public P1GravenImage1Strategy P1GravenImage1 = P1GravenImage1Strategy.GravenImage1None;

    [PropertyDisplay("P1 Wave Cannon conga priority", tooltip: "Conga order is West to East")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1WaveCannonAssignment = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    [PropertyDisplay("P1 Wave Cannon safe spot hints", tooltip: "Will display roughly where to stand for your bait")]
    public bool P1WaveCannonHints = false;

    [PropertyDisplay("P1 DoubleTrouble Knockback safe spot hints", tooltip: "Will display roughly where to stand for the stack")]
    public bool P1DoubleTroubleKnockbackHints = false;

    public enum P1GravenImage2Strategy {
        [PropertyDisplay("Normal Graven Image 2")]
        GravenImage2Normal,

        [PropertyDisplay("Uptime Graven Image 2")]
        GravenImage2Uptime,
    }

    [PropertyDisplay("P1 Graven Image 2 strategy")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2Uptime;

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("Modified Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky arrow CW box (Merry Go Round)")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 TeleTrouncing strategy")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Modified_Xolo;

    [PropertyDisplay("P1 Graven Image 3 Static Spots")]
    public bool P1GravenImage3Static = true;

    public enum P2ForsakenStrategy {
        [PropertyDisplay("EU meow braindead strategy using markerless")]
        Meow_Markerless,

        [PropertyDisplay("EU meow braindead strategy using DN ZENITH markers")]
        Meow_DN_ZENITH_Markers,

        [PropertyDisplay("NA Kroxy-Rinon (341 Melee Flex)")]
        Kroxy_Rinon_Melee_Flex,
    }

    [PropertyDisplay("P2 Forsaken strategy")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;

    // AI Settings
    [PropertyDisplay("P1 RevoltingRuinIII always around true north?", tooltip: "Only used by AI")]
    public bool P1RevoltingRuinIIIAlwaysAroundTrueNorth = true;

    // Debug Settings
    [PropertyDisplay("P1 Graven Image 1 knockback additional hints", tooltip: "Only used for debugging - H1 & R1 roles will still have to move a fair amount " +
                                                                              " if north is safe, other roles will only have to do slight adjustments")]
    public bool P1GravenImage1KnockbackAdditionalHints = false;
}
