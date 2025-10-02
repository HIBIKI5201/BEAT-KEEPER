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

        /// <summary> 現在の近接拍位置 </summary>
        public static int CurrentNearBeat => Mathf.FloorToInt(_self._currentBeatNear);

        /// <summary> 直前のジャストからの正規化した距離 </summary>
        public static float UnitFromJust => _self._unitFromJust;

        /// <summary>
        /// 最後の Near 拍の整数部分（floor 値）
        /// </summary>
        public static int LastNearFloor => Mathf.FloorToInt(_self._currentBeatNear);

        public static bool IsPlaying
        {
            get => !_self._currentTrack.Source?.IsPaused() ?? false;
        }

        /// <summary>
        /// 指定したオブジェクト名の音楽を再生する。
        /// </summary>
        public static CriAtomExPlayback Play(string name)
        {
            MusicTrack newTrack = _self._tracks.FirstOrDefault(x => x.Name == name);
            CriAtomExPlayback playback = newTrack.Source.Play();

            _self._currentTrack.Source?.Stop();

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
        private int _currentBeatNear;
        private float _unitFromJust;

        private int _lastBeat;
        private int _lastNearBeat;

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
            // 経過時間（秒）
            float elapsedSec = _currentPlayback.GetTime() / 1000f;

            // 1拍の秒数
            float beatDuration = 60f / _currentTrack.BPM;

            // Beat（1始まりの整数）
            _currentBeat = Mathf.FloorToInt(elapsedSec / beatDuration) + 1;

            // NearBeat（裏拍：0.5刻み → 拍を2倍して整数化）
            _currentBeatNear = Mathf.FloorToInt((elapsedSec / beatDuration) - 0.5f)+1;

            // ジャストからの正規化位置（0〜1）
            _unitFromJust = (elapsedSec % beatDuration) / beatDuration;
            CheckBeat();
        }

        private void CheckBeat()
        {
            if (_lastBeat != _currentBeat)
            {
                _lastBeat = _currentBeat;
                _onBeatChanged?.Invoke();
            }

            if (_lastNearBeat != _currentBeatNear)
            {
                _lastNearBeat = _currentBeatNear;
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
