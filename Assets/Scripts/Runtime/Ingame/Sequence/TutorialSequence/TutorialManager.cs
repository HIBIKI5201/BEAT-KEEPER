using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject _tutorialUi;
        [SerializeField] private Text _tutorialText;

        [Header("Playable")]
        [SerializeField] private PlayableDirector _director;

        [Header("リング")]
        [SerializeField] private UIElement_ChartRingManager _chartRingManager;
        [SerializeField, Tooltip("何拍ごとにインジケーターを出すか")] private int _indicatorGenerateCount;
        [SerializeField] private float _goodRange = 0.8f;
        [SerializeField] private float _perfectRange = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _indicatorSpeed = 1.0f;
        [SerializeField] private AnimationCurve _indicatorPosition;

        [Header("チュートリアルの設定")]
        [SerializeField, Tooltip("チュートリアルをプレイするかどうか")] private bool _playTutorial = true;
        [SerializeField] private int _attackTutorialClearCount = 4;
        [SerializeField] private int _skillTutorialClearCount = 1;
        [SerializeField] private int _missCount;

        [Header("チュートリアル用のテキスト")]
        [SerializeField] private string _attackIndicatorText;
        [SerializeField] private string _skillIndicatorText;
        [SerializeField] private string _enemyIndicatorText;
        [SerializeField] private string _chargeIndicatorText1;
        [SerializeField] private string _chargeIndicatorText2;

        [Header("サウンドエフェクトの名前")]
        [SerializeField] private string _ringNormalSound;
        [SerializeField] private string _comboAttackSound;
        [SerializeField] private string _perfectAttackSound;
        [SerializeField] private string _ringSkillSound;
        [SerializeField] private string _ringAvoidSound;
        [SerializeField] private string _dodgeSound;
        [SerializeField] private string _chargeSound;
        [SerializeField] private string _charging;
        [SerializeField] private string _chargeComplete;
        [SerializeField] private string _chargeGunshot;

        [Header("ボイス")]
        [SerializeField] private GameObject _textBox;
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        [SerializeField] private string _tutorialSuccess1;
        [SerializeField] private string _tutorialAttackStartVoice;
        [SerializeField] private string _tutorialSuccess2;
        [SerializeField] private string _tutorialField;
        [SerializeField] private string _chargeStartVoice;
        [SerializeField] private string _chargeCompleteVoice;

        private ChartKindEnum _chartKindEnum;

        private Queue<RingIndicatorBase> _activeIndicatorBaseQue = new();
        private BGMManager _bgmManager;
        private InputBuffer _inputBuffer;
        private PlayerManager _playerManager;
        private PlayerAnimeManager _playerAnimeManager;
        private EnemyAnimeManager _enemyAnimeManager;
        private int _currentIndicatorCount = 0;
        private int _currentTargetClearCount = 0;
        private int _currentChargeBeat;
        private int _allBeat;
        private float _indicatorTimer;
        private bool _isCharging;
        private bool _nextTutorial;
        private bool _operationTutorialPlaying;
        private bool _chargeAttackWaiting;
        private bool _indicatorGenerate = true;

        private async void Start()
        {
            _tutorialUi.SetActive(false);
            _chartKindEnum = ChartKindEnum.None;
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            _inputBuffer = await ServiceLocator.GetInstanceAsync<InputBuffer>();
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            _playerAnimeManager = _playerManager.GetPlayerAnimeManager();
            var buleSceneManager = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();
            var enemy = buleSceneManager.EnemyAdmin.GetActiveEnemy();
            _enemyAnimeManager = enemy.GetEnemyAnimeManager();
        }

        private void OnDestroy()
        {
            TutorialUnRegister();
        }

        public void StartTutorial()
        {
            _director.Play();
        }

        public void PlayVoice(string cueName, string text)
        {
            VoiceManager.PlayVoice(cueName);
            _textBox.SetActive(true);
            _textMeshPro.text = text;
#if UNITY_EDITOR
            if (!_playTutorial)
            {
                double duration = _director.playableAsset.duration;
                double jumpTime = duration * 0.95;

                _director.time = jumpTime;
                _director.Evaluate();
            }
#endif
        }

        public void OperationTutorialStart(ChartKindEnum chartKindEnum)
        {
            StartCoroutine(Explanation(chartKindEnum));
        }

        public void TutorialRegister(ChartKindEnum chartKindEnum)
        {
            if (_chartKindEnum == chartKindEnum) return;

            _chartKindEnum = chartKindEnum;
            _director.Pause();
            StartCoroutine(TutorialStartCoroutine(chartKindEnum));
        }

        private IEnumerator TutorialStartCoroutine(ChartKindEnum chartKindEnum)
        {
            yield return new WaitUntil(() => !_operationTutorialPlaying);
            if (chartKindEnum == ChartKindEnum.Attack)
                VoiceManager.PlayVoice(_tutorialAttackStartVoice);
            switch (chartKindEnum)
            {
                case ChartKindEnum.Attack:
                    _inputBuffer.Attack.started += OnShot;
                    break;
                case ChartKindEnum.Skill:
                    _inputBuffer.Attack.started += OnSkill;
                    break;
                case ChartKindEnum.Normal:
                    _inputBuffer.Avoid.started += OnAvoid;
                    break;
                case ChartKindEnum.Charge:
                    _inputBuffer.Interact.started += OnCharge;
                    _inputBuffer.Interact.canceled += OnCharge;
                    break;
            }
            _bgmManager.OnJustChangedBeat += OnBeat;
        }

        public void TutorialUnRegister()
        {
            _currentIndicatorCount = 0;
            _director.Resume();
            _bgmManager.OnJustChangedBeat -= OnBeat;
            _inputBuffer.Attack.started -= OnShot;
            _inputBuffer.Attack.started -= OnSkill;
            _inputBuffer.Avoid.started -= OnAvoid;
            _inputBuffer.Interact.started -= OnCharge;
            _inputBuffer.Interact.canceled -= OnCharge;

            if (_chargeAttackWaiting)
            {
                _enemyAnimeManager.ChargeAttack();
            }

            while (_activeIndicatorBaseQue.Count > 0)
            {
                StartCoroutine(EndIndicator(_activeIndicatorBaseQue.Dequeue()));
            }
        }

        /// <summary>
        /// 毎拍行う処理
        /// </summary>
        public void OnBeat()
        {
            _allBeat++;
            if (_activeIndicatorBaseQue.Count > 0 && !_activeIndicatorBaseQue.Peek().CheckRemainTime())
                EndIndicator(_activeIndicatorBaseQue.Dequeue());

            //表示されているすべてのリングのカウントを進める
            foreach (var indicator in _activeIndicatorBaseQue)
            {
                indicator.AddCount();
            }

            //インジケーターを生成
            if (_indicatorGenerateCount <= _currentIndicatorCount)
            {
                if (!_indicatorGenerate)
                {
                    _indicatorGenerate = true;
                    return;
                }

                var ringPosition = new Vector2(_indicatorPosition.Evaluate((_allBeat * _indicatorSpeed) % 10), 0);
                var ringObj = _chartRingManager.GenerateRing(_chartKindEnum, ringPosition, 0);
                var ringIndicator = ringObj.GetComponent<RingIndicatorBase>();
                if (ringIndicator) _activeIndicatorBaseQue.Enqueue(ringIndicator);

                _currentIndicatorCount = 0;
                switch (_chartKindEnum)
                {
                    case ChartKindEnum.Attack:
                        SoundEffectManager.PlaySoundEffect(_ringNormalSound);
                        break;
                    case ChartKindEnum.Skill:
                        SoundEffectManager.PlaySoundEffect(_ringSkillSound);
                        break;
                    case ChartKindEnum.Normal:
                        SoundEffectManager.PlaySoundEffect(_ringAvoidSound);
                        _enemyAnimeManager.PreAttack();
                        StartCoroutine(EnemyDelayAnimation());
                        break;
                    case ChartKindEnum.Charge:
                        //チャージ攻撃はほかのものより２倍のインターバルをかける
                        SoundEffectManager.PlaySoundEffect(_chargeSound);
                        _enemyAnimeManager.PreChargeAttack();
                        _chargeAttackWaiting = true;
                        _indicatorGenerate = false;
                        break;
                }
            }
            _currentIndicatorCount++;

            if (_isCharging) _currentChargeBeat++;

            //チュートリアルをクリアしているか確認する
            switch (_chartKindEnum)
            {
                case ChartKindEnum.Attack:
                    CheckTutorialClear(_attackTutorialClearCount, () =>
                    {
                        _inputBuffer.Attack.started -= OnShot;
                        VoiceManager.PlayVoice(_tutorialSuccess2);
                    })
            ;
                    break;
                case ChartKindEnum.Skill:
                    CheckTutorialClear(_skillTutorialClearCount, () => _inputBuffer.Attack.started -= OnSkill);
                    break;
                case ChartKindEnum.Normal:
                    CheckTutorialClear(_skillTutorialClearCount, () => _inputBuffer.Avoid.started -= OnAvoid);
                    break;
                case ChartKindEnum.Charge:
                    CheckTutorialClear(_skillTutorialClearCount, () =>
                    {
                        _inputBuffer.Interact.started -= OnCharge;
                        _inputBuffer.Interact.canceled -= OnCharge;
                    });
                    break;
            }
        }

        #region 共通処理

        /// <summary>
        /// チュートリアルをクリアしているかを確認し、終了処理を行う
        /// </summary>
        /// <param name="requiredCount"></param>
        /// <param name="unregisterAction"></param>
        private void CheckTutorialClear(int requiredCount, Action unregisterAction)
        {
            if (_currentTargetClearCount >= requiredCount)
            {
                Debug.Log("Tutorial Clear!----------------------------------------------------");
                _currentTargetClearCount = 0;
                unregisterAction?.Invoke();
                TutorialUnRegister();
            }
        }

        private bool CheckGood()
        {
            //インジケーターが生成されるのは_currentIndicatorCountが4の際
            if (_currentIndicatorCount == 3)
            {
                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                return Mathf.Abs(normalizedTimingFromJust - 0.5f) <= _goodRange / 2;
            }
            return false;
        }
        private bool CheckPerfect()
        {
            if (_currentIndicatorCount == 3)
            {
                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                return Mathf.Abs(normalizedTimingFromJust - 0.5f) <= _perfectRange / 2;
            }
            return false;
        }

        /// <summary>
        /// チュートリアル時に行うメッセージの表示および操作を待機する処理
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inputAction"></param>
        /// <returns></returns>
        private IEnumerator ShowTutorialMessage(string message, InputAction inputAction)
        {
            _tutorialUi.SetActive(true);
            _tutorialText.text = message;
            inputAction.started += OnWaitInput;
            yield return new WaitUntil(() => _nextTutorial);
            inputAction.started -= OnWaitInput;
            _tutorialUi.SetActive(false);
            _nextTutorial = false;
        }

        /// <summary>
        /// インジケーターを処理する共通関数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="successCondition"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFail"></param>
        /// <returns></returns>
        private bool HandleIndicator<T>(Func<bool> successCondition, Action<T> onSuccess, Action<T> onFail) where T : RingIndicatorBase
        {
            if (_activeIndicatorBaseQue.Count <= 0) return false;
            var indicator = _activeIndicatorBaseQue.Dequeue() as T;
            if (indicator == null) return false;

            if (successCondition())
            {
                _currentTargetClearCount++;
                onSuccess?.Invoke(indicator);
            }
            else
            {
                onFail?.Invoke(indicator);
            }
            EndIndicator(indicator);
            return true;
        }

        /// <summary>
        /// リングを１拍後に終了する処理
        /// </summary>
        /// <param name="ringIndicatorBase"></param>
        /// <returns></returns>
        private IEnumerator EndIndicator(RingIndicatorBase ringIndicatorBase)
        {
            yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat);
            ringIndicatorBase.End();
        }

        private IEnumerator EnemyDelayAnimation()
        {
            yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 4);
            _enemyAnimeManager.Attack();
        }

        #endregion

        #region チュートリアル専用の操作

        /// <summary>
        /// 通常攻撃用の処理
        /// </summary>
        /// <param name="ctx"></param>

        private void OnShot(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Started || _indicatorTimer >= Time.time || _activeIndicatorBaseQue.Count == 0) return;
            _indicatorTimer = Time.time + (float)MusicEngineHelper.DurationOfBeat * _missCount;

            HandleIndicator<PlayerIndicator>(
                () => CheckGood(),
                ind =>
                {
                    if (CheckPerfect())
                    {
                        ind.PlayPerfectEffect();
                        SoundEffectManager.PlaySoundEffect(_perfectAttackSound);
                    }
                    else
                    {
                        ind.PlayGoodEffect();
                        SoundEffectManager.PlaySoundEffect(_comboAttackSound);
                    }
                    _playerAnimeManager.Shoot();
                },
                ind =>
                {
                    ind.PlayFailEffect();
                    VoiceManager.PlayVoice(_tutorialField);
                    _indicatorGenerate = false;
                }
            );
        }

        /// <summary>
        /// スキル用の処理
        /// </summary>
        /// <param name="ctx"></param>
        private void OnSkill(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Started || _indicatorTimer >= Time.time || _activeIndicatorBaseQue.Count == 0) return;
            _indicatorTimer = Time.time + (float)MusicEngineHelper.DurationOfBeat * _missCount;

            HandleIndicator<SpecialIndicator>(
                () => CheckGood(),
                ind =>
                {
                    ind.PlaySuccessEffectPublic();
                    _playerAnimeManager.Skill();
                },
                ind => ind.PlayFailEffect()
            );
        }

        /// <summary>
        /// 回避時の処理
        /// </summary>
        /// <param name="ctx"></param>
        private void OnAvoid(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Started || _indicatorTimer >= Time.time || _activeIndicatorBaseQue.Count == 0) return;
            _indicatorTimer = Time.time + (float)MusicEngineHelper.DurationOfBeat * _missCount;
            HandleIndicator<EnemyIndicator>(
                () => CheckGood(),
                ind =>
                {
                    ind.OnPlayerAvoidSuccess(true);
                    SoundEffectManager.PlaySoundEffect(_dodgeSound);
                    _playerAnimeManager.Avoid();
                    _playerManager.FlowZoneSystem.SuccessResonance();
                },
                ind => ind.PlayFailEffect()
            );
        }

        /// <summary>
        /// チャージ攻撃用の処理
        /// </summary>
        /// <param name="ctx"></param>
        private void OnCharge(InputAction.CallbackContext ctx)
        {
            if (_activeIndicatorBaseQue.Count == 0) return;
            var chargeIndicator = (ChargeIndicator)_activeIndicatorBaseQue.Peek();

            if (ctx.phase == InputActionPhase.Started)
            {
                //チャージ開始時の処理
                var normalizedTiming = (float)Music.UnitFromJust;
                if (Mathf.Abs(normalizedTiming - 0.5f) <= _goodRange / 2 && _currentIndicatorCount == 3)
                {
                    _isCharging = true;
                    _currentChargeBeat = 0;
                    chargeIndicator.OnPlayerChargeTutorial();
                    SoundEffectManager.PlaySoundEffect(_charging);
                    _playerAnimeManager.ChargeShoot();
                    _enemyAnimeManager.PreChargeAttack();
                    _enemyAnimeManager.KnockBack(true);
                    _chargeAttackWaiting = false;
                }
                else
                {
                    chargeIndicator.PlayFailEffect();
                    _enemyAnimeManager.ChargeAttack();
                    _enemyAnimeManager.KnockBack(false);
                    _chargeAttackWaiting = false;
                    EndIndicator(_activeIndicatorBaseQue.Dequeue());
                }
            }
            else if (ctx.phase == InputActionPhase.Canceled)
            {
                //チャージ完了時の処理
                if (!_isCharging) return;
                if (_currentChargeBeat == 3 || _currentChargeBeat == 2)
                {
                    var normalizedTiming = (float)Music.UnitFromJust;
                    if (Mathf.Abs(normalizedTiming - 0.5f) <= _goodRange / 2)
                    {
                        _currentTargetClearCount++;
                        chargeIndicator.OnPlayerAttackSuccessTutorial();
                        SoundEffectManager.PlaySoundEffect(_chargeGunshot);
                        _enemyAnimeManager.ChargeAttack();
                    }
                    else
                    {
                        chargeIndicator.PlayFailEffect();
                    }
                }
                else
                {
                    chargeIndicator.PlayFailEffect();
                }
                EndIndicator(_activeIndicatorBaseQue.Dequeue());
                _isCharging = false;
                _currentChargeBeat = 0;
            }
        }

        #endregion

        #region 操作説明のコード

        private IEnumerator Explanation(ChartKindEnum chartKindEnum)
        {
            _operationTutorialPlaying = true;
            var ring = _chartRingManager.GenerateRing(chartKindEnum, Vector2.zero, 0);

            if (chartKindEnum == ChartKindEnum.Attack)
            {
                var ringObj = ring.GetComponent<PlayerIndicator>();
                ringObj.AddCount();
                SoundEffectManager.PlaySoundEffect(_ringNormalSound);
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                yield return ShowTutorialMessage(_attackIndicatorText, _inputBuffer.Attack);
                _playerAnimeManager.Shoot();
                ringObj.Resume();
                ringObj.PlayPerfectEffect();
                SoundEffectManager.PlaySoundEffect(_perfectAttackSound);
                VoiceManager.PlayVoice(_tutorialSuccess1);
            }
            else if (chartKindEnum == ChartKindEnum.Skill)
            {
                var ringObj = ring.GetComponent<SpecialIndicator>();
                ringObj.AddCount();
                SoundEffectManager.PlaySoundEffect(_ringSkillSound);
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                yield return ShowTutorialMessage(_skillIndicatorText, _inputBuffer.Attack);
                _playerAnimeManager.Skill();
                ringObj.Resume();
                ringObj.PlaySuccessEffectPublic();
            }
            else if (chartKindEnum == ChartKindEnum.Normal)
            {
                var ringObj = ring.GetComponent<EnemyIndicator>();
                ringObj.AddCount();
                SoundEffectManager.PlaySoundEffect(_ringAvoidSound);
                _enemyAnimeManager.PreAttack();
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                yield return ShowTutorialMessage(_enemyIndicatorText, _inputBuffer.Avoid);
                _playerManager.FlowZoneSystem.SuccessResonance();
                _playerAnimeManager.Avoid();
                _enemyAnimeManager.Attack();
                ringObj.Resume();
                ringObj.OnPlayerAvoidSuccess(true);
                SoundEffectManager.PlaySoundEffect(_dodgeSound);
            }
            else if (chartKindEnum == ChartKindEnum.Charge)
            {
                var ringObj = ring.GetComponent<ChargeIndicator>();
                ringObj.AddCount();
                SoundEffectManager.PlaySoundEffect(_chargeSound);
                _enemyAnimeManager.ChargeAttack();
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                _inputBuffer.Interact.started += OnWaitInput;
                _inputBuffer.Interact.canceled += OnWaitInput;
                VoiceManager.PlayVoice(_chargeStartVoice);
                yield return ShowTutorialMessage(_chargeIndicatorText1, _inputBuffer.Interact);
                ringObj.Resume();
                ringObj.OnPlayerChargeTutorial();

                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 3);
                VoiceManager.PlayVoice(_chargeCompleteVoice);
                _tutorialUi.SetActive(true);
                _tutorialText.text = _chargeIndicatorText2;
                SoundEffectManager.PlaySoundEffect(_chargeComplete);

                ringObj.Pause();
                yield return new WaitUntil(() => _nextTutorial);

                _enemyAnimeManager.PreChargeAttack();
                _enemyAnimeManager.KnockBack(true);
                _tutorialUi.SetActive(false);
                ringObj.OnPlayerAttackSuccessTutorial();
                _inputBuffer.Interact.canceled -= OnWaitInput;
                _nextTutorial = false;
                ringObj.Resume();
                SoundEffectManager.PlaySoundEffect(_chargeGunshot);
            }
            yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2.5f);

            _operationTutorialPlaying = false;
            ring.GetComponent<RingIndicatorBase>().End();
        }

        /// <summary>
        /// 入力を待機するための関数
        /// </summary>
        /// <param name="ctx"></param>
        private void OnWaitInput(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Started || ctx.phase == InputActionPhase.Canceled)
            {
                _nextTutorial = true;
            }
        }

        #endregion
    }
}
