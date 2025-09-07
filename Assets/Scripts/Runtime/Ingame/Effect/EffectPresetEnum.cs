namespace BeatKeeper
{
    /// <summary>
    /// プリセット用のエフェクトタイプ定義
    /// </summary>
    public enum EffectPresetEnum
    {
        /// <summary>デフォルト設定（ApproachPhase）</summary>
        Default,

        /// <summary>アプローチフェーズ-ロックオン中</summary>
        Focus,

        /// <summary>アプローチフェーズ-ビートクリスタル回収直後</summary>
        GetCrystal,

        /// <summary>バトルフェーズ</summary>
        Battle,

        /// <summary>フローゾーン</summary>
        FlowZone,

        /// <summary>フィニッシャー</summary>
        Finisher,

        /// <summary>クリアフェーズ</summary>
        Clear,
    }
}
