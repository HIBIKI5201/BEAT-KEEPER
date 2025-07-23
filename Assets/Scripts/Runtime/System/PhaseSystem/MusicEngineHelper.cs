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
        public static double DurationOfBeat => 60 / Music.CurrentTempo;

        /// <summary>現在の小節数を取得する</n>
        public static int GetCurrentBarCount() => Music.Just.Bar;

        /// <summary>現在の拍数を取得する</n>
        public static int GetCurrentBeatCount() => Music.Just.Beat;

        /// <summary>現在の16分音符位置を取得する</n>
        public static int GetCurrentUnitCount() => Music.Just.Unit;

        /// <summary>現在の音楽タイミングを取得する</n>
        public static TimingKey GetCurrentTiming() => new(Music.Just.Bar, Music.Just.Beat, Music.Just.Unit);

        /// <summary>
        ///     現在の音楽タイミングから、開始時点からの拍数を取得します。
        /// </summary>
        public static int GetBeatSinceStart()
        {
            // Music.Justは現在の拍のタイミングを表すため、その総単位数を取得し、拍単位に変換する
            if (Music.CurrentMeter == null)
            {
                return 0;
            }
            return Music.Just.GetTotalUnits(Music.CurrentMeter) / Music.CurrentMeter.UnitPerBeat
                + _beatOffset;
        }

        /// <summary>
        ///     現在の音楽タイミングから、開始時点からの近い方の拍数を取得します。
        /// </summary>
        public static int GetBeatNearerSinceStart()
        {
            // 新しいメソッドを使用して最も近い拍のタイミングを取得
            Timing closestBeatTiming = GetClosestBeatTiming();

            // Timingオブジェクトを開始時点からの総拍数に変換
            // Music.CurrentMeterがnullでないことはGetClosestBeatTiming()でチェック済み
            return closestBeatTiming.GetTotalUnits(Music.CurrentMeter) / Music.CurrentMeter.UnitPerBeat
                + _beatOffset;
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

        /// <summary>
        /// 現在の音楽再生位置から最も近い拍のタイミングを取得します。
        /// </summary>
        /// <returns>最も近い拍のTimingオブジェクト</returns>
        public static Timing GetClosestBeatTiming()
        {
            if (Music.CurrentMeter == null)
            {
                Debug.LogWarning("Music.CurrentMeter is null. Cannot get closest beat timing.");
                return new Timing();
            }

            MusicMeter meter = Music.CurrentMeter;

            // 現在の正確な総単位数を計算
            double currentTotalUnits = Music.Just.GetTotalUnits(meter) + Music.UnitFromJust;

            // 最も近い拍単位の総数を計算
            double totalBeats = currentTotalUnits / meter.UnitPerBeat;
            int closestTotalBeats = (int)Math.Round(totalBeats);

            // 最も近い拍単位の総数を音楽単位に戻す
            int closestTotalUnits = closestTotalBeats * meter.UnitPerBeat;

            // この総単位数からTimingオブジェクトを生成
            int bar = closestTotalUnits / meter.UnitPerBar;
            int remainingUnits = closestTotalUnits % meter.UnitPerBar;
            int beat = remainingUnits / meter.UnitPerBeat;
            int unit = remainingUnits % meter.UnitPerBeat; // 拍上なのでunitは0になるはず

            return new Timing(bar + meter.StartBar, beat, unit);
        }

        /// <summary>
        /// 指定されたタイミング（秒）から最も近い拍のタイミングを取得します。
        /// </summary>
        /// <param name="sec">秒単位のタイミング</param>
        /// <param name="meter">使用するMusicMeterインスタンス</param>
        /// <returns>最も近い拍のTimingオブジェクト</returns>
        public static Timing GetClosestBeatTimingFromSeconds(double sec, MusicMeter meter)
        {
            if (meter == null)
            {
                Debug.LogWarning("MusicMeter is null. Cannot get closest beat timing.");
                return new Timing();
            }

            // StartSecからの総単位数を計算
            double meterSec = sec - meter.StartSec;
            double totalUnits = meterSec / meter.SecPerUnit;
            
            // 最も近い拍単位の総数を計算
            double totalBeats = totalUnits / meter.UnitPerBeat;
            int closestTotalBeats = (int)Math.Round(totalBeats);
            int closestTotalUnits = closestTotalBeats * meter.UnitPerBeat;

            // この総単位数からTimingオブジェクトを生成
            int bar = closestTotalUnits / meter.UnitPerBar;
            int remainingUnits = closestTotalUnits % meter.UnitPerBar;
            int beat = remainingUnits / meter.UnitPerBeat;
            int unit = remainingUnits % meter.UnitPerBeat;

            return new Timing(bar + meter.StartBar, beat, unit);
        }

        /// <summary>
        /// 指定されたタイミング（ミリ秒）から最も近い拍のタイミングを取得します。
        /// </summary>
        /// <param name="msec">ミリ秒単位のタイミング</param>
        /// <param name="meter">使用するMusicMeterインスタンス</param>
        /// <returns>最も近い拍のTimingオブジェクト</returns>
        public static Timing GetClosestBeatTimingFromMilliSeconds(double msec, MusicMeter meter)
        {
            return GetClosestBeatTimingFromSeconds(msec / 1000.0, meter);
        }

        //ここより下にprivateの変数と関数を定義する
        private static int _beatOffset = 0;
        private static int _last;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _beatOffset = 0;
            _last = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Update()
        {
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
        }
    }
}