using R3;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// インゲームのフェーズを管理するクラス
    /// </summary>
    public class PhaseManager : MonoBehaviour
    {
        [SerializeField] private PhaseEnum _firstPhase = PhaseEnum.Approach;

        /// <summary>
        /// 現在のフェーズ
        /// </summary>
        public PhaseEnum CurrentPhase => _currentPhaseProp.Value;
        public ReadOnlyReactiveProperty<PhaseEnum> CurrentPhaseProp => _currentPhaseProp;
        private readonly ReactiveProperty<PhaseEnum> _currentPhaseProp = new ReactiveProperty<PhaseEnum>();

        private void Awake()
        {
            TransitionTo(_firstPhase); // 指定したフェーズから始める
        }
        
        /// <summary>
        /// フェーズを変更する※InGameSystemから変更すること
        /// </summary>
        public void TransitionTo(PhaseEnum nextPhase)
        {
            // 同じフェーズへの遷移をチェック
            if (CurrentPhase == nextPhase)
            {
                Debug.Log($"[PhaseManager] 同じフェーズです: {nextPhase}");
            }
            
            // フェーズの更新
            _currentPhaseProp.Value = nextPhase;
            Debug.Log($"[PhaseManager] フェーズが変更されました 現在：{CurrentPhase}");
        }
    }
}
