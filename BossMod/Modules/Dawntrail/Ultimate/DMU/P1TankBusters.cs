namespace BossMod.Dawntrail.Ultimate.DMU;

static class NorthAIHints {
    private const float maxMelee = 2.5f;

    public static void AddForbiddenZone(BossModule module, AIHints hints, float radius, DateTime activation) {
        var hitBox = module.PrimaryActor.HitboxRadius;
        var outerHitBox = hitBox + maxMelee;

        hints.AddForbiddenZone(new SDInvertedDonutSector(module.PrimaryActor.Position, hitBox, outerHitBox, Angle.AnglesCardinals[2],
            new Angle(MathF.Atan2(radius, outerHitBox))), activation);
    }

    public static void AddGoalZone(BossModule module, AIHints hints, float radius) {
        var hitBox = module.PrimaryActor.HitboxRadius;
        var outerHitBox = hitBox + maxMelee;

        hints.GoalZones.Add(p => p.InDonutCone(module.PrimaryActor.Position, hitBox, outerHitBox, Angle.AnglesCardinals[2],
            new Angle(MathF.Atan2(radius, outerHitBox))) ? PositionWeights.PRE_POSITION : 0.0f);
    }
}

sealed class RevoltingRuinIIIFirst : Components.BaitAwayIcon {
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public RevoltingRuinIIIFirst(BossModule module) : base(module, new AOEShapeCone(100.0f, 60.0f.Degrees()), (uint)IconID.TankBuster,
        (uint)AID.RevoltingRuinIIIFirstHit, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster) {
        EnableHints = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (CurrentBaits.Count != 0 && EnableHints) {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        if (!dmuConfig.P1RevoltingRuinIIIAlwaysAroundTrueNorth) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        // Movement for the tank that is taking the tank buster
        if (assignment == (dmuConfig.P1RevoltingRuinIIIBait1OT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT)) {
            NorthAIHints.AddGoalZone(Module, hints, PositionAIRadius.SEMI_PRECISE);
            return;
        }

        // Backup since the tank that is expected to take it might be dead, so I guess it should attempt to adjust for it. The adjustment will only be for the
        // other tank and not any other roles to prevent other roles movement looking odd (other roles should be surprised to have gotten the TB)
        if (IsBaitTarget(actor) && CurrentBaits.Count > 0 && actor.Role == Role.Tank) {
            NorthAIHints.AddForbiddenZone(Module, hints, PositionAIRadius.SEMI_PRECISE, CurrentBaits[0].Activation);
            return;
        }

        // Movement for everyone else (including the tank who is not taking the tank buster)
        if (CurrentBaits.Count > 0) {
            hints.AddForbiddenZone(new SDDonutSector(Module.PrimaryActor.Position, 0.0f, 20.0f, Angle.AnglesCardinals[2], 90.0f.Degrees()),
                CurrentBaits[0].Activation);
            return;
        }

        // This forbidden zone is always 2.0f so the pathfinder can see other current aoes that are active to avoid walking into them such as puddles
        hints.AddForbiddenZone(new SDDonutSector(Module.PrimaryActor.Position, 0.0f, 20.0f, Angle.AnglesCardinals[2], 90.0f.Degrees()),
            WorldState.FutureTime(2.0f));
    }
}

// The 2nd tankbuster is separated into its own component as we want to show it on the radar but remove the hints until it actually becomes active
sealed class RevoltingRuinIIISecond : Components.GenericBaitAway {
    private Actor? source;
    private DateTime activation;
    private AOEShape shape = new AOEShapeCone(100.0f, 60.0f.Degrees());
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

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
        if (!EnableHints) {
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);

        if (!dmuConfig.P1RevoltingRuinIIIAlwaysAroundTrueNorth) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        // Movement for the tank that is taking the tank buster
        if (assignment == (dmuConfig.P1RevoltingRuinIIIBait1OT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT)) {
            NorthAIHints.AddGoalZone(Module, hints, PositionAIRadius.SEMI_PRECISE);
            return;
        }

        // Backup since the tank that is expected to take it might be dead, so I guess it should attempt to adjust for it. The adjustment will only be for the
        // other tank and not any other roles to prevent other roles movement looking odd (other roles should be surprised to have gotten the TB)
        if (IsBaitTarget(actor) && CurrentBaits.Count > 0 && actor.Role == Role.Tank) {
            NorthAIHints.AddForbiddenZone(Module, hints, PositionAIRadius.SEMI_PRECISE, CurrentBaits[0].Activation);
            return;
        }

        // Movement for everyone else (including the tank who is not taking the tank buster)
        hints.AddForbiddenZone(new SDDonutSector(Module.PrimaryActor.Position, 0.0f, 20.0f, Angle.AnglesCardinals[2], 90.0f.Degrees()));
    }
}

sealed class HyperDrive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Hyperdrive, centerAtTarget: true, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster) {
    private DateTime activation;
    private readonly AOEShapeCircle shape = new(5.0f);
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

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

        if (!dmuConfig.P1HyperDriveAlwaysAroundTrueNorth) {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        // Movement for the tank that is taking the tank buster
        if (assignment == (dmuConfig.P1HyperDriveBait1OT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT)) {
            NorthAIHints.AddGoalZone(Module, hints, PositionAIRadius.SEMI_PRECISE);
            return;
        }

        // Backup since the tank that is expected to take it might be dead, so I guess it should attempt to adjust for it. The adjustment will only be for the
        // other tank and not any other roles to prevent other roles movement looking odd (other roles should be surprised to have gotten the TB)
        if (IsBaitTarget(actor) && CurrentBaits.Count > 0 && actor.Role == Role.Tank) {
            NorthAIHints.AddForbiddenZone(Module, hints, PositionAIRadius.SEMI_PRECISE, CurrentBaits[0].Activation);
            return;
        }

        // Movement for everyone else (including the tank who is not taking the tank buster)
        hints.AddForbiddenZone(new SDDonutSector(Module.PrimaryActor.Position, 0.0f, 20.0f, Angle.AnglesCardinals[2], 90.0f.Degrees()));
    }
}
