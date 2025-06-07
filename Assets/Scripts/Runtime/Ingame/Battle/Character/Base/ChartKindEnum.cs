using System;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    [Flags]
    public enum ChartKindEnum : int
    {
        None = 0,

        Attack = 1 << 0,

        Normal = 1 << 1,
        Charge = 1 << 2,
        Super = 1 << 3,
    }
}