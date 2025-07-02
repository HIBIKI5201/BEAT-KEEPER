using System;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    [Flags]
    public enum ChartKindEnum : int
    {
        None = 0,
        //プレイヤーアクション
        Attack = 1 << 0,
        Skill = 1 << 1,
        //エネミーアクション
        Normal = 1 << 2,
        Charge = 1 << 3,
        Super = 1 << 4,
    }
}