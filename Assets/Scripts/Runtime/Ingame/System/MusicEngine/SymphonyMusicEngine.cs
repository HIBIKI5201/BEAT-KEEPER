using CriWare;
using System;
using System.Linq;
using UnityEngine;

namespace BeatKeeper
{
    public class SymphonyMusicEngine : MonoBehaviour
    {
        public static CriAtomSource CurrentSource => _self._currentTrack.Source;
        public static CriAtomExPlayback CurrentPlayback => _self._currentPlayback;
        public static int CurrentBPM => _self._currentTrack.BPM;

        public static int CurrentBar => _self._currentBar;
        public static int CurrentBeat => _self._currentBeat;

        public static CriAtomExPlayback Play(string name)
        {
            // 新しい曲を再生。
            MusicTrack newTrack = _self._sources.FirstOrDefault(x => x.Name == name);
            CriAtomExPlayback playback = newTrack.Source.Play();

            // 再生中のを止める。
            _self._currentTrack.Source.Stop();
            _self._currentTrack = newTrack;
            _self._currentPlayback = playback;

            return playback;
        }

        public static void Stop()
        {
            _self._currentTrack.Source.Stop();
            _self._currentTrack = default;
        }

        private const int BEATS_PER_MEASURE = 4; // 1小節あたりの拍数

        private static SymphonyMusicEngine _self;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            _self = null;
        }

        private MusicTrack[] _sources;
        private MusicTrack _currentTrack;
        private CriAtomExPlayback _currentPlayback;

        private int _currentBar;
        private int _currentBeat;

        private void Awake()
        {
            //シングルトン化
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
            long timeMs = _currentPlayback.GetTime(); // 再生開始からの経過時間（ms）。

            // 1拍の長さ（ms）。
            float beatLengthMs = 60000f / _currentTrack.BPM;

            float beatPosition = timeMs / beatLengthMs;

            // 小節番号と小節内の拍位置に分解
            int measure = Mathf.FloorToInt(beatPosition / BEATS_PER_MEASURE) + 1; // 1始まりの小節番号
            float beatInMeasure = (beatPosition % BEATS_PER_MEASURE) + 1; // +1 して 1拍目から始まるように調整。

            _currentBar = measure;
            _currentBeat = Mathf.FloorToInt(beatInMeasure);
        }

        [Serializable]
        private struct MusicTrack
        {
            public string Name => _source.name;
            public CriAtomSource Source => _source;
            public int BPM => _bpm;

            [SerializeField]
            private CriAtomSource _source;

            [SerializeField]
            private int _bpm;
        }
    }
}
