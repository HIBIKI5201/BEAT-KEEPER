using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     プレイヤーのマネージャークラス
    /// </summary>
    public class PlayerManager : CharacterManagerB<PlayerData>, IDisposable
    {
        #region イベント
        public event Action OnShootComboAttack
        {
            add => _onShootComboAttack.Event += value;
            remove => _onShootComboAttack.Event -= value;
        }
        public event Action OnPerfectAttack;
        public event Action OnGoodAttack;
        public event Action OnMissAttack;

        public event Action OnChargeAttack;
        public event Action OnPerfectChargeAttack;
        public event Action OnGoodChargeAttack;
        public event Action OnMissChargeAttack;

        public event Action OnCharging
        {
            add => _onStartChargeAttack.Event += value;
            remove => _onStartChargeAttack.Event -= value;
        }
        public event Action OnPerfectCharging;
        public event Action OnGoodCharging;
        public event Action OnMissedCharging;

        public event Action OnFailedAvoid;
        public event Action OnSuccessAvoid
        {
            add => _onSuccessAvoid.Event += value;
            remove => _onSuccessAvoid.Event -= value;
        }
        public event Action OnPerfectAvoid
        {
            add => _onPerfectAvoid.Event += value;
            remove => _onPerfectAvoid.Event -= value;
        }
        public event Action OnGoodAvoid;

        public event Action OnSkill
        {
            add => _onSkill.Event += value;
            remove => _onSkill.Event -= value;
        }
        public event Action OnPerfectSkill;
        public event Action OnGoodSkill;
        public event Action OnMissedSkill;

        public event Action OnFinisher;
        #endregion

        #region プロパティ
        public ComboSystem ComboSystem => _comboSystem;
        public SpecialSystem SpecialSystem => _specialSystem;
        public FlowZoneSystem FlowZoneSystem => _flowZoneSystem;
        #endregion

        #region  公開メソッド

        #region 入力の購買

        /// <summary>
        ///     入力を購買する
        /// </summary>
        public void InputRegister()
        {
            if (_inputBuffer)
            {
                _inputBuffer.Interact.started += OnChargeAttackInput;
                _inputBuffer.Interact.canceled += OnChargeAttackInput;
                _inputBuffer.Attack.started += OnAttackInput;
                _inputBuffer.Avoid.started += OnAvoid;

                SymphonyDebugLogger.DirectLog("player input registered");
            }
            else
            {
                Debug.LogWarning("Input buffer is null");
            }
        }

        /// <summary>
        ///     入力の購買を終わる
        /// </summary>
        public void InputUnregister()
        {
            if (_inputBuffer)
            {
                _inputBuffer.Interact.started -= OnChargeAttackInput;
                _inputBuffer.Interact.canceled -= OnChargeAttackInput;
                _inputBuffer.Attack.started -= OnAttackInput;
                _inputBuffer.Avoid.started -= OnAvoid;

                SymphonyDebugLogger.DirectLog("player input unregistered");
            }
            else
            {
                Debug.LogWarning("Input buffer is null");
            }
        }
        #endregion

        /// <summary>
        ///     現在スタン中かを判定する
        /// </summary>
        /// <returns></returns>
        public bool IsStunning() => IsStunning(Time.time);

        /// <summary>
        ///     指定時間がスタン中かを判定する
        /// </summary>
        /// <param name="timing">判定する時間</param>
        /// <returns></returns>
        public bool IsStunning(float timing)
        {
            return _stunEndTiming > timing;
        }

        /// <summary>
        ///     フィニッシャーが可能かどうかを判定する
        /// </summary>
        /// <returns></returns>
        public bool IsFinisherable()
        {
            if (_target == null) return false; //ターゲットがいなければフィニッシャーはできない
            return _target.IsFinisherable;
        }
        
        /// <summary>
        /// タイムラインのイベントを元にモデルを表示する
        /// NOTE: ブレイクムービーから明けたときにはフェーズ変更時ではない任意のタイミングから呼び出せるようにしたい
        /// </summary>
        public void ModelActive()
        {
            _animeManager.SetAnimatorSpeed((float)(SymphonyMusicEngine.CurrentBPM / 120d));
            _modelParent.SetActive(true);
        }

        #endregion

        #region  インターフェースメソッド

        /// <summary>
        ///     攻撃を受けた際の処理
        /// </summary>
        /// <param name="data"></param>
        public override async void HitAttack(AttackData data)
        {
            //無敵時間なら受けない
            if (_lastAvoidSuccessTiming
                + MusicEngineHelper.DurationOfBeat
                 * (_data.AvoidInvincibilityTime + 0.5f) //敵の攻撃タイミングであるNearBeat分追加する
                 > Time.time)
            {
                Debug.Log("During Avoid Invincibility Time");
                return;
            }

            base.HitAttack(data);
            _onHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));
            SoundEffectManager.PlaySoundEffect(data.IsNockback ? _chargeHitSound : _hitSound);
            VoiceManager.PlayVoice(data.IsNockback ? _chargeDamagedVoice : _avoidDamagedVoice);

            float stunTime = data.IsNockback ? _data.ChargeHitStunTime : _data.HitStunTime; //チャージかに応じて変化
            _stunEndTiming = Time.time + stunTime * (float)MusicEngineHelper.DurationOfBeat; //スタン時間を更新する

            _comboSystem.ComboReset();
            if (data.IsNockback)
            {
                _animeManager.FatalHit();
            }
            else
            {
                _animeManager.Hit();
            }

            if (_stunTokenSource != null)
            {
                _stunTokenSource.Cancel(); //前のスタンをキャンセル
            }
            _stunTokenSource = new CancellationTokenSource();

            await Awaitable.WaitForSecondsAsync(stunTime, _stunTokenSource.Token);

            VoiceManager.PlayVoice(_stunEndVoice);
            _stunTokenSource = null;
        }

        #endregion

        #region プライベートフィールド
        [SerializeField]
        private UnityEventWrapper _onShootComboAttack = new();
        [SerializeField]
        private UnityEventWrapper _onStartChargeAttack = new();
        [SerializeField]
        private UnityEventWrapper _onSkill = new();
        [SerializeField]
        private UnityEventWrapper _onSuccessAvoid = new();
        [SerializeField]
        private UnityEventWrapper _onPerfectAvoid = new();
        [SerializeField]
        private UnityEventWrapper _onFlowZone = new();

        [SerializeField] private BattleBuffTimelineData _battleBuffData;
        [SerializeField] private GameObject _comboShootPerticle;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private RingIndicatorData _ringIndicatorData;
        [SerializeField] private GameObject _modelParent;

        #region サウンドクリップ
        [Header("SE")]
        [SerializeField, Tooltip("汎用的な発砲音（通常攻撃の1段目の発砲音）")]
        private string _comboAttackSound;

        [SerializeField, Tooltip("スキル")]
        private string _skillSound;

        [SerializeField, Tooltip("チャージ中")]
        private string _chargeAttackStartSound;

        [SerializeField, Tooltip("チャージ完了")]
        private string _chargeAttackEndSound;

        [SerializeField, Tooltip("強攻撃")]
        private string _chargeAttackSound;

        [SerializeField, Tooltip("回避")]
        private string _avoidSound;

        [SerializeField, Tooltip("フィニッシャー")]
        private string _finisherSound;

        [SerializeField, Tooltip("通常攻撃被弾")]
        private string _hitSound;
        
        [SerializeField, Tooltip("チャージ攻撃被弾")]
        private string _chargeHitSound;

        [SerializeField, Tooltip("Perfect判定時（攻撃・回避兼用）")]
        private string _perfectSound;

        [SerializeField, Tooltip("フローゾーンゲージが1になった時の音")]
        private string _flowZone1;

        [SerializeField, Tooltip("フローゾーンゲージが2になった時の音")]
        private string _flowZone2;

        [SerializeField, Tooltip("フローゾーンゲージが3になった時の音")]
        private string _flowZone3;

        [SerializeField, Tooltip("フローゾーンゲージが4になった時の音")]
        private string _flowZone4;

        [SerializeField, Tooltip("フローゾーン突入")]
        private string _flowZoneStartSound;

        [SerializeField, Tooltip("フローゾーン終了")]
        private string _flowZoneEndSound;
        #endregion

        #region ボイス
        [Header("Voice")]
        [SerializeField, Tooltip("コンボ攻撃のボイス1")]
        private string _comboShootVoice1;
        [SerializeField, Tooltip("コンボ攻撃のボイス2")]
        private string _comboShootVoice2;
        [SerializeField, Tooltip("コンボ攻撃のボイス3")]
        private string _comboShootVoice3;
        [SerializeField, Tooltip("コンボ攻撃のコンプリート")]
        private string _comboShootCompleteVoice;

        [SerializeField, Tooltip("チャージ攻撃開始のボイス")]
        private string _chargeStartShootVoice;
        [SerializeField, Tooltip("チャージ攻撃終了のボイス")]
        private string _chargeEndShootVoice;
        [SerializeField, Tooltip("チャージ攻撃失敗のボイス")]
        private string _chargeDamagedVoice;

        [SerializeField, Tooltip("回避成功時のボイス")]
        private string _avoidSuccessVoice;
        [SerializeField, Tooltip("回避失敗時のボイス")]
        private string _avoidDamagedVoice;

        [SerializeField, Tooltip("スキルのボイス")]
        private string _skillVoice;

        [SerializeField, Tooltip("スタン解除時のボイス")]
        private string _stunEndVoice;
        #endregion

        private InputBuffer _inputBuffer;
        private BGMManager _bgmManager;
        private ScoreManager _scoreManager;
        private PhaseManager _phaseManager;

        private IEnemy _target;
        private bool _isBattle;
        [Tooltip("スタンが終了するタイミング")] private float _stunEndTiming;
        private CancellationTokenSource _stunTokenSource;
        private ReactiveProperty<int> _comboAttackCounter = new();
        private CancellationTokenSource _chargeAttackChargingTokenSource;
        [Tooltip("最後の回避成功のタイミング")] private float _lastAvoidSuccessTiming;
        [Tooltip("パーフェクト攻撃の予約")] private bool _willPerfectAttack;
        [Tooltip("連打防止のための、拍内で一度しか押せないフラグ")] private bool _isThisBeatInputed;

        private PlayerAnimeManager _animeManager;
        private ComboSystem _comboSystem;
        private SpecialSystem _specialSystem;
        private FlowZoneSystem _flowZoneSystem;
        private SkillSystem _skillSystem;

        private bool _isMissed = false;
        #endregion

        #region 開発用の機能

        [Header("開発用")]
        [Obsolete("モック用"), SerializeField, Tooltip("攻撃のダメージ倍率"), Min(0.1f)]
        private float _damageScale = 1;

        [Obsolete("モック用"), SerializeField] private ParticleSystem _particleSystem;

        #endregion

        #region ライフサイクル

        protected override async void Awake()
        {
            await SystemInit(); //システムの初期化

            OnShootComboAttack += _particleSystem.Play;

            OnPerfectAvoid += () => Debug.Log("perfect avoid");
            OnGoodAvoid += () => Debug.Log("good avoid");
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _scoreManager = ServiceLocator.GetInstance<ScoreManager>();
            _bgmManager = ServiceLocator.GetInstance<BGMManager>();

            if (_bgmManager)
            {
                _bgmManager.OnJustChangedBeat += OnJustBeat;
                _bgmManager.OnNearChangedBeat += OnNearBeat;
            }
            else
            {
                Debug.LogWarning("Music engine is null");
            }

            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _phaseManager = phaseManager;
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnPhaseChanged)
                    .AddTo(destroyCancellationToken);
                
                // コンボシステムにフェーズマネージャーを渡す
                _comboSystem.SetupPhaseManager(phaseManager);
            }
            else
            {
                Debug.LogWarning("Phase manager is null");
            }
        }

        private void Update()
        {
            _comboSystem.Update();
        }

        public void Dispose()
        {
            InputUnregister();
            _flowZoneSystem?.Dispose();
        }

        private void OnDestroy()
        {
            if (_bgmManager != null)
            {
                _bgmManager.OnJustChangedBeat -= OnJustBeat;
                _bgmManager.OnNearChangedBeat -= OnNearBeat;
            }
            Dispose();
        }

        #endregion

        #region  オブザーバー系メソッド

        /// <summary>
        ///     フェーズが変わった際の処理
        /// </summary>
        /// <param name="phase"></param>
        private void OnPhaseChanged(PhaseEnum phase)
        {
            _isBattle = phase == PhaseEnum.Battle;

            switch (phase)
            {
                case PhaseEnum.Battle:
                    InputRegister();
                    var stage = ServiceLocator.GetInstance<BattleSceneManager>();
                    _target = stage.EnemyAdmin.GetActiveEnemy();
                    break;
                
                case PhaseEnum.Tutorial:
                    _animeManager.SetAnimatorSpeed((float)(SymphonyMusicEngine.CurrentBPM / 120d));
                    _modelParent.SetActive(true);
                    break;

                case PhaseEnum.Movie:
                    _flowZoneSystem.ResetFlowZone();
                    _flowZoneSystem.ResetResonanceCount();
                    _modelParent.SetActive(false);
                    break;
            }
        }

        /// <summary>
        ///     ノーツの処理を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnAttackInput(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (!_data) return;
            if (_isThisBeatInputed) return; //連打防止
            if (_target == null) return;

            ChartData.ChartDataElement[] chart = _target.EnemyData
                .GetChartDataByFlowZone(_flowZoneSystem.IsFlowZone.CurrentValue).Chart;
            int timing = MusicEngineHelper.GetBeatNearerSinceStart() % chart.Length;
            ChartKindEnum kind = chart[timing].AttackKind;

            if (IsAnotherPhaseByChartKind(kind)) return; //別のフェーズなら何もしない

            if (kind == ChartKindEnum.Attack)
            {
                _isThisBeatInputed = true;
                AttackFlow();
            }
            else if (kind == ChartKindEnum.Skill)
            {
                _isThisBeatInputed = true;
                if (IsFinisherable()) //フィニッシャーが可能ならフィニッシャーする
                {
                    Debug.Log("<color=red>Finisher invoke</color>");
                    FinisherFlow();
                    return;
                }

                Debug.Log("<color=red>skill invoke</color>");
                SKillFlow();
            }
        }

        /// <summary>
        ///     溜め攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnChargeAttackInput(InputAction.CallbackContext context)
        {
            const ChartKindEnum CHARGE_ATTACK_ENUM = ChartKindEnum.Charge;

            if (!_isBattle) return;
            if (_target == null) return;

            //タイミングや時間を取得
            ChartData chart = _target.EnemyData
                .GetChartDataByFlowZone(_flowZoneSystem.IsFlowZone.CurrentValue);
            int timing = MusicEngineHelper.GetBeatNearerSinceStart();


            switch (context.phase)
            {
                case InputActionPhase.Started: //チャージ開始
                    int chargeAttackRange = Mathf.RoundToInt(_data.ChargeAttackTime); //チャージ攻撃可能な拍数
                    if (chart[chargeAttackRange + timing].AttackKind != CHARGE_ATTACK_ENUM) return;

                    ChargeAttackCharging();
                    break;

                case InputActionPhase.Canceled: //発動
                    // チャージ中でなければ何もしない
                    if (_chargeAttackChargingTokenSource == null) break;

                    if (IsAnotherPhaseByChartKind(CHARGE_ATTACK_ENUM))
                    {
                        SymphonyDebugLogger.AddText(
                            $"[{nameof(PlayerManager)}] {CHARGE_ATTACK_ENUM} is in another phase");
                        SymphonyDebugLogger.TextLog();

                        break;
                    }

                    _chargeAttackChargingTokenSource?.Cancel();
                    _chargeAttackChargingTokenSource = null;

                    //現在がチャージ攻撃のタイミングでなければ失敗
                    if (chart[timing].AttackKind != ChartKindEnum.Charge)
                    {
                        MissedChargeAttack();
                        return;
                    }

                    // 成功判定へ
                    ChargeAttackActivation();
                    break;
            }
        }

        /// <summary>
        ///     回避
        /// </summary>
        /// <param name="context"></param>
        private void OnAvoid(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_isThisBeatInputed) return; //連打防止
            if (_target == null) return;

            SymphonyDebugLogger.AddText(
                $"[{nameof(PlayerManager)}]" +
                $"{_data.Name} is avoiding");

            ChartData chartData = _target.EnemyData
                .GetChartDataByFlowZone(_flowZoneSystem.IsFlowZone.CurrentValue);
            int timing = MusicEngineHelper.GetBeatNearerSinceStart() % chartData.Chart.Length;
            ChartKindEnum enemyAttackKind = chartData[timing].AttackKind;

            if (IsAnotherPhaseByChartKind(enemyAttackKind))
            {
                SymphonyDebugLogger.AddText(
                    $"[{nameof(PlayerManager)}] {enemyAttackKind} is in another phase");
                SymphonyDebugLogger.TextLog();
                return;
            }

            //Charge攻撃は回避できない
            if ((enemyAttackKind & ChartKindEnum.Charge) != 0)
            {
                SymphonyDebugLogger.AddText($"Enemy's attack of {enemyAttackKind}(timing:{timing}) can't be avoided");
                SymphonyDebugLogger.TextLog();
                return;
            }

            //敵が攻撃しないならミス
            if (!chartData.IsEnemyAttack(timing))
            {
                SymphonyDebugLogger.AddText($"Enemy not attack at timing {timing}");
                SymphonyDebugLogger.TextLog();
                return;
            }

            bool isPerfect = MusicEngineHelper
                .IsTimingWithinAcceptableRange(_data.PerfectAvoidRnage);
            bool isGood = MusicEngineHelper
                .IsTimingWithinAcceptableRange(_data.GoodAvoidRange);

            if (!isGood && !isPerfect)
            {
                //失敗時の処理
                MissedAvoid();
                
                SymphonyDebugLogger.AddText("avoid result : failed");
                SymphonyDebugLogger.TextLog();
                return;
            }

            SymphonyDebugLogger.AddText("avoid result : success");

            _flowZoneSystem.SuccessResonance();
            _comboSystem.Attack();

            if (isPerfect)
            {
                //スコアにパーフェクト倍率を掛ける
                _scoreManager.AddScore(
                    Mathf.FloorToInt(_data.AvoidScore * _data.AvoidPerfectScoreScale));
                _onPerfectAvoid?.Invoke();
            }
            else if (isGood)
            {
                _scoreManager.AddScore(_data.AvoidScore);
                OnGoodAvoid?.Invoke();
            }

            AvoidFlow();
            SymphonyDebugLogger.TextLog();
        }

        /// <summary>
        ///     表拍ビートの処理
        /// </summary>
        private void OnJustBeat()
        {
            if (_willPerfectAttack) //もしパーフェクト攻撃が予約されていれば実行
            {
                SymphonyDebugLogger.DirectLog("Quantize perfect attack executed");

                PerfectAttack();
            }
        }

        /// <summary>
        ///     裏拍ビートの処理
        /// </summary>
        private void OnNearBeat()
        {
            _willPerfectAttack = false;

            if (_isThisBeatInputed) //拍内での連打防止フラグをリセット
            {
                _isThisBeatInputed = false;
            }

            if (_isBattle)
            {
                MissedChart();
                Debug.Log($"on near beat {Time.time} {MusicEngineHelper.GetBeatSinceStart()}");
            }

            if (_isMissed)
            {
                _isMissed = false;
            }

            if (IsResetComboAttackCounter())
            {
                _comboAttackCounter.Value = 0;
            }
        }

        /// <summary>
        ///     フローゾーンが開始した時のイベント
        /// </summary>
        private void StartFlowZone()
        {
            SoundEffectManager.PlaySoundEffect(_flowZoneStartSound);
            _onFlowZone?.Invoke();
        }

        /// <summary>
        ///     フローゾーンが終了した時のイベント
        /// </summary>
        private void EndFlowZone() => SoundEffectManager.PlaySoundEffect(_flowZoneEndSound);


        #endregion

        public PlayerAnimeManager GetPlayerAnimeManager()
        {
            return _animeManager;
        }

        /// <summary>
        ///     システムの初期化処理
        /// </summary>
        private async Task SystemInit()
        {
            //システムの初期化
            _comboSystem = new ComboSystem(_data);
            _specialSystem = new SpecialSystem();
            _flowZoneSystem =
                new FlowZoneSystem(
                    await ServiceLocator.GetInstanceAsync<BGMManager>(),
                    _data);
            _skillSystem = new SkillSystem(_data);

            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                _animeManager = new(animator);
            }
            else
            {
                Debug.LogWarning("Character animator component not found");
            }

            if (_flowZoneSystem != null)
            {
                _flowZoneSystem.OnStartFlowZone += StartFlowZone;
                _flowZoneSystem.OnEndFlowZone += EndFlowZone;
                _flowZoneSystem.ResonanceCount.Subscribe(n =>
                {
                    string queName = n switch
                    {
                        1 => _flowZone1,
                        2 => _flowZone2,
                        3 => _flowZone3,
                        4 => _flowZone4,
                        _ => string.Empty
                    };
                    if (string.IsNullOrEmpty(queName)) return;
                    SoundEffectManager.PlaySoundEffect(queName);
                }).AddTo(destroyCancellationToken);
            }

            //コンボとアニメーターを同期
            if (_comboSystem != null && _animeManager != null)
            {
                _comboAttackCounter
                    .Subscribe(n => _animeManager.Combo(n))
                    .AddTo(destroyCancellationToken);
            }
        }

        /// <summary>
        ///     攻撃入力の一連のフロー
        /// </summary>
        private void AttackFlow()
        {
            if (_target == null) return;
            if (_isMissed) return;

            SymphonyDebugLogger.AddText($"{_data.Name} do attack");

            //攻撃が成功したか
            bool isPerfectHit = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.ComboPerfectRange);
            bool isGoodHit = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.ComboGoodRange);

            //評価のログ
            SymphonyDebugLogger.AddText($"{(isPerfectHit ? "perfect" : (isGoodHit ? "good" : "miss"))}attack");

            if (isGoodHit) //最低でもGood以上ならヒット
            {
                if (isPerfectHit)
                {
                    if (0 < SymphonyMusicEngine.UnitFromJust - 0.5f) //ビート前なら次のJustまで予約
                    {
                        _willPerfectAttack = true;
                    }
                    else //ビート後なら即座に実行
                    {
                        PerfectAttack();
                    }
                }
                else //Goodなら即座に実行する
                {
                    GoodAttack();
                }
            }
            else
            {
                MissAttack();
            }

            SymphonyDebugLogger.TextLog();
        }

        /// <summary>
        ///     スキルの一連のフロー
        /// </summary>
        private void SKillFlow()
        {
            if (_isMissed) return;

            bool isPerfect = MusicEngineHelper
                .IsTimingWithinAcceptableRange(_data.PerfectSkillRange);
            bool isGood = MusicEngineHelper
                .IsTimingWithinAcceptableRange(_data.GoodSkillRange);

            if (!isGood && !isPerfect)
            {
                Debug.Log("skill is failed");
                return;
            }

            SymphonyDebugLogger.AddText($"{_data.Name} do skill");

            SoundEffectManager.PlaySoundEffect(_skillSound);

            if (isPerfect)
            {
                SuccessSkill();
                OnPerfectSkill?.Invoke();
            }
            else if (isGood)
            {
                SuccessSkill();
                OnGoodSkill?.Invoke();
            }
            else { MissSkill(); }

            SymphonyDebugLogger.TextLog();
        }

        private void SuccessSkill()
        {
            _onSkill?.Invoke();
            _animeManager.Skill();
            _skillSystem.StartSkill();
            VoiceManager.PlayVoice(_skillVoice);
        }

        /// <summary>
        ///     フィニッシャーの一連のフロー
        /// </summary>
        private void FinisherFlow()
        {
            SoundEffectManager.PlaySoundEffect(_finisherSound);

            OnFinisher?.Invoke();
            InputUnregister();
        }

        /// <summary>
        ///     パーフェクト攻撃を行う
        /// </summary>
        private void PerfectAttack()
        {
            OnPerfectAttack?.Invoke();
            _specialSystem.AddSpecialEnergy(0.05f);
            SoundEffectManager.PlaySoundEffect(_perfectSound);
            BothComboAttack();
            AttackEnemy(_data.ComboAttackPower, _data.PerfectCriticalDamage);
        }

        /// <summary>
        ///     グッド攻撃を行う
        /// </summary>
        private void GoodAttack()
        {
            OnGoodAttack?.Invoke();
            BothComboAttack();
            AttackEnemy(_data.ComboAttackPower);
        }

        /// <summary>
        ///    ミスした際の処理
        /// </summary>
        private void MissAttack()
        {
            Debug.Log("miss attack");
            
            OnMissAttack?.Invoke();
            _comboSystem.ComboReset();
            _comboAttackCounter.Value = 0; //コンボカウンターをリセット
            _isMissed = true;
        }

        /// <summary>
        ///    成功時の共通処理
        /// </summary>
        private void BothComboAttack()
        {
            _onShootComboAttack?.Invoke();
            if (_comboShootPerticle != null && _muzzle != null)
            { Instantiate(_comboShootPerticle, _muzzle.position, _muzzle.rotation); }
            _comboSystem.Attack(); //コンボを更新
            _animeManager.Shoot();
            _target.NormalAttackRandomHit();
            SoundEffectManager.PlaySoundEffect(_comboAttackSound);
            VoiceManager.PlayVoice(
                (_comboAttackCounter.Value % 3) switch
                {
                    0 => _comboShootVoice1,
                    1 => _comboShootVoice2,
                    2 => _comboShootVoice3,
                    _ => string.Empty
                });

            if (2 <= _comboAttackCounter.Value)
            {
                PlayComboCompleteVoice();
            }

            _comboAttackCounter.Value = ++_comboAttackCounter.Value % 3;
        }

        private async void PlayComboCompleteVoice()
        {
            await Awaitable.WaitForSecondsAsync((float)MusicEngineHelper.DurationOfBeat, destroyCancellationToken);

            VoiceManager.PlayVoice(_comboShootCompleteVoice);
        }

        private void MissSkill()
        {
            Debug.Log("miss skill");
            _comboSystem?.ComboReset();
            OnMissedSkill?.Invoke();
            _isMissed = true;
        }


        /// <summary>
        ///     チャージ攻撃の溜め
        /// </summary>
        private async void ChargeAttackCharging()
        {
            if (_isMissed) return;

            bool isPerfecet = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.ChargeStartPerfectRange);
            bool isGood = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.ChargeStartGoodRange);

            //チャージ攻撃失敗
            if (!isPerfecet && !isGood)
            {
                MissedCharging();

                SymphonyDebugLogger.AddText("charge attack start is failed");
                SymphonyDebugLogger.TextLog();
                return;
            }

            Debug.Log($"{_data.Name} start charge attack");
            _onStartChargeAttack?.Invoke();
            if (isPerfecet) { OnPerfectCharging?.Invoke(); }
            else if (isGood) { OnGoodCharging?.Invoke(); }

            _chargeAttackChargingTokenSource = new();
            _scoreManager.AddScore(_data.ChargeStartScore);
            _comboSystem.Attack();

            SoundEffectManager.PlaySoundEffect(_chargeAttackStartSound);
            VoiceManager.PlayVoice(_chargeStartShootVoice);
            _animeManager.ChargeShoot();

            try
            {
                //チャージが完了するまで待機
                await Awaitable.WaitForSecondsAsync(
                    _data.ChargeAttackTime * (float)MusicEngineHelper.DurationOfBeat,
                    _chargeAttackChargingTokenSource.Token);
            }
            catch (OperationCanceledException ex) { return; }
            finally
            {
                _chargeAttackChargingTokenSource = null; //チャージ中のトークンを解放
            }

            SoundEffectManager.PlaySoundEffect(_chargeAttackEndSound);
        }

        private void MissedCharging()
        {
            Debug.Log("missed charging");

            _comboSystem.ComboReset();
            OnMissedCharging?.Invoke();
            _isMissed = true;
        }

        /// <summary>
        ///     チャージ攻撃を発動する
        /// </summary>
        private void ChargeAttackActivation()
        {
            if (_isMissed) return;

            bool isPerfect = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.ChargeEndPerfectRange);
            bool isGood = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.ChargeEndGoodRange);

            //チャージ攻撃失敗
            if (!isPerfect && !isGood)
            {
                MissedChargeAttack();
                return;
            }

            Debug.Log($"{_data.Name} is full charge attacking");
            OnChargeAttack?.Invoke();

            if (isPerfect) { OnPerfectChargeAttack?.Invoke(); }
            else if (isGood) { OnGoodChargeAttack?.Invoke(); }

            SoundEffectManager.PlaySoundEffect(_chargeAttackSound);
            VoiceManager.PlayVoice(_chargeEndShootVoice);
            AttackEnemy(_data.ChargeAttackPower, nockback: true);
            _scoreManager.AddScore(_data.ChargeEndScore);
            _comboSystem.Attack();
        }

        private void MissedChargeAttack()
        {
            Debug.Log("miss charge attack");
            OnMissChargeAttack?.Invoke();
            _comboSystem.ComboReset();
            _isMissed = true;
        }

        /// <summary>
        ///     敵に攻撃を行う
        /// </summary>
        /// <param name="scoreScale"></param>
        private void AttackEnemy(float power, float scoreScale = 1, bool nockback = false)
        {
            if (_battleBuffData) //タイムラインバフ
            {
                var buffData = _battleBuffData.Data;

                int timing = MusicEngineHelper.GetBeatSinceStart();

                for (int i = buffData.Length - 1; i >= 0; i--)
                {
                    if (buffData[i].Timing < timing)
                    {
                        power *= buffData[i].Value;
                        SymphonyDebugLogger.AddText($"{buffData[i].Value} buff of {buffData[i].Timing} active");
                        break;
                    }
                }
            }

            if (_skillSystem.IsActive) //スキルがアクティブ中ならバフを適用
            {
                power *= _data.SkillStrangth;
            }

            power *= _damageScale;

            _target.HitAttack(new(power, nockback));

            // スコア計算
            float score = power * scoreScale * _data.ComboScoreScale
                [_comboAttackCounter.Value % _data.ComboScoreScale.Length];
            _scoreManager?.AddScore(Mathf.FloorToInt(score)); // スコアを加算。小数点以下は切り捨てる
        }

        /// <summary>
        ///     回避の一連のフロー
        /// </summary>
        private void AvoidFlow()
        {
            _isThisBeatInputed = true; //連打防止フラグを立てる

            _onSuccessAvoid?.Invoke();
            SoundEffectManager.PlaySoundEffect(_avoidSound);
            VoiceManager.PlayVoice(_avoidSuccessVoice);

            _animeManager.Avoid();
            _lastAvoidSuccessTiming = Time.time;
        }

        private void MissedAvoid()
        {
            OnFailedAvoid?.Invoke();
            _isMissed = true;
        }

        /// <summary>
        ///     指定されたノーツ種が開始されたのが別フェーズかを判定する
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        private bool IsAnotherPhaseByChartKind(ChartKindEnum kind)
        {
            if (kind == ChartKindEnum.None) return false;
            if (_ringIndicatorData == null) return false;

            if (!_ringIndicatorData.TryGetRingData(kind, out RingData data)) return false;
            int effectLength = data.EffectLength;

            float startTiming = Time.time - 
                (float)MusicEngineHelper.DurationOfBeat 
                * (effectLength - 1); // 1オリジンのため
            return _phaseManager.IsAnotherPhaseByTiming(startTiming);
        }

        /// <summary>
        ///     見逃したチャートの処理
        /// </summary>
        private async void MissedChart()
        {
            if (_target == null) return;

            // 次のノーツを取得する
            int timing = MusicEngineHelper.GetBeatSinceStart() + 1; 
            ChartData chart = _target.EnemyData
                .GetChartDataByFlowZone(_flowZoneSystem.IsFlowZone.CurrentValue);
            ChartKindEnum kind = chart[timing].AttackKind;

            bool isCharging = false;
            const int CHARGE_START_BEAT = 3;
            if (_ringIndicatorData
                .TryGetRingData(ChartKindEnum.Charge, out RingData chargeRingData))
            {
                int chargeStartTiming = chargeRingData.EffectLength - CHARGE_START_BEAT;
                if (chart[timing + chargeStartTiming].AttackKind == ChartKindEnum.Charge)
                {
                    isCharging = true;
                }
            }

            if (!isCharging && kind == ChartKindEnum.None) return;
            if (IsAnotherPhaseByChartKind(kind)) return;

            _ringIndicatorData.TryGetRingData(kind, out RingData data);
            if (!isCharging && data == null) return;

            float duration = (float)MusicEngineHelper.DurationOfBeat;

            bool missedFlag = false;
            Action action = null;

            if (kind == ChartKindEnum.Attack)
            {
                action = () =>
                {
                    missedFlag = true;
                    OnShootComboAttack -= action;
                    OnMissAttack -= action;
                };
                OnShootComboAttack += action;
                OnMissAttack += action;
            }
            else if (kind == ChartKindEnum.Charge)
            {
                action = () =>
                {
                    missedFlag = true;
                    OnChargeAttack -= action;
                    OnMissChargeAttack -= action;
                };
                OnChargeAttack += action;
                OnMissChargeAttack += action;
            }
            else if (isCharging)
            {
                action = () =>
                {
                    missedFlag = true;
                    OnCharging -= action;
                    OnMissedCharging -= action;
                };
                OnCharging += action;
                OnMissedCharging += action;
            }
            else if (kind == ChartKindEnum.Normal)
            {
                action = () =>
                {
                    missedFlag = true;
                    OnSuccessAvoid -= action;
                    OnFailedAvoid -= action;
                };
                OnSuccessAvoid += action;
                OnFailedAvoid += action;

            }
            else if (kind == ChartKindEnum.Skill)
            {
                action = () =>
                {
                    missedFlag = true;
                    OnSkill -= action;
                    OnMissedSkill -= action;
                    OnFinisher -= action;
                };
                OnSkill += action;
                OnMissedSkill += action;
                OnFinisher += action;
            }

            float timer = Time.time + duration; //拍が終わるタイミング
            Debug.Log($"miss timer {timer} {timing}");
            try
            {
                await SymphonyTask.WaitUntil(() => timer < Time.time || missedFlag,
                    destroyCancellationToken);
            }
            finally
            {
                // イベントの解放処理
                if (kind == ChartKindEnum.Attack)
                {
                    OnShootComboAttack -= action;
                    OnMissAttack -= action;
                }
                else if (kind == ChartKindEnum.Charge)
                {
                    OnChargeAttack -= action;
                    OnMissChargeAttack -= action;
                }
                else if (isCharging)
                {
                    OnCharging -= action;
                    OnMissedCharging -= action;
                }
                else if (kind == ChartKindEnum.Normal)
                {
                    OnSuccessAvoid -= action;
                    OnFailedAvoid -= action;
                }
                else if (kind == ChartKindEnum.Skill)
                {
                    OnSkill -= action;
                    OnMissedSkill -= action;
                    OnFinisher -= action;
                }
            }

            if (missedFlag) return; // 成功していたら何もしない。
            if (_isMissed) return; // 既にミス処理が行われていたら実行されない。

            switch (kind)
            {
                case ChartKindEnum.Attack: //コンボ攻撃の時。
                    MissAttack();
                    break;
                case ChartKindEnum.Charge: // チャージ発射の時。
                    MissedChargeAttack();
                    break;
                case ChartKindEnum.Skill: // スキルの時。
                    MissSkill();
                    break;
                case ChartKindEnum.Normal: // 回避の時。
                    MissedAvoid();
                    break;
            }

            if (isCharging) // チャージ開始の時。
            {
                MissedCharging();
            }
        }

        private bool IsResetComboAttackCounter()
        {
            if (_target == null) return false;

            int timing = MusicEngineHelper.GetBeatNearerSinceStart();
            ChartData chart = _target.EnemyData
                .GetChartDataByFlowZone(_flowZoneSystem.IsFlowZone.CurrentValue);
            ChartKindEnum kind = chart[timing].AttackKind;

            if (kind == ChartKindEnum.None) return false;

            return kind != ChartKindEnum.Attack;
        }

# if UNITY_EDITOR
        /// <summary>
        ///     スペシャルエネルギーを追加するためのデバッグ機能
        /// </summary>
        [ContextMenu(nameof(AddSpecialEnergy))]
        private void AddSpecialEnergy() => _specialSystem.AddSpecialEnergy(1);
#endif
    }
}