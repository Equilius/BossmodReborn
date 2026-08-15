namespace BossMod.Dawntrail.Ultimate.DMU;

// Knockback
//  if buff: ensure behind everyone in terms of x coord
//  if non-buff: ensure in front of stack player in terms of x coord
//  this only works for first one, other ones will need a custom one as well

// TODO consider adding a config option where the stack player will adjust if needed, e.g. a player is slightly off - unsure tho could cause problems
//  generally every strat does it the same way
// TODO consider adding an option for safety check if player is behind?
//  Will need to ensure it will not move too far back to make the other players in the stack miss it by moving backwards
//  ACTUALLY -> it would be easier for the players without the debuff to just self-check themselves instead as then it will just resolve on them instead
//      of potentially affecting the whole group of that side

// TODO disable blizzard + lightning safespots until knockback has resolved -> should have enough time to move -> most likely can just be the default version of them
sealed class DoubleTroubleTrapStacks(BossModule module) : Components.UniformStackSpread(module, 6.0f, 0.0f, 4, 4) {
    public bool active = false;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DoubleTroubleTrap) {
            var party = Raid.WithSlot(true, true, true);
            BitMask allowedSupports = default;
            BitMask allowedDDs = default;

            for (var i = 0; i < party.Length; ++i) {
                ref var p = ref party[i];

                if (p.Item2.Role is Role.Tank or Role.Healer) {
                    allowedSupports.Set(p.Item1);
                }

                if (p.Item2.Role is Role.Melee or Role.Ranged) {
                    allowedDDs.Set(p.Item1);
                }
            }

            if (actor.Role is Role.Tank or Role.Healer) {
                AddStack(actor, status.ExpireAt, ~allowedSupports);
            }

            if (actor.Role is Role.Melee or Role.Ranged) {
                AddStack(actor, status.ExpireAt, ~allowedDDs);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.DoubleTroubleTrap) {
            Stacks.RemoveAt(0);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (!active) {
            return;
        }

        base.DrawArenaForeground(pcSlot, pc);

        if (!dmuConfig.P1DoubleTroubleKnockbackHints) {
            return;
        }

        WPos safeSpot = default;

        if (IsStackTarget(pc)) {
            safeSpot = P1DoubleTroubleKnockBackData.StackSafeSpots.GetValueOrDefault(pc.Role).stackDebuff;
        }

        if (!IsStackTarget(pc)) {
            safeSpot = P1DoubleTroubleKnockBackData.StackSafeSpots.GetValueOrDefault(pc.Role).stackHelper;
        }


        if (safeSpot == default) {
            return;
        }

        Arena.ZoneCircleOutline(safeSpot, 1.0f, Colors.Safe);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!active) {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (!active) {
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) {
        if (!active) {
            return PlayerPriority.Irrelevant;
        }

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }
}

sealed class DoubleTroubleTrapKnockback(BossModule module) : Components.GenericKnockback(module) {
    private readonly List<Knockback> knockbacks = [];
    private readonly DoubleTroubleTrapStacks? doubleTroubleTrapStacks = module.FindComponent<DoubleTroubleTrapStacks>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        knockbacks.Clear();

        if (doubleTroubleTrapStacks == null || doubleTroubleTrapStacks.Stacks.Count == 0) {
            return [];
        }

        foreach (var stack in doubleTroubleTrapStacks.Stacks) {
            if (actor.Position.InCircle(stack.Target.Position, stack.Radius)) {
                knockbacks.Add(new(stack.Target.Position, 14f, stack.Activation));
            }
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}
