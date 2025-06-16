using SymphonyFrameWork.Debugger;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// MusicEngineの補助クラス
    /// </summary>
    public static class MusicEngineHelper
    {
        /// <summary>1拍の秒数</summary>
        public static double DurationOfBeat => 60 / Music.CurrentTempo;

        /// <summary>現在の小節数を取得する</summary>
        public static int GetCurrentBarCount() => Music.Just.Bar;

        /// <summary>現在の拍数を取得する</summary>
        public static int GetCurrentBeatCount() => Music.Just.Beat;

        /// <summary>現在の16分音符位置を取得する</summary>
        public static int GetCurrentUnitCount() => Music.Just.Unit;

        /// <summary>現在の音楽タイミングを取得する</summary>
        public static TimingKey GetCurrentTiming() => new(Music.Just.Bar, Music.Just.Beat, Music.Just.Unit);

        /// <summary>
        ///     現在の音楽タイミングから、開始時点からの拍数を取得します。
        /// </summary>
        //TODO 音楽がループした際に合計拍数がリセットされないようにする
        public static int GetBeatSinceStart() => Music.Just.Beat + Music.Just.Bar * 4;

        /// <summary>
        ///     現在の音楽タイミングから、開始時点からの近い方の拍数を取得します。
        /// </summary>
        public static int GetBeatNearerSinceStart()
        {
            var timing = GetBeatSinceStart();

            if (0.5f < Music.UnitFromJust) //もしタイミングが後半なら次の拍にする
            {
                timing++;
            }

            return timing;
        }

        /// <summary>
        ///     入力タイミングが許容範囲内にあるかどうかを判定します
        /// </summary>
        public static bool IsTimingWithinAcceptableRange(float range)
        {
            // 許容範囲が0から1の間であることを検証
            if (range < 0f || 1f < range)
            {
                Debug.LogWarning($"[MusicEngineHelper] 許容範囲は0から1の間である必要があります。現在の値:{range}");
                return false;
            }

            var normalizedTimingFromJust = (float)Music.UnitFromJust;

            #region デバッグログ
            SymphonyDebugLog.DirectLog(
                "[MusicEngineHelper]\n"
                + $"{(Mathf.Abs(normalizedTimingFromJust - 0.5f) <= range / 2 ? "Success" : "Failed")}\n"
                + $"timing : {Mathf.Abs(normalizedTimingFromJust - 0.5f) * 2}\n"
                + $"range : {range}");
            #endregion

            // Justタイミング付近か判定
            return Mathf.Abs(normalizedTimingFromJust - 0.5f) <= range / 2;
        }
    }
}