using System;
using BeatKeeper.Runtime.Ingame.System;
using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    /// フローゾーンを管理するシステム
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

        private readonly PlayerData _data;
        private readonly BGMManager _musicEngineHelper;

        /// <summary>
        /// リズム共鳴回数
        /// </summary>
        public ReadOnlyReactiveProperty<int> ResonanceCount => _resonanceCount;
        private readonly ReactiveProperty<int> _resonanceCount = new();
        
        /// <summary>
        /// フローゾーン中か
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsFlowZone => _isFlowZone;
        private readonly ReactiveProperty<bool> _isFlowZone = new();

        private int _count;

        public void Dispose()
        {
            _musicEngineHelper.OnJustChangedBeat -= OnBeat;
        }

        /// <summary>
        ///     拍数が変更されるタイミングで呼び出されるメソッド
        /// </summary>
        private void OnBeat()
        {
            _count++;

            if (_count >= _data.FlowZoneDuration) // フローゾーン継続時間が終了したら
            {
                FlowZoneEnd(); // フローゾーンを終了する
            }
        }

        /// <summary>
        ///     リズム共鳴に成功
        /// </summary>
        public void SuccessResonance()
        {
            // フローゾーン中であれば以下の処理はスキップする
            if (_isFlowZone.Value) return;
            
            _resonanceCount.Value++;
            
            if (_resonanceCount.Value >= 7)
            {
                _isFlowZone.Value = true; // 7回リズム共鳴に成功したらフローゾーン突入
                _musicEngineHelper.OnJustChangedBeat += OnBeat; // 継続時間を確認するために拍数を取得する
            }
        }

        /// <summary>
        ///     フローゾーンの継続時間をリセット
        /// </summary>
        public void ResetFlowZone()
        {
            if(_isFlowZone.Value) // フローゾーン中であれば終了する
            {
                FlowZoneEnd();
            }
        }

        /// <summary>
        ///     フローゾーンを終了するメソッド
        /// </summary>
        private void FlowZoneEnd()
        {
            _isFlowZone.Value = false;
            _count = 0;
            _resonanceCount.Value = 0;
            _musicEngineHelper.OnJustChangedBeat -= OnBeat; // 購読をやめる
        }
    }
}
