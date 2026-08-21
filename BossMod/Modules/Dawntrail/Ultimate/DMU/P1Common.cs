namespace BossMod.Dawntrail.Ultimate.DMU;

// The 2nd tankbuster is separated into its own component as we want to show it on the radar but remove the hints until it actually becomes active
sealed class RevoltingRuinIIISecond : Components.GenericBaitAway {
    private Actor? source;
    private DateTime activation;
    private AOEShape shape = new AOEShapeCone(100.0f, 60.0f.Degrees());
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public RevoltingRuinIIISecond(BossModule module) : base(module, (uint)AID.RevoltingRuinIIISecondHit, true, true, true, false,
        AIHints.PredictedDamageType.Tankbuster) {
        EnableHints = false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIIIFirstHit) {
            source = caster;
            activation = Module.CastFinishAt(spell, 3.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIIIFirstHit) {
            EnableHints = true;
        }

        if (spell.Action.ID == (uint)AID.RevoltingRuinIIISecondHit) {
            source = null;
            NumCasts++;
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (source == null) {
            return;
        }

        var byEnmity = RaidByEnmity(source);
        var target = byEnmity.Count > 1 ? byEnmity[1] : null;

        if (target != null) {
            CurrentBaits.Add(new(source, target, shape, activation));
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        if (EnableHints) {
            base.AddGlobalHints(hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (EnableHints) {
            base.AddAIHints(slot, actor, assignment, hints);

            if (dmuConfig.P1RevoltingRuinIIIAlwaysAroundTrueNorth && IsBaitTarget(actor)) {
                RevoltingRuinIIIAIHints.AddTruthNorthHint(Module, hints);
            }
        }
    }
}

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
