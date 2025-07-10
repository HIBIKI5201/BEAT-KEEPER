using System;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    /// <summary>
    ///    戦闘で使用する譜面の種類を定義する列挙型
    /// </summary>
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