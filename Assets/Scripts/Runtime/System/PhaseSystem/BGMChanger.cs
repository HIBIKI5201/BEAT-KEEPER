using BeatKeeper.Runtime.Ingame.System;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// フェーズ情報を元にBGMを変更するためのクラス
    /// </summary>
    public class BGMChanger : MonoBehaviour
    {
        private InGameSystem _inGameSystem;
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        private void Start()
        {
            _inGameSystem = ServiceLocator.GetInstance<InGameSystem>();
            _inGameSystem.PhaseManager.CurrentPhaseProp
                .Subscribe(OnPhaseChanged)
                .AddTo(_disposable);
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
            _disposable?.Dispose();
        }
    }
}