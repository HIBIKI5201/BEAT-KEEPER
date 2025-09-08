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

        /// <summary>
        ///     指定された時間が今とは別のフェーズかどうかをチェックする
        /// </summary>
        /// <param name="timing">アプリケーション開始からの時間(s)</param>
        /// <returns></returns>
        public bool IsAnotherPhaseByTiming(float timing)
        {
            return timing < _phaseChangeTiming;
        }
        
        /// <summary>
        ///     フェーズを変更する
        /// </summary>
        public void TransitionTo(PhaseEnum nextPhase)
        {
            // 同じフェーズへの遷移をチェック
            if (CurrentPhase == nextPhase)
            {
                Debug.Log($"[PhaseManager] 同じフェーズです: {nextPhase}");
                return;
            }

            // フェーズの更新
            _currentPhaseProp.Value = nextPhase;
            _phaseChangeTiming = Time.time;

            Debug.Log($"[PhaseManager] フェーズが変更されました 現在：{CurrentPhase}");
        }

        [SerializeField] private PhaseEnum _firstPhase = PhaseEnum.Movie;
        private readonly ReactiveProperty<PhaseEnum> _currentPhaseProp = new();

        private float _phaseChangeTiming;
        private void Awake()
        {
            TransitionTo(_firstPhase); // 指定したフェーズから始める
        }
    }
}