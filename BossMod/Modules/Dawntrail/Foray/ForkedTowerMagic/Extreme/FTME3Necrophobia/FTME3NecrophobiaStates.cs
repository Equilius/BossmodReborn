namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

[SkipLocalsInit]
sealed class FTME3NecrophobiaStates : StateMachineBuilder
{
    public FTME3NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
