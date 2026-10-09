namespace BossMod.Dawntrail.Ultimate.DMU;

using Direction = P1TeleTrouncingData.Direction;

// TODO add stack arena for puddles - currently you can actually take either, it just assumes you will take the one always closer to you
// Might need to setup something up for this

/*
Keep gravenImage 3 static and non-static spot option
 */

// TODO
//  1. Setup specific strat - check it works for modified xolo
//  2. Setup config so you can pick no strat - make the default freaky arrow
//  2.1 Setup so it just returns if no strategy is selected for drawning and stuff

// TODO push work
// TODO setup knockback part
// TODO setup gravenImage 3 static and non-static spot
// TODO final mechanic - pre-position and solve it
// TODO try and setup the plugin as a fork so other people can use it locally

// TODO fixes:
//  1. Tank busters - send players south, some tanks are strange and will angle the tank buster at around East and West which can kill the player
//      shouldn't happen, but can, so maintain the player keeping them south of a donut shape around the boss to potentially
//      still do positional on the back if needed
//  2. Gravitas puddles - move puddle baits slightly up so its more precise - just change the position of the forbidden zone to be more precise, I think
//      its currently 2 squares long when it should just be one
//  3. Gravitas stacks are the wrong colour - sometimes green, sometimes yellow
//  4. REFER to comment at top of page - knockback 2 where taking puddles, make a bitmask for which puddle zone they should take and get the midpoint
//      of the puddles while keeping in melee range if possible to ensure we take all the puddles
//  5. For double trouble 2 change the timer from 1.2 seconds to 0.9 seconds and see if it improves it

