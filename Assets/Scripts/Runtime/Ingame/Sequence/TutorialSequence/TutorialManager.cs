using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private UIElement_ChartRingManager _chartRingManager;
        [SerializeField, Tooltip("何拍ごとにインジケーターを出すか")] private int _indicatorGenerateCount;
        [SerializeField, Tooltip("チュートリアルクリアに何回good以上の判定を出すか")] private int _targetClearCount = 4;
        [SerializeField] private float _goodRange = 0.8f;
        [SerializeField] private float _perfectRange = 0.5f;
        [SerializeField, Tooltip("チュートリアルをプレイするかどうか")] private bool _playTutorial = true;
        ChartKindEnum _chartKindEnum;

        private List<RingIndicatorBase> _activeRingIndicator = new();
        private BGMManager _bgmManager;
        private InputBuffer _inputBuffer;
        private int _currentIndicatorCount = 0;
        private int _currentTargetClearCount = 0;

        private async void Start()
        {
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            _inputBuffer = await ServiceLocator.GetInstanceAsync<InputBuffer>();
        }

        private void OnDestroy()
        {
            TutorialUnRegister();
        }

        public void StartTutorial()
        {
            _director.Play();
        }


        public void PlayVoice(int voiceNum)
        {

        }

        public void TutorialRegister(ChartKindEnum chartKindEnum)
        {
            if (!_playTutorial) return;
            _chartKindEnum = chartKindEnum;
            _inputBuffer.Attack.started += OnShot;
            _bgmManager.OnJustChangedBeat += TutorialIndicatorGenerate;
            _director.Pause();
        }

        public void TutorialUnRegister()
        {
            _director.Resume();
            _bgmManager.OnJustChangedBeat -= TutorialIndicatorGenerate;
            foreach (var ind in _activeRingIndicator)
            {
                ind.End();
            }
        }

        /// <summary>
        /// チュートリアルで入力を受け付けるためのインジケーターを生成する処理
        /// </summary>
        public void TutorialIndicatorGenerate()
        {
            if (_activeRingIndicator.Count > 0 && !_activeRingIndicator[0].CheckRemainTime()) _activeRingIndicator.RemoveAt(0);

            foreach (var ind in _activeRingIndicator)
            {
                ind.AddCount();
            }
            if (_indicatorGenerateCount <= _currentIndicatorCount)
            {
                var ringObj = _chartRingManager.GenerateRing(_chartKindEnum, Vector2.zero, 0);
                var ringIndicator = ringObj.GetComponent<RingIndicatorBase>();
                if (ringIndicator)
                {
                    _activeRingIndicator.Add(ringIndicator);
                }
                _currentIndicatorCount = 0;
            }
            Debug.Log(_currentIndicatorCount);
            _currentIndicatorCount++;
        }

        /// <summary>
        /// チュートリアル用のショット処理
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnShot(InputAction.CallbackContext callbackContext)
        {
            if (_activeRingIndicator.Count == 0) return;
           
            if (callbackContext.phase == InputActionPhase.Started)
            {
                var isGood = CheckGood();
                var isPerfect = CheckPerfect();
                var playerIndicator = (PlayerIndicator)_activeRingIndicator[0];
                if (isGood)
                {
                    _currentTargetClearCount++;

                    if (isPerfect)
                    {
                        Debug.Log("Perfect!");
                        playerIndicator.PlayPerfectEffect();
                    }
                    else
                    {
                        Debug.Log("Good!");
                        playerIndicator.PlayGoodEffect();
                    }
                }
                else
                {
                    Debug.Log("Missed!");
                    playerIndicator.PlayFailEffect();
                    _activeRingIndicator.RemoveAt(0);
                }
            }
        }

        private bool CheckGood()
        {
            if (_currentIndicatorCount == 0 || _currentIndicatorCount == _indicatorGenerateCount) return false;

            var normalizedTimingFromJust = (float)Music.UnitFromJust;
            Debug.Log($"Normalized Timing from Just: {normalizedTimingFromJust}");

            // Justタイミング付近か判定
            return Mathf.Abs(normalizedTimingFromJust) <= _goodRange / 2;
        }
        private bool CheckPerfect()
        {
            if (_currentIndicatorCount == 0 || _currentIndicatorCount == _indicatorGenerateCount) return false;
            var normalizedTimingFromJust = (float)Music.UnitFromJust;
            // Justタイミング付近か判定
            Debug.Log($"Normalized Timing from Just: {normalizedTimingFromJust}");
            return Mathf.Abs(normalizedTimingFromJust) <= _perfectRange / 2;
        }
    }
}
