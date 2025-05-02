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
        private PhaseManager _phaseManager;
        private CompositeDisposable _phaseSubscriptions = new CompositeDisposable();
        
        private void Start()
        {
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            
            // フェーズが切り替わったタイミングでBGM変更処理を呼ぶサブスクリプション
            _phaseManager.CurrentPhaseProp
                .Subscribe(OnPhaseChanged)
                .AddTo(_phaseSubscriptions);
        }
        
        /// <summary>
        /// BGMを変更する
        /// </summary>
        private void OnPhaseChanged(PhaseEnum newPhase)
        {
            if (newPhase == PhaseEnum.Clear)
            {
                return; //TODO: Clearフェーズの処理を作る
            }
            
            int index = (int)newPhase;
            Music.SetHorizontalSequence(((BGMEnum)index).ToString());
            
            Debug.Log("[BGMChanger] BGMを変更しました");
        }

        private void OnDestroy()
        {
            _phaseSubscriptions?.Dispose();
        }
    }
}