sealed class TeleTrouncing : Components.GenericAOEs {
    private readonly record struct arrowDebuff(Direction direction, DateTime activation);
    private readonly List<List<arrowDebuff>> debuffs = [];
    private readonly List<List<(WPos safeSpot, DateTime activation)>> arrowHints = [];
    private readonly List<AOEInstance> arrows = [];
    private readonly AOEShapeCircle shape = new(2.0f);
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public TeleTrouncing(BossModule module) : base(module) {
        for (int i = 0; i < PartyState.MaxPartySize; i++) {
            debuffs.Add([
                new arrowDebuff(),
                new arrowDebuff()
            ]);

            arrowHints.Add([]);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        Direction direction = (SID)status.ID switch {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => Direction.NONE
        };

        if (direction == Direction.NONE) {
            return;
        }

        if (Raid.FindSlot(actor.InstanceID) is var slot && slot < 0) {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8) {
            debuffs[slot][1] = new arrowDebuff(direction, status.ExpireAt);
        } else {
            debuffs[slot][0] = new arrowDebuff(direction, status.ExpireAt);
        }

        var firstArrow = debuffs[slot][0];
        var secondArrow = debuffs[slot][1];

        if (firstArrow.direction == Direction.NONE || secondArrow.direction == Direction.NONE) {
            return;
        }

        if (!P1TeleTrouncingData.TryGetSafeSpots(dmuConfig.P1TeleTrouncing, firstArrow.direction, secondArrow.direction, out var safeSpots)) {
            return;
        }

        if (safeSpots.arrow1.direction == firstArrow.direction) {
            arrowHints[slot].Add((safeSpots.arrow1.safeSpot, firstArrow.activation));
            arrowHints[slot].Add((safeSpots.arrow2.safeSpot, secondArrow.activation));
        } else {
            arrowHints[slot].Add((safeSpots.arrow2.safeSpot, firstArrow.activation));
            arrowHints[slot].Add((safeSpots.arrow1.safeSpot, secondArrow.activation));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        Direction direction = (SID)status.ID switch {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => Direction.NONE
        };

        if (direction == Direction.NONE) {
            return;
        }

        if (Raid.FindSlot(actor.InstanceID) is var slot && slot < 0) {
            return;
        }

        if (arrowHints[slot].Count != 0) {
            arrowHints[slot].RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1) {
            NumCasts++;
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.TeleTrouncingArrow) {
            arrows.Add(new(shape, actor.Position, actor.Rotation, actorID: actor.InstanceID));
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.TeleTrouncingArrow) {
            arrows.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (arrowHints[pcSlot].Count == 0) {
            return;
        }

        for (int i = 0; i < arrowHints[pcSlot].Count; i++) {
            Arena.ZoneCircleOutline(arrowHints[pcSlot][i].safeSpot, PositionDrawSize.NORMAL, i == 0 ? Colors.Safe : Colors.Danger, 2.0f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        base.DrawArenaBackground(pcSlot, pc);

        if (arrows.Count == 0) {
            return;
        }

        foreach (var arrow in arrows) {
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation + 90.Degrees()).ToDirection(), Colors.Background);
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation - 90.Degrees()).ToDirection(), Colors.Background);
            Arena.AddLine(arrow.Origin + arrow.Rotation.ToDirection(), arrow.Origin + (arrow.Rotation + 180.Degrees()).ToDirection(), Colors.Background);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (arrowHints[slot].Count == 0 || debuffs[slot].Count == 0) {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(arrowHints[slot][0].safeSpot, PositionAIRadius.PRECISE), arrowHints[slot][0].activation);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(arrows);
}

sealed class TeleTrouncingOLD(BossModule module) : BossComponent(module) {
    public int NumCasts = 0;
    private (Direction direction, DateTime activation)? Debuff1;
    private (Direction direction, DateTime activation)? Debuff2;
    private readonly List<WPos> hints = [];
    private enum Direction { UP, DOWN, LEFT, RIGHT }
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1) {
            NumCasts++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        var player = Raid.FindSlot(actor.InstanceID);
        if (player is not (>= 0 and PartyState.PlayerSlot)) {
            return;
        }

        Direction? dir = status.ID switch {
            (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2 => Direction.UP,
            (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2 => Direction.DOWN,
            (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2 => Direction.LEFT,
            (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => null
        };

        if (dir == null) {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8) {
            Debuff2 = (dir.Value, status.ExpireAt);
        } else {
            Debuff1 = (dir.Value, status.ExpireAt);
        }

        if (Debuff1 == null || Debuff2 == null) {
            return;
        }

        // Case 1: Both debuffs are in the same direction
        if (Debuff1.Value.direction == Debuff2.Value.direction) {
            if (Debuff1.Value.direction == Direction.DOWN) { // A waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                    hints.Add(new WPos(87.750f, 88.030f));
                    hints.Add(new WPos(87.750f, 93.570f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                    hints.Add(new WPos(112.000f, 94.000f));
                    hints.Add(new WPos(112.000f, 100.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.LEFT) { // B waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                    hints.Add(new WPos(112.135f, 87.993f));
                    hints.Add(new WPos(106.579f, 87.922f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                    hints.Add(new WPos(106.000f, 112.000f));
                    hints.Add(new WPos(100.000f, 112.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.UP) { // C waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                    hints.Add(new WPos(111.989f, 112.003f));
                    hints.Add(new WPos(112.125f, 106.306f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                    hints.Add(new WPos(88.000f, 106.000f));
                    hints.Add(new WPos(88.000f, 100.000f));
                }
            }

            if (Debuff1.Value.direction == Direction.RIGHT) { // D waymark
                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                    hints.Add(new WPos(88.069f, 112.037f));
                    hints.Add(new WPos(93.798f, 112.161f));
                }

                if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                    hints.Add(new WPos(94.000f, 88.000f));
                    hints.Add(new WPos(100.000f, 88.000f));
                }
            }

            return;
        }

        // Case 2: Both debuffs are in different directions
        var debuff1First = Debuff1.Value.activation <= Debuff2.Value.activation;

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.LEFT)) {

            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                if (upFirst) {
                    hints.Add(new WPos(93.781f, 93.593f)); // 1 waymark
                    hints.Add(new WPos(93.576f, 88.051f)); // non-waymark
                } else {
                    hints.Add(new WPos(93.576f, 88.051f)); // non-waymark
                    hints.Add(new WPos(93.781f, 93.593f)); // 1 waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                if (upFirst) {
                    hints.Add(new WPos(88.000f, 112.000f));
                    hints.Add(new WPos(94.000f, 112.000f));
                }else {
                    hints.Add(new WPos(94.000f, 112.000f));
                    hints.Add(new WPos(88.000f, 112.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.RIGHT)) {
            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                if (upFirst) {
                    hints.Add(new WPos(111.955f, 93.877f)); // non-waymark
                    hints.Add(new WPos(106.422f, 93.756f)); // 2 waymark
                } else {
                    hints.Add(new WPos(106.422f, 93.756f)); // 2 waymark
                    hints.Add(new WPos(111.955f, 93.877f)); // non-waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                if (upFirst) {
                    hints.Add(new WPos(88.000f, 94.000f));
                    hints.Add(new WPos(88.000f, 88.000f));
                } else {
                    hints.Add(new WPos(88.000f, 88.000f));
                    hints.Add(new WPos(88.000f, 94.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.RIGHT)) {
            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                if (downFirst) {
                    hints.Add(new WPos(106.413f, 106.444f)); // 3 waymark
                    hints.Add(new WPos(106.337f, 112.135f)); // 3 non-waymark
                } else {
                    hints.Add(new WPos(106.337f, 112.135f)); // 3 non-waymark
                    hints.Add(new WPos(106.413f, 106.444f)); // 3 waymark
                }
            }

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                if (downFirst) {
                    hints.Add(new WPos(112.000f, 88.000f));
                    hints.Add(new WPos(106.000f, 88.000f));
                } else {
                    hints.Add(new WPos(106.000f, 88.000f));
                    hints.Add(new WPos(112.000f, 88.000f));
                }
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.LEFT)) {
            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Modified_Xolo) {
                if (downFirst) {
                    hints.Add(new WPos(88.103f, 106.377f)); // 4 non-waymark
                    hints.Add(new WPos(93.685f, 106.316f)); // 4 waymark
                } else {
                    hints.Add(new WPos(93.685f, 106.316f)); // 4 waymark
                    hints.Add(new WPos(88.103f, 106.377f)); // 4 non-waymark
                }
            } else if (dmuConfig.P1TeleTrouncing == DMUConfig.P1TeleTrouncingStrategy.Freaky_Arrow) {
                if (downFirst) {
                    hints.Add(new WPos(112.000f, 106.000f));
                    hints.Add(new WPos(112.000f, 112.000f));
                } else {
                    hints.Add(new WPos(112.000f, 112.000f));
                    hints.Add(new WPos(112.000f, 106.000f));
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID is (uint)SID.TelePortentUP or (uint)SID.TelePortentUP2
            or (uint)SID.TelePortentDOWN or (uint)SID.TelePortentDOWN2
            or (uint)SID.TelePortentLEFT or (uint)SID.TelePortentLEFT2
            or (uint)SID.TelePortentRIGHT or (uint)SID.TelePortentRIGHT2)
        {

            var player = Raid.FindSlot(actor.InstanceID);
            if (player != PartyState.PlayerSlot) {
                return;
            }

            if (hints.Count != 0) {
                hints.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (NumCasts == 16) {
            return;
        }

        if (Debuff1 == null || Debuff2 == null) {
            return;
        }

        var count = hints.Count;
        for (var i = 0; i < count; ++i) {
            Arena.ZoneCircleOutline(hints[i], 1.0f, i == 0 ? Colors.Safe : default, 2f);
        }
    }
}
