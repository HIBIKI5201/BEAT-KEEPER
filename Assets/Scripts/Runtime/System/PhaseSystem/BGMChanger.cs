using BeatKeeper.Runtime.Ingame.System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// フェーズ情報を元にBGMを変更するためのクラス
    /// </summary>
    public class BGMChanger : MonoBehaviour
    {
        [SerializeField] private InGameSystem _inGameSystem;
        
        private void Start()
        {
            if (_inGameSystem == null)
            {
                _inGameSystem = FindObjectOfType<InGameSystem>();
                if (_inGameSystem == null)
                {
                    Debug.LogError("[BGMChanger] InGameSystemが見つかりませんでした");
                }
            }
            _inGameSystem.OnPhaseChange += OnPhaseChanged;
        }
        
        /// <summary>
        /// BGMを変更する
        /// </summary>
        private void OnPhaseChanged(PhaseEnum newPhase)
        {
            if (newPhase == PhaseEnum.Clear)
            {
                return; //TODO: Clearフェーズの処理を作る(曲の変更ではなく音響効果の変更を行う予定)
            }
            
            int index = (int)newPhase;
            Music.SetHorizontalSequence(((BGMEnum)index).ToString());
            Debug.Log("[BGMChanger] BGMを変更しました");
        }

        private void OnDestroy()
        {
            _inGameSystem.OnPhaseChange -= OnPhaseChanged;
        }
    }
}