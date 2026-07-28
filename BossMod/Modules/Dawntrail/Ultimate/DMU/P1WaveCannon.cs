namespace BossMod.Dawntrail.Ultimate.DMU;

// Knockback
//  if buff: ensure behind everyone in terms of x coord
//  if non-buff: ensure in front of stack player in terms of x coord
//  this only works for first one, other ones will need a custom one as well

// TODO what happens if the party roles is not configured?

// TODO figure out activation time for baits
// TODO this is just baitAwayEveryone with activation time available
// Place left-most position slightly up, like 4.0f
sealed class WaveCannon : Components.GenericBaitAway {
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public WaveCannon(BossModule module) : base(module) {
        var source = module.Enemies((uint)OID.StatueWaveCannon).FirstOrDefault();
        var shape = new AOEShapeRect(100.0f, 3.0f);

        if (source != null) {
            var party = Raid.WithoutSlot(false);
            var len = party.Length;
            foreach (var player in party) {
                CurrentBaits.Add(new(source, player, shape));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.WaveCannon) {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        var myAssignment = dmuConfig.P1WaveCannonAssignment[assignment];

        // TODO these seem correct roughly, need just need to figure out a way to create these by an equation
        //  Then make the player aim for this spot with a decent radius for them
        //  And make the players avoid each other still with the baits aoes -> these should turn on at 1.0 seconds or less otherwise the map
        //      will instantly just become a large yellow block
        Arena.AddCircle(new WPos(82.0f, 94.0f), 1.0f, Colors.Safe, 1.0f);
        Arena.AddCircle(new WPos(86.5f, 96.0f), 1.0f, Colors.Safe, 1.0f);
        Arena.AddCircle(new WPos(91.0f, 98.0f), 1.0f, Colors.Safe, 1.0f);
        Arena.AddCircle(new WPos(95.5f, 100.0f), 1.0f, Colors.Safe, 1.0f);

        Arena.AddCircle(new WPos(104.5f, 100.0f), 1.0f, Colors.Safe, 1.0f);
        Arena.AddCircle(new WPos(109.0f, 98.0f), 1.0f, Colors.Safe, 1.0f);
        Arena.AddCircle(new WPos(113.5f, 96.0f), 1.0f, Colors.Safe, 1.0f);
        Arena.AddCircle(new WPos(118.0f, 94.0f), 1.0f, Colors.Safe, 1.0f);

        // 120
    }
}
