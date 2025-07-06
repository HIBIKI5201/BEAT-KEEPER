using R3;
using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    ///     インゲームのフェーズを管理するクラス
    /// </summary>
    public class PhaseManager : MonoBehaviour
    {
        /// <summary>
        ///     現在のフェーズ
        /// </summary>
        public PhaseEnum CurrentPhase => _currentPhaseProp.Value;

        public ReadOnlyReactiveProperty<PhaseEnum> CurrentPhaseProp => _currentPhaseProp;

        public void NextPhase()
        {
            TransitionTo((PhaseEnum)((int)(_currentPhaseProp.Value + 1) % Enum.GetValues(typeof(PhaseEnum)).Length));
        }

        [SerializeField] private PhaseEnum _firstPhase = PhaseEnum.Movie;
        private readonly ReactiveProperty<PhaseEnum> _currentPhaseProp = new();

        private void Awake()
        {
            TransitionTo(_firstPhase); // 指定したフェーズから始める
        }

        /// <summary>
        ///     フェーズを変更する※InGameSystemから変更すること
        /// </summary>
        private void TransitionTo(PhaseEnum nextPhase)
        {
            // 同じフェーズへの遷移をチェック
            if (CurrentPhase == nextPhase) Debug.Log($"[PhaseManager] 同じフェーズです: {nextPhase}");

            // フェーズの更新
            _currentPhaseProp.Value = nextPhase;
            Debug.Log($"[PhaseManager] フェーズが変更されました 現在：{CurrentPhase}");
        }
    }
}