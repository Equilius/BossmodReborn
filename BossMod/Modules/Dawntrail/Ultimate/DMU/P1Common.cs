namespace BossMod.Dawntrail.Ultimate.DMU;

class BlizzardIIIBlowout(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlizzardIIIBlowout, (uint)AID.BlizzardIIIBlowout1],
    new AOEShapeCone(40f, 45f.Degrees())) {
    public bool enabledHints = false;

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (enabledHints) {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (enabledHints) {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

class LightningSafeSpots(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThrummingThunderIII, (uint)AID.ThrummingThunderIII1],
    new AOEShapeRect(40.0f, 5.0f)) {
    public bool enabledHints = false;

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (enabledHints) {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (enabledHints) {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class LightOfJudgment(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightOfJudgment);
