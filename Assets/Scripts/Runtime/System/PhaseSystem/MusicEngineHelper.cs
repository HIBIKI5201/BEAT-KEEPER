using SymphonyFrameWork.Debugger;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// MusicEngineの補助クラス
    /// </summary>
    public static class MusicEngineHelper
    {
        /// <summary>1拍の秒数</summary>
        public static double DurationOfBeat => 60f / SymphonyMusicEngine.CurrentBPM;

        /// <summary>
        ///     現在の音楽タイミングから、開始時点からの拍数を取得します。
        /// </summary>
        public static int GetBeatSinceStart()
        {
            return SymphonyMusicEngine.CurrentBeat;
        }

        /// <summary>
        ///     現在の音楽タイミングから、開始時点からの近い方の拍数を取得します。
        /// </summary>
        public static int GetBeatNearerSinceStart()
        {
            if (!SymphonyMusicEngine.IsPlaying) return 0;

            // Timingオブジェクトを開始時点からの総拍数に変換
            return SymphonyMusicEngine.CurrentNearBeat
                + _beatOffset - _startTiming;
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

            var normalizedTimingFromJust = SymphonyMusicEngine.UnitFromJust;

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

        /// <summary>
        /// タイミングの開始位置を設定する
        /// </summary>
        public static void SetStartTiming()
        {
            _startTiming = SymphonyMusicEngine.CurrentBeat + _beatOffset;
        }

        //ここより下にprivateの変数と関数を定義する
        private static int _beatOffset = 0;
        private static int _last;

        // 今回のフェーズが始まったタイミングの記録
        private static int _startTiming = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _beatOffset = 0;
            _last = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Update()
        {
            /*
            while (Application.isPlaying)
            {
                //ここからブロックのループとオフセットの設定の処理
                if (Music.CurrentMeter != null)
                {
                    var currentBeat = Music.Just.GetTotalUnits(Music.CurrentMeter) / Music.CurrentMeter.UnitPerBeat;
                    if (currentBeat < _last)
                    {
                        // ループを検出した場合、前の拍数にオフセットを加算する
                        // ループの長さは、ループ直前の拍数 - ループ後の拍数 + 1
                        _beatOffset += _last - currentBeat + 1;
                    }
                    _last = currentBeat;
                }

                await Awaitable.NextFrameAsync();
            }
            */
        }
    }
}