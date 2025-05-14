using System;
using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    /// フローゾーンを管理するシステム
    /// </summary>
    public class FlowZoneSystem : IDisposable
    {
        private readonly MusicEngineHelper _musicEngineHelper;
        private int _count;
        private const int DURATION = 16; // フローゾーンの継続拍数

        public FlowZoneSystem(MusicEngineHelper musicEngineHelper)
        {
            _musicEngineHelper = musicEngineHelper;
        }
        
        /// <summary>
        /// リズム共鳴回数
        /// </summary>
        public ReadOnlyReactiveProperty<int> ResonanceCount => _resonanceCount;
        private ReactiveProperty<int> _resonanceCount = new();
        
        /// <summary>
        /// フローゾーン中か
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsFlowZone => _isFlowZone;
        private ReactiveProperty<bool> _isFlowZone = new();

        /// <summary>
        /// リズム共鳴に成功
        /// </summary>
        public void SuccessResonanceHit()
        {
            if (_isFlowZone.Value)
            {
                return; // フローゾーン中であれば以下の処理はスキップする
            }
            
            if (_resonanceCount.Value >= 6)
            {
                _isFlowZone.Value = true; // 7回リズム共鳴に成功したらフローゾーン突入
                _musicEngineHelper.OnJustChangedBeat += Count;
            }
            
            _resonanceCount.Value++; // 0~6の範囲
        }

        private void Count()
        {
            _count++;
            
            if (_count <= DURATION) // 継続時間が終了したら
            {
                _isFlowZone.Value = false;
                _resonanceCount.Value = 0; // 共鳴回数をリセット
                _musicEngineHelper.OnJustChangedBeat -= Count;
            }
        }

        public void Dispose()
        {
            _musicEngineHelper.OnJustChangedBeat -= Count;
        }
    }
}
