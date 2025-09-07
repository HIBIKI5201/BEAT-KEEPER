namespace BeatKeeper
{
    /// <summary>
    /// エフェクトタイプの列挙型
    /// </summary>
    public enum EffectTypeEnum
    {
        /// <summary>ビネット効果の強度の調整</summary>
        VignetteIntensity,

        /// <summary>彩度の調整</summary>
        Saturation,

        /// <summary>コントラストの調整</summary>
        Contrast,

        /// <summary>被写界深度のフォーカス位置の調整</summary>
        DepthOfFieldFocus,

        /// <summary>レンズディストーション（歪み）の強度の調整</summary>
        LensDistortion,

        /// <summary>ブルームエフェクトの強度</summary>
        BloomIntensity,

        /// <summary>色収差エフェクトの強度</summary>
        ChromaticAberration,

        /// <summary>フィルムグレインエフェクトの強度</summary>
        FilmGrain,

        /// <summary>カメラの視野角の調整</summary>
        CameraFov
    }
}
