using System;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    [Flags]
    public enum AttackKindEnum : int
    {
        None = 0,
        Normal = 1 << 0,
        Charge = 1 << 1,
        Super = 1 << 2
    }
}