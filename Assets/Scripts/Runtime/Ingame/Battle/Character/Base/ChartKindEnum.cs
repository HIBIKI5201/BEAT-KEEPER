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
        Attack = 1 << 0, //コンボ攻撃
        Skill = 1 << 1, //スキル攻撃
        //エネミーアクション
        Normal = 1 << 2, //通常攻撃
        Charge = 1 << 3, //チャージ攻撃

        Finisher = 1 << 4, //フィニッシャー攻撃
    }
}