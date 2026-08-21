namespace BossMod.Dawntrail.Ultimate.DMU;

static class RevoltingRuinIIIAIHints {
    public static void AddTruthNorthHint(BossModule module, AIHints hints) {
        var maxMelee = 2.5f;
        var cleaveNudge = 10.0f; // The amount the player is allowed to move left and right of the north position

        var hitBox = module.PrimaryActor.HitboxRadius;
        var outerHitBox = hitBox + maxMelee;
        hints.GoalZones.Add(p => p.InDonutCone(module.PrimaryActor.Position, hitBox, outerHitBox, Angle.AnglesCardinals[2],
            new Angle(MathF.Atan2(cleaveNudge, outerHitBox))) ? 100.0f : 0.0f);
    }
}

sealed class RevoltingRuinIIIFirst(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(100.0f, 60.0f.Degrees()), (uint)IconID.TankBuster,
    (uint)AID.RevoltingRuinIIIFirstHit, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster) {
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (dmuConfig.P1RevoltingRuinIIIAlwaysAroundTrueNorth && IsBaitTarget(actor)) {
            RevoltingRuinIIIAIHints.AddTruthNorthHint(Module, hints);
        }
    }
}

sealed class HyperDrive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Hyperdrive, centerAtTarget: true, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster) {
    private DateTime activation;
    private readonly AOEShapeCircle shape = new(5.0f);

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

        if (IsBaitTarget(actor)) {
            RevoltingRuinIIIAIHints.AddTruthNorthHint(Module, hints);
        }
    }
}
