using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System.Collections;
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
        private int _currentChargeBeat;
        private bool _isCharging;
        private bool _nextTutorial;
        private bool _operationTutorialPlaying;

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

        public void OperationTutorialStart(ChartKindEnum chartKindEnum)
        {
            Debug.Log("OperationTutorialStart");
            StartCoroutine(Explanation(chartKindEnum));
        }

        public void TutorialRegister(ChartKindEnum chartKindEnum)
        {
            if (!_playTutorial) return;
            if (_chartKindEnum == chartKindEnum) return;
            _chartKindEnum = chartKindEnum;
            _director.Pause();
            StartCoroutine(TutorialStartCoroutine(chartKindEnum));
        }

        private IEnumerator TutorialStartCoroutine(ChartKindEnum chartKindEnum)
        {
            yield return new WaitUntil(() => !_operationTutorialPlaying);
            if (_chartKindEnum == ChartKindEnum.Attack)
            {
                _inputBuffer.Attack.started += OnShot;
            }
            else if (chartKindEnum == ChartKindEnum.Skill)
            {
                _inputBuffer.Attack.started += OnSkill;
            }
            else if (chartKindEnum == ChartKindEnum.Normal)
            {
                _inputBuffer.Avoid.started += OnAvoid;
            }
            else if (chartKindEnum == ChartKindEnum.Charge)
            {
                _inputBuffer.Interact.started += OnCharge;
                _inputBuffer.Interact.canceled += OnCharge;
            }
            _bgmManager.OnJustChangedBeat += TutorialIndicatorGenerate;
        }

        public void TutorialUnRegister()
        {
            _director.Resume();
            _bgmManager.OnJustChangedBeat -= TutorialIndicatorGenerate;
            _inputBuffer.Attack.started -= OnShot;
            _inputBuffer.Attack.started -= OnSkill;
            _inputBuffer.Avoid.started -= OnAvoid;
            _inputBuffer.Interact.started -= OnCharge;
            _inputBuffer.Interact.canceled -= OnCharge;
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

            if (_isCharging) _currentChargeBeat++;

            #region チュートリアルクリア判定

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
            else if (_chartKindEnum == ChartKindEnum.Skill)
            {
                if (_currentTargetClearCount >= _skillTutorialClearCount)
                {
                    Debug.Log("Tutorial Clear!----------------------------------------------------");
                    _currentTargetClearCount = 0;
                    _inputBuffer.Attack.started -= OnSkill;
                    TutorialUnRegister();
                }
            }
            else if (_chartKindEnum == ChartKindEnum.Normal)
            {
                if (_currentTargetClearCount >= _skillTutorialClearCount)
                {
                    Debug.Log("Tutorial Clear!----------------------------------------------------");
                    _currentTargetClearCount = 0;
                    _inputBuffer.Avoid.started -= OnAvoid;
                    TutorialUnRegister();
                }
            }
            else
            {
                if (_currentTargetClearCount >= _skillTutorialClearCount)
                {
                    Debug.Log("Tutorial Clear!----------------------------------------------------");
                    _currentTargetClearCount = 0;
                    _inputBuffer.Interact.started -= OnCharge;
                    _inputBuffer.Interact.canceled -= OnCharge;
                    TutorialUnRegister();
                }
            }
            #endregion
        }

        #region チュートリアル専用の操作

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

        private void OnAvoid(InputAction.CallbackContext callbackContext)
        {
            if (_activeRingIndicator.Count == 0) return;
            Debug.Log(_currentIndicatorCount);
            var avoidIndicator = (EnemyIndicator)_activeRingIndicator[0];
            if (callbackContext.phase == InputActionPhase.Started)
            {
                var isGood = CheckGood();
                var isPerfect = CheckPerfect();
                if (isGood)
                {
                    _currentTargetClearCount++;
                    Debug.Log("Good!");
                    avoidIndicator.OnPlayerAvoidSuccess(isPerfect);
                }
                else
                {
                    Debug.Log("Missed!");
                    avoidIndicator.PlayFailEffect();
                    _activeRingIndicator.RemoveAt(0);
                }
            }
        }

        private void OnCharge(InputAction.CallbackContext callbackContext)
        {
            if (_activeRingIndicator.Count == 0) return;
            var chargeIndicator = (ChargeIndicator)_activeRingIndicator[0];

            if (callbackContext.phase == InputActionPhase.Started)
            {
                if (_currentIndicatorCount == 3)
                {
                    var normalizedTiming = (float)Music.UnitFromJust;
                    if (Mathf.Abs(normalizedTiming - 0.5f) <= _goodRange / 2)
                    {
                        _isCharging = true;
                        _currentChargeBeat = 0;
                        chargeIndicator.OnPlayerChargeTutorial();
                    }
                    else
                    {
                        chargeIndicator.PlayFailEffect();
                        _activeRingIndicator.RemoveAt(0);
                    }
                }
                else
                {
                    chargeIndicator.PlayFailEffect();
                    _activeRingIndicator.RemoveAt(0);
                }
            }
            else if (callbackContext.phase == InputActionPhase.Canceled)
            {
                if (!_isCharging) return;

                if (_currentChargeBeat == 3 || _currentChargeBeat == 2)
                {
                    var normalizedTiming = (float)Music.UnitFromJust;
                    // 拍の真ん中で離せているか？
                    if (Mathf.Abs(normalizedTiming - 0.5f) <= _goodRange / 2)
                    {
                        _currentTargetClearCount++;
                        chargeIndicator.OnPlayerAttackSuccessTutorial();
                    }
                    else
                    {
                        chargeIndicator.PlayFailEffect();
                        _activeRingIndicator.RemoveAt(0);
                    }
                }
                else
                {
                    Debug.Log(_currentChargeBeat);
                    chargeIndicator.PlayFailEffect();
                    _activeRingIndicator.RemoveAt(0);
                }

                _isCharging = false;
                _currentChargeBeat = 0;
            }
        }

        #endregion

        #region 操作説明の操作
        private IEnumerator Explanation(ChartKindEnum chartKindEnum)
        {
            _operationTutorialPlaying = true;
            if (chartKindEnum == ChartKindEnum.Attack)
            {
                var ringObj = _chartRingManager.GenerateRing(chartKindEnum, Vector2.zero, 0).GetComponent<PlayerIndicator>();
                ringObj.AddCount();
                yield return new WaitForNextBeat(2);

                ringObj.Pause();
                _inputBuffer.Attack.started += OnWaitInput;
                yield return new WaitUntil(() => _nextTutorial);

                _inputBuffer.Attack.started -= OnWaitInput;
                _nextTutorial = false;
                ringObj.Resume();
                ringObj.PlayPerfectEffect();
                SoundEffectManager.PlaySoundEffect(_perfectAttackSound);
            }
            else if (chartKindEnum == ChartKindEnum.Skill)
            {
                var ringObj = _chartRingManager.GenerateRing(chartKindEnum, Vector2.zero, 0).GetComponent<SpecialIndicator>();
                ringObj.AddCount();
                yield return new WaitForNextBeat(2);
                ringObj.Pause();
                _inputBuffer.Attack.started += OnWaitInput;
                yield return new WaitUntil(() => _nextTutorial);
                _inputBuffer.Attack.started -= OnWaitInput;
                _nextTutorial = false;
                ringObj.Resume();
                ringObj.PlaySuccessEffectPublic();
            }
            else if (chartKindEnum == ChartKindEnum.Normal)
            {
                var ringObj = _chartRingManager.GenerateRing(chartKindEnum, Vector2.zero, 0).GetComponent<EnemyIndicator>();
                ringObj.AddCount();
                yield return new WaitForNextBeat(2);
                ringObj.Pause();
                _inputBuffer.Avoid.started += OnWaitInput;
                yield return new WaitUntil(() => _nextTutorial);
                _inputBuffer.Avoid.started -= OnWaitInput;
                _nextTutorial = false;
                ringObj.Resume();
                ringObj.OnPlayerAvoidSuccess(true);
            }
            else if (chartKindEnum == ChartKindEnum.Charge)
            {
                //リング生成
                var ringObj = _chartRingManager.GenerateRing(chartKindEnum, Vector2.zero, 0).GetComponent<ChargeIndicator>();
                //リングを動かし始める
                ringObj.AddCount();
                ringObj.AddCount();
                //リングが閉まるまで待機
                yield return new WaitForNextBeat(3);
                //リングを一時停止
                ringObj.Pause();
                //入力を登録
                _inputBuffer.Interact.started += OnWaitInput;
                _inputBuffer.Interact.canceled += OnWaitInput;
                //入力を待つ
                yield return new WaitUntil(() => _nextTutorial);
                _nextTutorial = false;
                // startedのイベントを解除
                _inputBuffer.Interact.started -= OnWaitInput;
                //一時停止を解除
                ringObj.Resume();
                // チャージの演出
                ringObj.OnPlayerChargeTutorial();
                // チャージが完了するまで待機
                yield return new WaitForNextBeat(2);
                //チャージが完了したら入力を待つ
                ringObj.Pause();
                yield return new WaitUntil(() => _nextTutorial);
                //　チャージ攻撃完了演出
                ringObj.OnPlayerAttackSuccessTutorial();
                _inputBuffer.Interact.canceled -= OnWaitInput;
                _nextTutorial = false;
                ringObj.Resume();
            }
            _operationTutorialPlaying = false;
        }

        private void OnWaitInput(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.phase == InputActionPhase.Started || callbackContext.phase == InputActionPhase.Canceled)
            {
                _nextTutorial = true;
            }
        }

        #endregion

        #region チュートリアル上での評価処理
        private bool CheckGood(float offset = 0)
        {
            if (_currentIndicatorCount == _indicatorGenerateCount - 1 || _currentIndicatorCount == _indicatorGenerateCount)
            {
                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                Debug.Log($"Normalized Timing from Just: {normalizedTimingFromJust}");

                // Justタイミング付近か判定
                return Mathf.Abs(normalizedTimingFromJust - 0.5f + offset) <= _goodRange / 2;
            }
            return false;
        }
        private bool CheckPerfect(float offset = 0)
        {
            if (_currentIndicatorCount == _indicatorGenerateCount - 1 || _currentIndicatorCount == _indicatorGenerateCount)
            {

                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                // Justタイミング付近か判定
                Debug.Log($"Normalized Timing from Just: {normalizedTimingFromJust}");
                return Mathf.Abs(normalizedTimingFromJust - 0.5f + offset) <= _perfectRange / 2;
            }
            return false;
        }
        #endregion
    }
}
