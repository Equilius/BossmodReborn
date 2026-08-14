namespace BossMod.Dawntrail.Ultimate.DMU;

// Knockback
//  if buff: ensure behind everyone in terms of x coord
//  if non-buff: ensure in front of stack player in terms of x coord
//  this only works for first one, other ones will need a custom one as well

sealed class WaveCannon : Components.BaitAwayEveryone {
    private readonly DateTime activation;
    private const float baitActivation = 4.0f;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public WaveCannon(BossModule module) : base(module, module.Enemies((uint)OID.StatueWaveCannon).FirstOrDefault(), new AOEShapeRect(100.0f, 3.0f)) {
        activation = WorldState.FutureTime(baitActivation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.WaveCannon) {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (CurrentBaits.Count == 0) {
            return;
        }

        base.DrawArenaForeground(pcSlot, pc);

        if (!dmuConfig.P1WaveCannonHints) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];
        var myAssignment = (PartyRolesConfig.Assignment)dmuConfig.P1WaveCannonAssignment[assignment];
        var safeSpot = P1WaveCannonData.Safespots.GetValueOrDefault(myAssignment);

        if (safeSpot == default) {
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, 0.75f, Colors.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = CurrentBaits.Count;
        if (count == 0) {
            return;
        }

        var remaining = (activation - WorldState.CurrentTime).TotalSeconds;
        if (remaining <= 1.0f) {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        var safeSpot = P1WaveCannonData.Safespots.GetValueOrDefault(assignment);
        if (safeSpot == default) {
            return;
        }

        hints.GoalZones.Add(AIHints.GoalProximity(safeSpot, 1.0f, 50.0f));
    }
}
