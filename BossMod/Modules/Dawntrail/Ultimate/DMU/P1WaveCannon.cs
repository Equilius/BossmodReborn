namespace BossMod.Dawntrail.Ultimate.DMU;

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
        if (remaining <= 0.7f) {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        var safeSpot = P1WaveCannonData.Safespots.GetValueOrDefault(assignment);
        if (safeSpot == default) {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedCircle(safeSpot, 1.0f), activation);
    }
}

// TODO fix AIHint for when towers overlap - maybe just be corrected in default component
sealed class WaveCannonTowers(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4.0f) {
    private readonly DateTime[] debuffs = new DateTime[PartyState.MaxPartySize];
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.MagicVulnerabilityUp) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0) {
                debuffs[slot] = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);

        if (Towers.Count == 4) {
            Towers.Sort((t1, t2) => t1.Position.X.CompareTo(t2.Position.X));

            // Case: party assignments are not set up so, we just assign towers base on debuffs
            var slots = partyConfig.SlotsPerAssignment(Raid);
            if (slots.Length == 0) {
                setupDefaultForbiddenPlayers();
                return;
            }

            // Case: Party assignment are set up so we can solve it correctly
            setupSpecificForbiddenPlayers();
        }
    }

    private void setupDefaultForbiddenPlayers() {
        for (int i = 0; i < Towers.Count; i++) {
            var tower = Towers[i];
            BitMask forbiddenPlayers = default;

            for (int k = 0; k < debuffs.Length; k++) {
                var playerDebuff = debuffs[k];
                if (playerDebuff > tower.Activation) {
                    forbiddenPlayers.Set(k);
                }
            }

            tower.ForbiddenSoakers = forbiddenPlayers;
            Towers[i] = tower;
        }
    }

    private void setupSpecificForbiddenPlayers() {
        var activation = Towers[0].Activation;
        List<(int slot, int order)> soakers = [];

        // Find the four players without debuffs
        for (int i = 0; i < debuffs.Length; i++) {
            if (debuffs[i] > activation) {
                continue;
            }

            var assignment = partyConfig[Raid.Members[i].ContentId];
            var order = dmuConfig.P1WaveCannonAssignment[assignment];
            soakers.Add((i, order));
        }

        soakers.Sort((a, b) => a.order.CompareTo(b.order));

        for (int i = 0; i < Towers.Count && i < soakers.Count; i++) {
            var tower = Towers[i];
            var soaker = soakers[i];
            BitMask forbiddenPlayers = default;
            forbiddenPlayers.Set(soaker.slot);
            tower.ForbiddenSoakers = ~forbiddenPlayers;
            Towers[i] = tower;
        }
    }
}
