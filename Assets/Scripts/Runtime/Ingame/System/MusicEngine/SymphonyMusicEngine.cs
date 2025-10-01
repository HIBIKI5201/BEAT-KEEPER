using CriWare;
using System;
using System.Linq;
using UnityEngine;

namespace BeatKeeper
{
    [DefaultExecutionOrder(-1000)] // なるべく早く動かす。
    public class SymphonyMusicEngine : MonoBehaviour
    {
        /// <summary>
        /// 拍の位置が変化したときに発火するイベント。
        /// </summary>
        public static event Action OnBeatChanged
        {
            add => _self._onBeatChanged += value;
            remove => _self._onBeatChanged -= value;
        }

        /// <summary>
        /// 拍の中心（裏拍など）を通過したときに発火するイベント。
        /// </summary>
        public static event Action OnBeatNeared
        {
            add => _self._onBeatNeared += value;
            remove => _self._onBeatNeared -= value;
        }

        public static CriAtomSource CurrentSource => _self._currentTrack.Source;
        public static CriAtomExPlayback CurrentPlayback => _self._currentPlayback;
        public static int CurrentBPM => _self._currentTrack.BPM;

        /// <summary> 現在の整数の拍番号（1始まり） </summary>
        public static int CurrentBeat => _self._currentBeat;

        /// <summary> 現在の近接拍位置（例: 3.5 など） </summary>
        public static float CurrentNearBeat => _self._currentBeatNear;

        /// <summary>
        /// 最後の Near 拍の整数部分（floor 値）
        /// </summary>
        public static int LastNearFloor => Mathf.FloorToInt(_self._currentBeatNear);

        /// <summary>
        /// 指定したオブジェクト名の音楽を再生する。
        /// </summary>
        public static CriAtomExPlayback Play(string name)
        {
            MusicTrack newTrack = _self._tracks.FirstOrDefault(x => x.Name == name);
            CriAtomExPlayback playback = newTrack.Source.Play();

            _self._currentTrack.Source.Stop();

            _self._currentTrack = newTrack;
            _self._currentPlayback = playback;

            return playback;
        }

        /// <summary>
        /// 再生中の音楽を停止する。
        /// </summary>
        public static void Stop()
        {
            _self._currentTrack.Source.Stop();
            _self._currentTrack = default;
        }

        private const int BEATS_PER_MEASURE = 4; // 1小節あたりの拍数。

        private static SymphonyMusicEngine _self;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            _self = null;
        }

        [SerializeField]
        private MusicTrack[] _tracks;

        private event Action _onBeatChanged;
        private event Action _onBeatNeared;

        private MusicTrack _currentTrack;
        private CriAtomExPlayback _currentPlayback;

        private int _currentBeat;
        private float _currentBeatNear;

        private int _lastBeat;

        private void Awake()
        {
            // シングルトン化
            if (_self != null)
            {
                Destroy(gameObject);
                return;
            }
            _self = this;
        }

        private void Update()
        {
            if (!_currentPlayback.IsPaused())
            {
                Tick();
            }
        }

        private void Tick()
        {
            long timeMs = _currentPlayback.GetTime(); // 再生開始からの経過時間（ms）

            float beatLengthMs = 60000f / _currentTrack.BPM; // 1拍の長さ（ms）

            float beatPosition = timeMs / beatLengthMs; // 小数付きの拍位置（絶対）

            float beatInMeasure = (beatPosition % BEATS_PER_MEASURE) + 1f; // 小節内の拍位置

            _currentBeat = Mathf.FloorToInt(beatPosition) + 1;

            CheckBeat(beatInMeasure);
        }

        private void CheckBeat(float beatInMeasure)
        {
            // 拍の変化チェック
            if (_lastBeat != _currentBeat)
            {
                _lastBeat = _currentBeat;
                _onBeatChanged?.Invoke();
            }

            // 裏拍（0.5 を超えた瞬間に発火）
            float targetNear = _currentBeat + 0.5f;

            if (_currentBeatNear < targetNear && beatInMeasure >= targetNear)
            {
                _currentBeatNear = targetNear;
                _onBeatNeared?.Invoke();
            }
        }

        [Serializable]
        private struct MusicTrack
        {
            public string Name => _source.name;
            public CriAtomSource Source => _source;
            public int BPM => _bpm;

            [SerializeField] private CriAtomSource _source;
            [SerializeField] private int _bpm;
        }
    }
}
