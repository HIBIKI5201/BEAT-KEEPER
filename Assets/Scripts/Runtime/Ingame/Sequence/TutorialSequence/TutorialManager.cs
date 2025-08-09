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
        [SerializeField, Tooltip("チュートリアルクリアに何回good以上の判定を出すか")] private int _attackTutorialClearCount = 4;
        [SerializeField, Tooltip("チュートリアルクリアに何回good以上の判定を出すか")] private int _skillTutorialClearCount = 1;
        [SerializeField] private float _goodRange = 0.8f;
        [SerializeField] private float _perfectRange = 0.5f;
        [SerializeField, Tooltip("チュートリアルをプレイするかどうか")] private bool _playTutorial = true;
        [SerializeField] private string _comboAttackSound;
        [SerializeField] private string _perfectAttackSound;
        ChartKindEnum _chartKindEnum;

        private List<RingIndicatorBase> _activeRingIndicator = new();
        private BGMManager _bgmManager;
        private InputBuffer _inputBuffer;
        private int _currentIndicatorCount = 0;
        private int _currentTargetClearCount = 0;

        private async void Start()
        {
            _chartKindEnum = ChartKindEnum.None;
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
            if (_chartKindEnum == chartKindEnum) return;
            _chartKindEnum = chartKindEnum;
            if (_chartKindEnum == ChartKindEnum.Attack)
            {
                _inputBuffer.Attack.started += OnShot;
            }
            else if (chartKindEnum == ChartKindEnum.Skill)
            {
                _inputBuffer.Attack.started += OnSkill;
            }
            _bgmManager.OnJustChangedBeat += TutorialIndicatorGenerate;
            _director.Pause();
        }

        public void TutorialUnRegister()
        {
            _director.Resume();
            _bgmManager.OnJustChangedBeat -= TutorialIndicatorGenerate;
            _inputBuffer.Attack.started -= OnShot;
            _inputBuffer.Attack.started -= OnSkill;
            foreach (var ind in _activeRingIndicator)
            {
                ind.End();
            }
            _activeRingIndicator.Clear();
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
            _currentIndicatorCount++;

            if (_chartKindEnum == ChartKindEnum.Attack)
            {

                if (_currentTargetClearCount >= _attackTutorialClearCount)
                {
                    Debug.Log("Tutorial Clear!----------------------------------------------------");
                    _currentTargetClearCount = 0;
                    _inputBuffer.Attack.started -= OnShot;
                    TutorialUnRegister();
                }
            }
        }

        /// <summary>
        /// チュートリアル用のショット処理
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnShot(InputAction.CallbackContext callbackContext)
        {
            if (_activeRingIndicator.Count == 0) return;
            Debug.Log(_currentIndicatorCount);
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
                        SoundEffectManager.PlaySoundEffect(_perfectAttackSound);
                    }
                    else
                    {
                        Debug.Log("Good!");
                        playerIndicator.PlayGoodEffect();
                        SoundEffectManager.PlaySoundEffect(_comboAttackSound);
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

        private void OnSkill(InputAction.CallbackContext callbackContext)
        {
            if (_activeRingIndicator.Count == 0) return;

            if (callbackContext.phase == InputActionPhase.Started)
            {
                var isGood = CheckGood();
                var isPerfect = CheckPerfect();
                var specitalIndicator = (SpecialIndicator)_activeRingIndicator[0];
                if (isGood)
                {
                    _currentTargetClearCount++;
                    Debug.Log("Good!");
                    specitalIndicator.PlaySuccessEffectPublic();

                }
                else
                {
                    Debug.Log("Missed!");
                    specitalIndicator.PlayFailEffect();
                    _activeRingIndicator.RemoveAt(0);
                }
            }
            if (_currentTargetClearCount >= _skillTutorialClearCount)
            {
                _currentTargetClearCount = 0;
                _inputBuffer.Attack.started -= OnSkill;
                TutorialUnRegister();
            }
        }

        private bool CheckGood()
        {
            if (_currentIndicatorCount == _indicatorGenerateCount - 1 || _currentIndicatorCount == _indicatorGenerateCount)
            {
                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                Debug.Log($"Normalized Timing from Just: {normalizedTimingFromJust}");

                // Justタイミング付近か判定
                return Mathf.Abs(normalizedTimingFromJust - 0.5f) <= _goodRange / 2;
            }
            return false;
        }
        private bool CheckPerfect()
        {
            if (_currentIndicatorCount == _indicatorGenerateCount - 1 || _currentIndicatorCount == _indicatorGenerateCount)
            {

                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                // Justタイミング付近か判定
                Debug.Log($"Normalized Timing from Just: {normalizedTimingFromJust}");
                return Mathf.Abs(normalizedTimingFromJust - 0.5f) <= _perfectRange / 2;
            }
            return false;
        }
    }
}
