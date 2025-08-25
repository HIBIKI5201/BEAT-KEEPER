using System;
using BeatKeeper.Runtime.Ingame.System;
using R3;
using UnityEngine;
using BeatKeeper.Runtime.System;
using Random = UnityEngine.Random;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     フローゾーンを管理するシステム
    /// </summary>
    public class FlowZoneSystem : IDisposable
    {
        public FlowZoneSystem(BGMManager bgmManager, PlayerData data)
        {
            if (bgmManager)
            {
                _musicEngineHelper = bgmManager;
            }
            else
            {
                Debug.LogWarning("FlowZoneSystem: musicEngineHelper is null");
            }

            _data = data; // フローゾーンの継続時間を拍数で指定
        }

        ~FlowZoneSystem()
        {
            Dispose();
        }

        public const int MAX_COUNT = 5;

        public event Action OnStartFlowZone;
        public event Action OnEndFlowZone;

        public ReadOnlyReactiveProperty<int> ResonanceCount => _resonanceCount;
        public ReadOnlyReactiveProperty<bool> IsFlowZone => _isFlowZone;

        public void Dispose()
        {
            _musicEngineHelper.OnJustChangedBeat -= OnJustBeat;
        }

        /// <summary>
        ///     リズム共鳴に成功
        /// </summary>
        public void SuccessResonance()
        {
            // フローゾーン中であれば以下の処理はスキップする
            if (_isFlowZone.Value) return;

            _resonanceCount.Value++;

            if (_resonanceCount.Value >= MAX_COUNT)
            {
                StartFlowZone();
            }
        }

        /// <summary>
        ///     フローゾーンの継続時間をリセット
        /// </summary>
        public void ResetFlowZone()
        {
            if (_isFlowZone.Value) // フローゾーン中であれば終了する
            {
                EndFlowZone();
            }
        }
        
        /// <summary>
        ///     ゾーンカウントをリセットする
        /// </summary>
        public void ResetResonanceCount()
        {
            _resonanceCount.Value = 0;
        }

        private readonly string[] _voices = {"voice_flow_zone_enter_01", "voice_flow_zone_enter_02"};
        private readonly PlayerData _data;
        private readonly BGMManager _musicEngineHelper;

        /// <summary>
        /// リズム共鳴回数
        /// </summary>
        private readonly ReactiveProperty<int> _resonanceCount = new();
        
        /// <summary>
        /// フローゾーン中か
        /// </summary>
        private readonly ReactiveProperty<bool> _isFlowZone = new();

        private int _count;
        
        /// <summary>
        ///     拍数が変更されるタイミングで呼び出されるメソッド
        /// </summary>
        private void OnJustBeat()
        {
            _count++;

            if (_count >= _data.FlowZoneDuration) // フローゾーン継続時間が終了したら
            {
                EndFlowZone(); // フローゾーンを終了する
            }
        }

        /// <summary>
        ///     フローゾーンを開始する
        /// </summary>
        private void StartFlowZone()
        {
            _isFlowZone.Value = true; // 5回リズム共鳴に成功したらフローゾーン突入
            
            PlayRandomVoice(); // ボイス再生
            
            _musicEngineHelper.OnJustChangedBeat += OnJustBeat; // 継続時間を確認するために拍数を取得する
            OnStartFlowZone?.Invoke();
        }

        /// <summary>
        ///     フローゾーンを終了する
        /// </summary>
        private void EndFlowZone()
        {
            _isFlowZone.Value = false;
            _count = 0;
            _resonanceCount.Value = 0;
            _musicEngineHelper.OnJustChangedBeat -= OnJustBeat; // 購読をやめる
            OnEndFlowZone?.Invoke();
        }
        
        /// <summary>
        /// ボイスを再生する
        /// </summary>
        private void PlayRandomVoice()
        {
            var rand = Random.Range(0, _voices.Length);
            var cueName = _voices[rand];
            VoiceManager.PlayVoice(cueName);
        }
    }
}
