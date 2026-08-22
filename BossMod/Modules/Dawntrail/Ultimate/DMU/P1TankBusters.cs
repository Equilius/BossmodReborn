namespace BossMod.Dawntrail.Ultimate.DMU;

static class NorthAIHints {
    private const float maxMelee = 2.5f;

    public static void AddTruthNorthHint(BossModule module, AIHints hints, float radius = 5.0f) {
        var hitBox = module.PrimaryActor.HitboxRadius;
        var outerHitBox = hitBox + maxMelee;
        hints.GoalZones.Add(p => p.InDonutCone(module.PrimaryActor.Position, hitBox, outerHitBox, Angle.AnglesCardinals[2],
            new Angle(MathF.Atan2(radius, outerHitBox))) ? 100.0f : 0.0f);
    }
}

sealed class RevoltingRuinIIIFirst(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(100.0f, 60.0f.Degrees()), (uint)IconID.TankBuster,
    (uint)AID.RevoltingRuinIIIFirstHit, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster) {
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (dmuConfig.P1RevoltingRuinIIIAlwaysAroundTrueNorth && IsBaitTarget(actor)) {
            NorthAIHints.AddTruthNorthHint(Module, hints);
        }
    }
}

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
                NorthAIHints.AddTruthNorthHint(Module, hints);
            }
        }
    }
}

sealed class HyperDrive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Hyperdrive, centerAtTarget: true, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster) {
    private DateTime activation;
    private readonly AOEShapeCircle shape = new(5.0f);
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.LightOfJudgment) {
            activation = Module.CastFinishAt(spell, 3.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Hyperdrive) {
            NumCasts++;

            if (NumCasts >= 3) {
                activation = default;
            }
        }
    }

    public override void Update() {
        CurrentBaits.Clear();

        if (activation == default) {
            return;
        }

        var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (target != null) {
            CurrentBaits.Add(new(Module.PrimaryActor, target, shape, activation));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (dmuConfig.P1HyperDriveAlwaysAroundTrueNorth && IsBaitTarget(actor)) {
            NorthAIHints.AddTruthNorthHint(Module, hints, 1.5f);
        }
    }
}
