using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class TutorialManager : MonoBehaviour
    {
        private readonly int ChargeHash = Animator.StringToHash("Beam_01");
        [Header("UI")]
        [SerializeField] private GameObject _tutorialUi;
        [SerializeField] private Text _tutorialText;
        [SerializeField] private Image _tutorialFocusImage;
        [SerializeField] private Vector2 _defaultFocusPosition;
        [SerializeField] private Vector2 _chargeEndFocusPosition;
        [SerializeField] private SubTitleManager _subTitleManager;

        [Header("Playable")]
        [SerializeField] private PlayableDirector _director;

        [Header("リング")]
        [SerializeField] private UIElement_ChartRingManager _chartRingManager;
        [SerializeField, Tooltip("何拍ごとにインジケーターを出すか")] private int _indicatorGenerateCount;
        [SerializeField] private float _goodRange = 0.8f;
        [SerializeField] private float _perfectRange = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _indicatorSpeed = 1.0f;
        [SerializeField] private AnimationCurve _indicatorPosition;
        [SerializeField] private Vector2 _tutorialFocusFlow;

        [Header("チュートリアルの設定")]
        [SerializeField, Tooltip("一定回数失敗でスキップするかどうか")] private bool _skipOnMiss = true;
        [SerializeField, Tooltip("ミスをしたときに一定拍数判定を取れなくする")] private int _InvalidationTime;
        [SerializeField] private int _attackTutorialClearCount = 4;
        [SerializeField] private int _attackMissCount;
        [SerializeField] private int _othersTutorialClearCount = 1;
        [SerializeField] private int _othersMissCount;
        [SerializeField] private float _skipTime = 1.5f;

        [Header("チュートリアル用のテキスト")]
        [SerializeField] private string _attackIndicatorKey;
        [SerializeField] private string _skillIndicatorKey;
        [SerializeField] private string _enemyIndicatorKey;
        [SerializeField] private string _chargeIndicatorKey1;
        [SerializeField] private string _chargeIndicatorKey2;

        [Header("サウンドエフェクトの名前")]
        [SerializeField] private string _tutorialSE;
        [SerializeField] private string _ringNormalSound;
        [SerializeField] private string _comboAttackSound;
        [SerializeField] private string _perfectAttackSound;
        [SerializeField] private string _ringSkillSound;
        [SerializeField] private string _skillSuccessSound;
        [SerializeField] private string _ringAvoidSound;
        [SerializeField] private string _dodgeSound;
        [SerializeField] private string _avoidMissSound;
        [SerializeField] private string _chargeSound;
        [SerializeField] private string _charging;
        [SerializeField] private string _chargeComplete;
        [SerializeField] private string _chargeGunshot;
        [SerializeField] private string _chargeMissSound;

        [Header("ボイス")]
        [SerializeField] private bool _useSubtitle = true;
        [SerializeField] private string _tutorialSuccess1;
        [SerializeField] private string _tutorialAttackStartVoice;
        [SerializeField] private string _tutorialSuccess2;
        [SerializeField] private string _tutorialField;
        [SerializeField] private string _flowZone1;
        [SerializeField] private string _chargeStartVoice;
        [SerializeField] private string _chargeCompleteVoice;

        [Header("スキル発動ボイス")]
        [SerializeField] private string _attackNormalVoiceName;
        [SerializeField] private string _avoidVoiceName;
        [SerializeField] private string _chargeStartVoiceName;
        [SerializeField] private string _chargeEndVoiceName;
        [SerializeField] private string _skillVoiceName;
        [SerializeField] private string _missAvoidVoice;
        [SerializeField] private string _missChargeVoice;

        [Header("エフェクトのGameObject名")]
        [SerializeField] private string _playerSkillEffectName;
        [SerializeField] private string[] _enemyBeamEffectNames;

        private ChartKindEnum _chartKindEnum;

        private RingIndicatorBase _activeIndicator;
        private BGMManager _bgmManager;
        private InputBuffer _inputBuffer;
        private PlayerManager _playerManager;
        private PlayerAnimeManager _playerAnimeManager;
        private EnemyAnimeManager _enemyAnimeManager;
        private LocalizeTextManager _localizeTextManager;
        private ParticleSystem _skillEffect;
        private Animator[] _enemyBeamEffects;
        private Coroutine _tutorialCoroutine;
        private int _currentIndicatorCount = 0;
        private int _currentTargetClearCount = 0;
        private int _currentChargeBeat;
        private int _allBeat;
        private int _generateInterval;
        private int _currentMissCount;
        private float _indicatorTimer;
        private float _skipThresholdTime;
        private bool _nextTutorial;
        private bool _operationTutorialPlaying;
        private bool _chargeAttackWaiting;
        private bool _isCharging;
        private bool _isIndicatorWaitForInput;
        private bool _operationTutorialEnemyAttacking;
        private bool _operationTutorialEnemyCharging;
        private bool _isPushSkipButton;
        private async void Start()
        {
            _tutorialUi.SetActive(false);
            _tutorialFocusImage.enabled = false;
            _chartKindEnum = ChartKindEnum.None;
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            _inputBuffer = await ServiceLocator.GetInstanceAsync<InputBuffer>();
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            _playerAnimeManager = _playerManager.GetPlayerAnimeManager();
            var battleSceneManager = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();
            var enemy = battleSceneManager.EnemyAdmin.GetActiveEnemy();
            _enemyAnimeManager = enemy.GetEnemyAnimeManager();
            _localizeTextManager = await ServiceLocator.GetInstanceAsync<LocalizeTextManager>();

            _skillEffect = _playerManager.gameObject.transform.Find(_playerSkillEffectName).gameObject.GetComponent<ParticleSystem>();

            _enemyBeamEffects = new Animator[_enemyBeamEffectNames.Length];
            for (int i = 0; i < _enemyBeamEffectNames.Length; i++)
            {
                _enemyBeamEffects[i] = enemy.transform.Find(_enemyBeamEffectNames[i]).GetComponent<Animator>();
            }
        }

        private void Update()
        {
            if (_isPushSkipButton && Time.time >= _skipThresholdTime)
            {
                SkipTutorial();
                _isPushSkipButton = false;
            }
        }

        public void EndTutorial()
        {
            _tutorialUi.SetActive(false);
            _inputBuffer.Quit.started -= OnSkipTutorial;
            _inputBuffer.Quit.canceled -= OnSkipTutorial;
        }

        public void StartTutorial()
        {
            _director.Play();
            _inputBuffer.Quit.started += OnSkipTutorial;
            _inputBuffer.Quit.canceled += OnSkipTutorial;
        }

        public void PlayVoice(string cueName)
        {
            VoiceManager.PlayVoice(cueName);
            if (!_localizeTextManager.DontUseSubtitle) _subTitleManager.TutorialSubtitleTextShow(cueName);
            if (cueName == _flowZone1)
            {
                _tutorialFocusImage.GetComponent<RectTransform>().anchoredPosition = _tutorialFocusFlow;
                _tutorialFocusImage.enabled = true;
            }
        }

        public void OperationTutorialStart(ChartKindEnum chartKindEnum)
        {
            _tutorialCoroutine = StartCoroutine(Explanation(chartKindEnum));
        }

        public void TutorialRegister(ChartKindEnum chartKindEnum)
        {
            if (_chartKindEnum == chartKindEnum) return;

            _currentMissCount = 0;
            _currentTargetClearCount = 0;
            _chartKindEnum = chartKindEnum;
            _director.Pause();
            StartCoroutine(TutorialStartCoroutine(chartKindEnum));
        }

        private void SkipTutorial()
        {
            if (_tutorialCoroutine != null)
            {
                StopCoroutine(_tutorialCoroutine);
                _tutorialCoroutine = null;
            }
            _inputBuffer.Attack.started -= OnWaitInput;
            _inputBuffer.Avoid.started -= OnWaitInput;
            _inputBuffer.Interact.started -= OnWaitInput;
            _inputBuffer.Interact.canceled -= OnWaitInput;

            _tutorialUi.SetActive(false);
            _tutorialFocusImage.enabled = false;

            if (_operationTutorialEnemyAttacking)
            {
                _enemyAnimeManager.Attack();
                _operationTutorialEnemyAttacking = false;
            }
            else if (_operationTutorialEnemyCharging)
            {
                _enemyAnimeManager.ChargeAttack();
                _enemyAnimeManager.KnockBack(false);
                _operationTutorialEnemyCharging = false;
            }

            _activeIndicator?.End();

            double duration = _director.playableAsset.duration;
            double jumpTime = duration * 0.95;
            _director.time = jumpTime;
            _director.Evaluate();

            TutorialUnRegister();
        }

        private IEnumerator TutorialStartCoroutine(ChartKindEnum chartKindEnum)
        {
            yield return new WaitUntil(() => !_operationTutorialPlaying);
            _tutorialFocusImage.enabled = false;
            if (chartKindEnum == ChartKindEnum.Attack)
            {
                PlayVoice(_tutorialAttackStartVoice);
                _generateInterval = 2;
            }
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
            _bgmManager.OnJustChangedBeat -= OnBeat;
            _inputBuffer.Attack.started -= OnShot;
            _inputBuffer.Attack.started -= OnSkill;
            _inputBuffer.Avoid.started -= OnAvoid;
            _inputBuffer.Interact.started -= OnCharge;
            _inputBuffer.Interact.canceled -= OnCharge;
            _currentIndicatorCount = 0;

            if (_chargeAttackWaiting)
            {
                _enemyAnimeManager.ChargeAttack();
            }

            if (_activeIndicator != null)
            {
                _activeIndicator.End();
                _activeIndicator = null;
            }
            _director.Resume();
        }

        /// <summary>
        /// 毎拍行う処理
        /// </summary>
        public void OnBeat()
        {
            _allBeat++;
            _currentIndicatorCount++;

            // インジケーターが存在する場合の処理
            if (_activeIndicator != null)
            {
                // ノーツにAddCountをする
                _activeIndicator.AddCount();

                // ノーツが時間切れを起こしているかを調べる。
                if (_activeIndicator.IsExpired())
                {
                    _activeIndicator.End();
                    _activeIndicator = null;
                    _currentMissCount++;
                    if (_chartKindEnum == ChartKindEnum.Charge)
                    {
                        _isCharging = false;
                        _currentChargeBeat = 0;
                        _playerAnimeManager.FatalHit();
                        _enemyAnimeManager.KnockBack(false);
                        _enemyAnimeManager.ChargeAttack();
                        VoiceManager.PlayVoice(_missChargeVoice);
                        SoundEffectManager.PlaySoundEffect(_chargeMissSound);
                        foreach (var item in _enemyBeamEffects)
                        {
                            item.Play(ChargeHash);
                        }
                    }
                    else if (_chartKindEnum == ChartKindEnum.Normal)
                    {
                        _playerAnimeManager.Hit();
                        VoiceManager.PlayVoice(_missAvoidVoice);
                        SoundEffectManager.PlaySoundEffect(_avoidMissSound);
                    }
                }
            }

            // インジケーター生成処理
            if (_indicatorGenerateCount <= _currentIndicatorCount)
            {
                _currentIndicatorCount = 0;
                //インターバルがあった場合は今回の生成をスキップ
                if (_generateInterval > 0)
                {
                    _generateInterval--;
                    return;
                }

                // インジケーターがまだ消えずに残っている場合は強制的に消す
                if (_activeIndicator != null)
                {
                    _activeIndicator.End();
                }

                // 新規インジケーター生成
                var ringPosition = new Vector2(_indicatorPosition.Evaluate((_allBeat * _indicatorSpeed) % 10), 0);
                var ringObj = _chartRingManager.GenerateRing(_chartKindEnum, ringPosition, 0);
                _activeIndicator = ringObj.GetComponent<RingIndicatorBase>();
                _isIndicatorWaitForInput = true;

                // 生成時の音を鳴らす。
                if (_chartKindEnum == ChartKindEnum.Attack) SoundEffectManager.PlaySoundEffect(_ringNormalSound);
                else if (_chartKindEnum == ChartKindEnum.Skill) SoundEffectManager.PlaySoundEffect(_ringSkillSound);
                else if (_chartKindEnum == ChartKindEnum.Normal)
                {
                    SoundEffectManager.PlaySoundEffect(_ringAvoidSound);

                    _enemyAnimeManager.PreAttack();
                    StartCoroutine(EnemyDelayAnimation());
                }
                else if (_chartKindEnum == ChartKindEnum.Charge)
                {
                    SoundEffectManager.PlaySoundEffect(_chargeSound);

                    _enemyAnimeManager.PreChargeAttack();
                    _chargeAttackWaiting = true;
                    _generateInterval = 2;
                }
            }

            if (_isCharging) _currentChargeBeat++;

            // チュートリアルをクリアしているかを調べる。
            bool clearCheck = _chartKindEnum == ChartKindEnum.Attack ?
                _attackTutorialClearCount <= _currentTargetClearCount : _othersTutorialClearCount <= _currentTargetClearCount;
            bool missCheck = _chartKindEnum == ChartKindEnum.Attack ?
                _attackMissCount <= _currentMissCount : _othersMissCount <= _currentMissCount;

            if (_skipOnMiss ? clearCheck || missCheck : clearCheck)
            {
                if (clearCheck && _chartKindEnum == ChartKindEnum.Attack)
                {
                    PlayVoice(_tutorialSuccess2);
                }
                _currentTargetClearCount = 0;
                _currentMissCount = 0;
                TutorialUnRegister();
            }
        }

        #region 共通処理

        private bool CheckGood()
        {
            //インジケーターが生成されるのは_currentIndicatorCountが4の際
            if (_currentIndicatorCount == 2 && _generateInterval == 0)
            {
                var normalizedTimingFromJust = (float)Music.UnitFromJust;
                if (Mathf.Abs(normalizedTimingFromJust - 0.5f) <= _goodRange / 2)
                {
                    return true;
                }
            }

            PlayVoice(_tutorialField);
            _currentMissCount++;
            if (_activeIndicator != null)
            {
                _activeIndicator.End();
                _activeIndicator = null;
            }
            _generateInterval = 2;
            return false;
        }
        private bool CheckPerfect()
        {
            if (_currentIndicatorCount == 2)
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
            //説明テキスト表示
            _tutorialUi.SetActive(true);
            _tutorialText.text = message;
            SoundEffectManager.PlaySoundEffect(_tutorialSE);

            //強調表示
            _tutorialFocusImage.enabled = true;
            _tutorialFocusImage.GetComponent<RectTransform>().anchoredPosition = _defaultFocusPosition;

            inputAction.started += OnWaitInput;

            //入力があるまで待機
            yield return new WaitUntil(() => _nextTutorial);

            //説明テキスト非表示
            _tutorialFocusImage.enabled = false;
            _tutorialUi.SetActive(false);
            inputAction.started -= OnWaitInput;
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
            var indicator = _activeIndicator as T;
            if (indicator == null) return false;

            // プレイヤー操作を受け付けたとき参照を消し、同じインジケーターで処理が行われないようにする
            _activeIndicator = null;
            _isIndicatorWaitForInput = false;

            if (successCondition())
            {
                _currentTargetClearCount++;
                onSuccess?.Invoke(indicator);
            }
            else
            {
                onFail?.Invoke(indicator);
            }
            StartCoroutine(EndIndicator(indicator));
            return true;
        }


        private IEnumerator EndIndicator(RingIndicatorBase ringIndicatorBase)
        {
            _activeIndicator = null;
            _isIndicatorWaitForInput = false;
            if (_isIndicatorWaitForInput)
            {
                if (ringIndicatorBase is ChargeIndicator && _chargeAttackWaiting)
                {
                    //チャージ中にリングが消える場合は失敗扱いにする
                    _currentChargeBeat = 0;
                    _isCharging = false;
                    _chargeAttackWaiting = false;
                    _enemyAnimeManager.ChargeAttack();
                    _enemyAnimeManager.KnockBack(false);
                    _playerAnimeManager.FatalHit();
                    SoundEffectManager.PlaySoundEffect(_chargeMissSound);
                    foreach (var item in _enemyBeamEffects)
                    {
                        item.Play(ChargeHash);
                    }
                }
                else if (ringIndicatorBase is EnemyIndicator)
                {
                    _playerAnimeManager.Hit();
                    SoundEffectManager.PlaySoundEffect(_avoidMissSound);
                }
            }

            yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat);
            if (ringIndicatorBase != _activeIndicator)
            {
                ringIndicatorBase.End();
                ringIndicatorBase.AddCount();
            }
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
            if (ctx.phase != InputActionPhase.Started || _indicatorTimer >= Time.time || _activeIndicator == null) return;
            _indicatorTimer = Time.time + (float)MusicEngineHelper.DurationOfBeat * _InvalidationTime;

            HandleIndicator<PlayerIndicator>(
                () => CheckGood(),
                ind =>
                {
                    if (_attackTutorialClearCount != _currentTargetClearCount)
                        VoiceManager.PlayVoice(_attackNormalVoiceName);
                    if (CheckPerfect())
                    {
                        ind.PlayPerfectEffect();
                        SoundEffectManager.PlaySoundEffect(_perfectAttackSound);
                        SoundEffectManager.PlaySoundEffect(_comboAttackSound);
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
                }
            );
        }

        /// <summary>
        /// スキル用の処理
        /// </summary>
        /// <param name="ctx"></param>
        private void OnSkill(InputAction.CallbackContext ctx)
        {

            if (ctx.phase != InputActionPhase.Started || _indicatorTimer >= Time.time || _activeIndicator == null) return;
            _indicatorTimer = Time.time + (float)MusicEngineHelper.DurationOfBeat * _InvalidationTime;

            HandleIndicator<SpecialIndicator>(
                () => CheckGood(),
                ind =>
                {
                    _skillEffect.Play();
                    SoundEffectManager.PlaySoundEffect(_skillSuccessSound);
                    if (_othersTutorialClearCount != _currentTargetClearCount)
                        VoiceManager.PlayVoice(_skillVoiceName);
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
            if (ctx.phase != InputActionPhase.Started || _indicatorTimer >= Time.time || _activeIndicator == null) return;
            _indicatorTimer = Time.time + (float)MusicEngineHelper.DurationOfBeat * _InvalidationTime;
            HandleIndicator<EnemyIndicator>(
                () => CheckGood(),
                ind =>
                {
                    if (_othersTutorialClearCount != _currentTargetClearCount)
                        VoiceManager.PlayVoice(_avoidVoiceName);
                    ind.OnPlayerAvoidSuccess(true);
                    SoundEffectManager.PlaySoundEffect(_dodgeSound);
                    _playerAnimeManager.Avoid();
                    _playerManager.FlowZoneSystem.SuccessResonance();
                },
                ind =>
                {
                    ind.PlayFailEffect();
                    _playerAnimeManager.Hit();
                    SoundEffectManager.PlaySoundEffect(_avoidMissSound);
                }
            );
        }

        /// <summary>
        /// チャージ攻撃用の処理
        /// </summary>
        /// <param name="ctx"></param>
        private void OnCharge(InputAction.CallbackContext ctx)
        {
            if (_activeIndicator == null) return;
            var chargeIndicator = _activeIndicator as ChargeIndicator;

            if (ctx.phase == InputActionPhase.Started)
            {
                //チャージ開始時の処理
                var normalizedTiming = (float)Music.UnitFromJust;
                if (Mathf.Abs(normalizedTiming - 0.5f) <= _goodRange / 2 && _currentIndicatorCount == 2)
                {
                    _isCharging = true;
                    _currentChargeBeat = 0;
                    chargeIndicator.OnPlayerChargeTutorial();
                    SoundEffectManager.PlaySoundEffect(_charging);
                    VoiceManager.PlayVoice(_chargeStartVoiceName);
                    _playerAnimeManager.ChargeShoot();
                    _chargeAttackWaiting = false;
                }
                else
                {
                    chargeIndicator.PlayFailEffect();
                    _enemyAnimeManager.ChargeAttack();
                    _enemyAnimeManager.KnockBack(false);
                    _playerAnimeManager.FatalHit();
                    _chargeAttackWaiting = false;
                    if (_activeIndicator != null)
                    {
                        StartCoroutine(EndIndicator(_activeIndicator));
                        _activeIndicator = null;
                    }
                    PlayVoice(_tutorialField);
                    SoundEffectManager.PlaySoundEffect(_chargeMissSound);
                    _currentMissCount++;
                    foreach (var item in _enemyBeamEffects)
                    {
                        item.Play(ChargeHash);
                    }
                    _currentMissCount++;
                    _isCharging = false;
                }
            }
            else if (ctx.phase == InputActionPhase.Canceled)
            {
                //チャージ完了時の処理
                if (!_isCharging) return;
                if (_currentChargeBeat == 2 || _currentChargeBeat == 1)
                {
                    var normalizedTiming = (float)Music.UnitFromJust;
                    if (Mathf.Abs(normalizedTiming - 0.5f) <= _goodRange / 2 && _currentIndicatorCount == 0)
                    {
                        _currentTargetClearCount++;
                        chargeIndicator.OnPlayerAttackSuccessTutorial();
                        SoundEffectManager.PlaySoundEffect(_chargeGunshot);
                        if (_othersTutorialClearCount != _currentTargetClearCount)
                            VoiceManager.PlayVoice(_chargeEndVoiceName);
                        _enemyAnimeManager.ChargeAttack();
                        _enemyAnimeManager.KnockBack(true);
                    }
                    else
                    {
                        chargeIndicator.PlayFailEffect();
                        _enemyAnimeManager.ChargeAttack();
                        _enemyAnimeManager.KnockBack(false);
                        _playerAnimeManager.FatalHit();
                        PlayVoice(_tutorialField);
                        SoundEffectManager.PlaySoundEffect(_chargeMissSound);
                        _currentMissCount++;
                        foreach (var item in _enemyBeamEffects)
                        {
                            item.Play(ChargeHash);
                        }
                        _currentMissCount++;
                        _isCharging = false;
                    }
                }
                else
                {
                    chargeIndicator.PlayFailEffect();
                    _enemyAnimeManager.ChargeAttack();
                    _enemyAnimeManager.KnockBack(false);
                    _playerAnimeManager.FatalHit();
                    PlayVoice(_tutorialField);
                    SoundEffectManager.PlaySoundEffect(_chargeMissSound);
                    _currentMissCount++;
                    foreach (var item in _enemyBeamEffects)
                    {
                        item.Play(ChargeHash);
                    }
                    _currentMissCount++;
                    _isCharging = false;
                }
                if (_activeIndicator != null)
                {
                    StartCoroutine(EndIndicator(_activeIndicator));
                    _activeIndicator = null;
                }
                _isCharging = false;
                _currentChargeBeat = 0;
            }
        }
        
        private void OnSkipTutorial(InputAction.CallbackContext context)
        {
            Debug.Log("SkipTutorial");
            if (context.phase == InputActionPhase.Started)
            {
                _isPushSkipButton = true;
                _skipThresholdTime = Time.time + _skipTime;
            }
            else
            {
                _isPushSkipButton = false;
            }

        }

        #endregion

        #region 操作説明

        private IEnumerator Explanation(ChartKindEnum chartKindEnum)
        {
            _operationTutorialPlaying = true;
            var ring = _chartRingManager.GenerateRing(chartKindEnum, Vector2.zero, 0);
            _activeIndicator = ring.GetComponent<RingIndicatorBase>();
            if (chartKindEnum == ChartKindEnum.Attack)
            {
                var ringObj = ring.GetComponent<PlayerIndicator>();
                ringObj.AddCount();
                SoundEffectManager.PlaySoundEffect(_ringNormalSound);
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                yield return ShowTutorialMessage(_localizeTextManager.GetTutorialOperationMessage(_attackIndicatorKey), _inputBuffer.Attack);

                ringObj.Resume();
                ringObj.PlayPerfectEffect();
                SoundEffectManager.PlaySoundEffect(_perfectAttackSound);
                SoundEffectManager.PlaySoundEffect(_comboAttackSound);
                VoiceManager.PlayVoice(_attackNormalVoiceName);
                _playerAnimeManager.Shoot();
                yield return new WaitForSeconds(0.5f);
                PlayVoice(_tutorialSuccess1);
                yield return new WaitForSeconds(3f);
            }
            else if (chartKindEnum == ChartKindEnum.Skill)
            {

                var ringObj = ring.GetComponent<SpecialIndicator>();
                ringObj.AddCount();
                SoundEffectManager.PlaySoundEffect(_ringSkillSound);
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                yield return ShowTutorialMessage(_localizeTextManager.GetTutorialOperationMessage(_skillIndicatorKey), _inputBuffer.Attack);

                _skillEffect.Play();
                _playerAnimeManager.Skill();
                SoundEffectManager.PlaySoundEffect(_skillSuccessSound);
                VoiceManager.PlayVoice(_skillVoiceName);
                ringObj.Resume();
                ringObj.PlaySuccessEffectPublic();
            }
            else if (chartKindEnum == ChartKindEnum.Normal)
            {
                var ringObj = ring.GetComponent<EnemyIndicator>();
                ringObj.AddCount();
                _operationTutorialEnemyAttacking = true;
                SoundEffectManager.PlaySoundEffect(_ringAvoidSound);
                _enemyAnimeManager.PreAttack();
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                _director.Pause();
                yield return ShowTutorialMessage(_localizeTextManager.GetTutorialOperationMessage(_enemyIndicatorKey), _inputBuffer.Avoid);
                _director.Resume();
                _playerManager.FlowZoneSystem.SuccessResonance();
                _playerAnimeManager.Avoid();
                _enemyAnimeManager.Attack();
                _operationTutorialEnemyAttacking = false;
                ringObj.Resume();
                ringObj.OnPlayerAvoidSuccess(true);
                SoundEffectManager.PlaySoundEffect(_dodgeSound);
                VoiceManager.PlayVoice(_avoidVoiceName);
                _director.Resume();
            }
            else if (chartKindEnum == ChartKindEnum.Charge)
            {
                var ringObj = ring.GetComponent<ChargeIndicator>();
                ringObj.AddCount();
                _operationTutorialEnemyCharging = true;
                SoundEffectManager.PlaySoundEffect(_chargeSound);
                _enemyAnimeManager.PreChargeAttack();
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2);

                ringObj.Pause();
                _inputBuffer.Interact.started += OnWaitInput;
                PlayVoice(_chargeStartVoice);
                yield return ShowTutorialMessage(_localizeTextManager.GetTutorialOperationMessage(_chargeIndicatorKey1), _inputBuffer.Interact);

                SoundEffectManager.PlaySoundEffect(_charging);
                VoiceManager.PlayVoice(_chargeStartVoiceName);
                _inputBuffer.Interact.started -= OnWaitInput;
                ringObj.Resume();
                ringObj.OnPlayerChargeTutorial();
                _playerAnimeManager.ChargeShoot();
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 1.5f);

                _playerAnimeManager.PauseAnimator();
                yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 0.5f);

                PlayVoice(_chargeCompleteVoice);
                SoundEffectManager.PlaySoundEffect(_chargeComplete);
                _tutorialUi.SetActive(true);
                _tutorialText.text = _localizeTextManager.GetTutorialOperationMessage(_chargeIndicatorKey2);
                _tutorialFocusImage.GetComponent<RectTransform>().anchoredPosition = _chargeEndFocusPosition;
                _tutorialFocusImage.enabled = true;
                ringObj.Pause();
                _inputBuffer.Interact.canceled += OnWaitInput;
                yield return new WaitUntil(() => _nextTutorial);

                _playerAnimeManager.ResumeAnimator();
                _enemyAnimeManager.ChargeAttack();
                _enemyAnimeManager.KnockBack(true);
                _operationTutorialEnemyCharging = false;
                _tutorialUi.SetActive(false);
                ringObj.OnPlayerAttackSuccessTutorial();
                _inputBuffer.Interact.canceled -= OnWaitInput;
                _nextTutorial = false;
                ringObj.Resume();
                SoundEffectManager.PlaySoundEffect(_chargeGunshot);
                VoiceManager.PlayVoice(_chargeEndVoiceName);
            }
            _tutorialFocusImage.enabled = false;
            yield return new WaitForSeconds((float)MusicEngineHelper.DurationOfBeat * 2.5f);

            _operationTutorialPlaying = false;
            ring.GetComponent<RingIndicatorBase>().End();
            _activeIndicator = null;
        }

        /// <summary>
        /// 入力を待機するための関数
        /// </summary>
        /// <param name="ctx"></param>
        private void OnWaitInput(InputAction.CallbackContext ctx)
        {
            _nextTutorial = true;
        }

        #endregion
    }
}
