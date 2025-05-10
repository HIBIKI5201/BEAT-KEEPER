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
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        private void Start()
        {
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _phaseManager.CurrentPhaseProp
                .Subscribe(OnPhaseChanged)
                .AddTo(_disposable);
        }
        
        /// <summary>
        /// BGMを変更する
        /// </summary>
        private void OnPhaseChanged(PhaseEnum newPhase)
        {
            switch (newPhase)
            {
                case PhaseEnum.Approach:
                case PhaseEnum.Battle2:
                case PhaseEnum.Battle3:
                    int index = (int)newPhase;
                    Music.SetHorizontalSequence(((BGMEnum)index).ToString());
                    Debug.Log("[BGMChanger] BGMを変更しました" + newPhase);
                    break;
                case PhaseEnum.Battle1:
                    // Enumの変更タイミングとは別のタイミングで処理を行うため、ここで行うべき処理はない
                    break;
                case PhaseEnum.Clear:
                    //TODO: Clearフェーズの処理を作る(曲の変更ではなく音響効果の変更を行う予定)
                    break;
            }
        }

        /// <summary>
        /// BGMを任意のタイミングで変更する
        /// </summary>
        public void ChangeBGM(PhaseEnum newPhase)
        {
            // 想定されていないBGM変更は行わないようにする
            if (newPhase != PhaseEnum.Battle1)
            {
                return;
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