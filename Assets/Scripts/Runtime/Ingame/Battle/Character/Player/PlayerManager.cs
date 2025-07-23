using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
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
        public event Action OnShootComboAttack;
        public event Action OnPerfectAttack;
        public event Action OnGoodAttack;

        public event Action OnStartChargeAttack;
        public event Action OnShootChargeAttack;
        public event Action OnFullChargeAttack;
        public event Action OnNonFullChargeAttack;

        public event Action OnFailedAvoid;
        public event Action OnSuccessAvoid;
        
        public event Action OnSkill;

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
                _inputBuffer.Move.performed += OnMoveInput;
                _inputBuffer.Move.canceled += OnMoveInput;
                _inputBuffer.Interact.started += OnChargeAttackInput;
                _inputBuffer.Interact.canceled += OnChargeAttackInput;
                _inputBuffer.Attack.started += OnAttackInput;
                _inputBuffer.Special.started += OnSpecialInput;
                _inputBuffer.Finishier.started += OnFinisherInput;
                _inputBuffer.Avoid.started += OnAvoid;

                SymphonyDebugLog.DirectLog("player input registered");
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
                _inputBuffer.Move.performed -= OnMoveInput;
                _inputBuffer.Move.canceled -= OnMoveInput;
                _inputBuffer.Interact.started -= OnChargeAttackInput;
                _inputBuffer.Interact.canceled -= OnChargeAttackInput;
                _inputBuffer.Attack.started -= OnAttackInput;
                _inputBuffer.Special.started -= OnSpecialInput;
                _inputBuffer.Finishier.started -= OnFinisherInput;
                _inputBuffer.Avoid.started -= OnAvoid;

                SymphonyDebugLog.DirectLog("player input unregistered");
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
        #endregion

        #region  インターフェースメソッド

        /// <summary>
        ///     攻撃を受けた際の処理
        /// </summary>
        /// <param name="data"></param>
        public override void HitAttack(AttackData data)
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
            OnHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));
            SoundEffectManager.PlaySoundEffect(_hitSound);

            float stunTime = data.IsNockback ? _data.ChargeHitStunTime : _data.HitStunTime; //チャージかに応じて変化
            _stunEndTiming = Time.time + stunTime * (float)MusicEngineHelper.DurationOfBeat; //スタン時間を更新する

            _comboSystem.ComboReset();
            _animeManager.Hit();
        }

        #endregion

        #region プライベートフィールド

        [SerializeField] private BattleBuffTimelineData _battleBuffData;

        #region サウンドクリップ
        [Header("SE")]
        [SerializeField, Tooltip("汎用的な発砲音（通常攻撃の1段目の発砲音）")]
        private string _comboAttackSound;

        [SerializeField, Tooltip("チャージ中")]
        private string _chargeAttackStartSound;

        [SerializeField, Tooltip("チャージ完了")]
        private string _chargeAttackEndSound;

        [SerializeField, Tooltip("強攻撃")]
        private string _chargeAttackSound;

        [SerializeField, Tooltip("回避")]
        private string _avoidSound;

        [SerializeField, Tooltip("被弾")]
        private string _hitSound;

        [SerializeField, Tooltip("Perfect判定時（攻撃・回避兼用）")]
        private string _perfectSound;

        [SerializeField, Tooltip("フローゾーン突入")]
        private string _flowZoneStartSound;

        [SerializeField, Tooltip("フローゾーン終了")]
        private string _flowZoneEndSound;
        #endregion

        private InputBuffer _inputBuffer;
        private BGMManager _bgmManager;
        private ScoreManager _scoreManager;
        private AudioSource _soundEffectSource;

        private IEnemy _target;
        private bool _isBattle;
        [Tooltip("スタンが終了するタイミング")] private float _stunEndTiming;
        private float _chargeAttackTimer;
        private CancellationTokenSource _chargeAttackChargingTokenSource;
        [Tooltip("最後の回避成功のタイミング")] private float _lastAvoidSuccessTiming;
        [Tooltip("パーフェクト攻撃の予約")] private bool _willPerfectAttack;
        [Tooltip("連打防止のための、拍内で一度しか押せないフラグ")] private bool _isThisBeatInputed;

        private PlayerAnimeManager _animeManager;
        private ComboSystem _comboSystem;
        private SpecialSystem _specialSystem;
        private FlowZoneSystem _flowZoneSystem;
        private SkillSystem _skillSystem;

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
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnPhaseChanged)
                    .AddTo(destroyCancellationToken);
            }
            else
            {
                Debug.LogWarning("Phase manager is null");
            }

            _soundEffectSource = AudioManager.GetAudioSource(AudioGroupTypeEnum.SE.ToString());
        }

        private void Update()
        {
            _comboSystem.Update();
        }

        public void Dispose()
        {
            _bgmManager.OnJustChangedBeat -= OnJustBeat;
            _bgmManager.OnNearChangedBeat -= OnNearBeat;

            InputUnregister();
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

            //ターゲットを探す
            if (_isBattle)
            {
                InputRegister();

                var stage = ServiceLocator.GetInstance<BattleSceneManager>();
                _target = stage.EnemyAdmin.GetActiveEnemy();
            }
            else
            {
                _flowZoneSystem.ResetFlowZone();
                _flowZoneSystem.ResetResonanceCount();
            }
        }

        /// <summary>
        ///     移動入力を受け取った際の処理
        /// </summary>
        /// <param name="context"></param>
        private void OnMoveInput(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();
            _animeManager.MoveVector(dir);
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

            var chart = _target.EnemyData.ChartData.Chart;
            var timing = MusicEngineHelper.GetBeatNearerSinceStart() % chart.Length;
            var kind = chart[timing].AttackKind;

            if (kind == ChartKindEnum.Attack)
            {
                AttackFlow();
            }
            else if (kind == ChartKindEnum.Skill)
            {
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
            if (!_isBattle) return;

            var chart = _target.EnemyData.ChartData.Chart;
            int timing = MusicEngineHelper.GetBeatNearerSinceStart();

            bool willChargeAttack = false;
            int chargeAttackRange = Mathf.CeilToInt(_data.ChargeAttackTime); //チャージ攻撃可能な拍数
            for (int i = 0; i <  chargeAttackRange; i++)
            {
                if (ChartKindEnum.Charge == chart[(timing + i) % chart.Length].AttackKind)
                {
                    willChargeAttack = true; //チャージ攻撃が可能ならフラグを立てる
                    break;
                }
            }

            if (!willChargeAttack) return; //直近がチャージ攻撃でなければ何もしない

            switch (context.phase)
            {
                case InputActionPhase.Started: //チャージ開始
                    ChargeAttackCharging();
                    break;

                case InputActionPhase.Canceled: //発動
                    ChargeAttackActivation();
                    break;
            }
        }

        //TODO 必殺技は廃止予定
        /// <summary>
        ///     必殺技
        /// </summary>
        /// <param name="context"></param>
        private void OnSpecialInput(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_target == null) return;

            if (1 <= _specialSystem.SpecialEnergy.CurrentValue)
            {
                _specialSystem.ResetSpecialEnergy();
                _target.HitAttack(new AttackData(2000, true));
            }
        }

        //TODO フィニッシャーは廃止予定
        /// <summary>
        ///     フィニッシャー
        /// </summary>
        /// <param name="context"></param>
        private void OnFinisherInput(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;

            Debug.Log($"{_data.Name} is attacking");
        }

        /// <summary>
        ///     回避
        /// </summary>
        /// <param name="context"></param>
        private void OnAvoid(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_isThisBeatInputed) return; //連打防止

            SymphonyDebugLog.AddText(
                $"[{nameof(PlayerManager)}]" +
                $"{_data.Name} is avoiding");

            var chart = _target.EnemyData.ChartData.Chart;
            var timing = MusicEngineHelper.GetBeatNearerSinceStart() % chart.Length;
            var enemyAttackKind = chart[timing].AttackKind;

            //Charge攻撃は回避できない
            if ((enemyAttackKind & ChartKindEnum.Charge) != 0)
            {
                SymphonyDebugLog.AddText($"Enemy's attack of {enemyAttackKind}(timing:{timing}) can't be avoided");
                SymphonyDebugLog.TextLog();
                return;
            }

            if (!IsAvoidSuccess())
            {
                //失敗時の処理
                OnFailedAvoid?.Invoke();

                SymphonyDebugLog.AddText("avoid result : failed");
                SymphonyDebugLog.TextLog();
                return;
            }

            SymphonyDebugLog.AddText("avoid result : success");
            AvoidFlow();
            SymphonyDebugLog.TextLog();
        }

        /// <summary>
        ///     表拍ビートの処理
        /// </summary>
        private void OnJustBeat()
        {
            if (_willPerfectAttack) //もしパーフェクト攻撃が予約されていれば実行
            {
                SymphonyDebugLog.DirectLog("Quantize perfect attack executed");

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
        }

        /// <summary>
        ///     フローゾーンが開始した時のイベント
        /// </summary>
        private void StartFlowZone() => SoundEffectManager.PlaySoundEffect(_flowZoneStartSound);

        /// <summary>
        ///     フローゾーンが終了した時のイベント
        /// </summary>
        private void EndFlowZone() => SoundEffectManager.PlaySoundEffect(_flowZoneEndSound);


        #endregion

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

            //コンボとアニメーターを同期
            if (_comboSystem != null && _animeManager != null)
            {
                _comboSystem.ComboCount
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

            SymphonyDebugLog.AddText($"{_data.Name} do attack");

            //攻撃が成功したか
            bool isPerfectHit = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.PerfectRange);
            bool isGoodHit = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.GoodRange);

            //評価のログ
            SymphonyDebugLog.AddText($"{(isPerfectHit ? "perfect" : (isGoodHit ? "good" : "miss"))}attack");

            if (isGoodHit) //最低でもGood以上ならヒット
            {
                OnShootComboAttack?.Invoke();
                _comboSystem.Attack(); //コンボを更新

                SoundEffectManager.PlaySoundEffect(_comboAttackSound);
                _isThisBeatInputed = true;

                if (isPerfectHit)
                {
                    if (0 < (float)Music.UnitFromJust - 0.5f) //ビート前なら次のJustまで予約
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
                _comboSystem?.ComboReset();
            }

            SymphonyDebugLog.TextLog();
        }

        /// <summary>
        ///     スキルの一連のフロー
        /// </summary>
        private void SKillFlow()
        {
            if (_isThisBeatInputed) return; //連打防止

            SymphonyDebugLog.AddText($"{_data.Name} do skill");

            OnSkill?.Invoke();
            _animeManager.Skill();
            _skillSystem.StartSkill();

            SymphonyDebugLog.TextLog();
        }

        /// <summary>
        ///     フィニッシャーの一連のフロー
        /// </summary>
        private void FinisherFlow()
        {
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
            _animeManager.Shoot();
            AttackEnemy(_data.ComboAttackPower, _data.PerfectCriticalDamage);
        }

        /// <summary>
        ///     グッド攻撃を行う
        /// </summary>
        private void GoodAttack()
        {
            OnGoodAttack?.Invoke();
            _animeManager.Shoot();
            AttackEnemy(_data.ComboAttackPower);
        }

        /// <summary>
        ///     チャージ攻撃の溜め
        /// </summary>
        private async void ChargeAttackCharging()
        {
            Debug.Log($"{_data.Name} start charge attack");
            OnStartChargeAttack?.Invoke();
            _chargeAttackChargingTokenSource = new();

            _chargeAttackTimer = Time.time; //チャージ開始時間を記録
            SoundEffectManager.PlaySoundEffect(_chargeAttackStartSound);
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

        /// <summary>
        ///     チャージ攻撃を発動する
        /// </summary>
        private void ChargeAttackActivation()
        {
            _chargeAttackChargingTokenSource?.Cancel(); //チャージ中のタスクをキャンセル

            OnShootChargeAttack?.Invoke();
            SoundEffectManager.PlaySoundEffect(_chargeAttackSound);
            AttackEnemy(_data.ChargeAttackPower);

            //フルチャージかどうか
            if (_chargeAttackTimer + MusicEngineHelper.DurationOfBeat * _data.ChargeAttackTime
                < Time.time)
            {
                Debug.Log($"{_data.Name} is full charge attacking");
                OnFullChargeAttack?.Invoke();
            }
            else
            {
                Debug.Log($"{_data.Name} is non full charge attacking");
                OnNonFullChargeAttack?.Invoke();
            }
        }


        /// <summary>
        ///     敵に攻撃を行う
        /// </summary>
        /// <param name="damageScale"></param>
        private void AttackEnemy(float power, float damageScale = 1)
        {
            power *= damageScale;

            if (_battleBuffData) //タイムラインバフ
            {
                var buffData = _battleBuffData.Data;

                var timing = MusicEngineHelper.GetBeatSinceStart();

                for (int i = buffData.Length - 1; i >= 0; i--)
                {
                    if (buffData[i].Timing < timing)
                    {
                        power *= buffData[i].Value;
                        SymphonyDebugLog.AddText($"{buffData[i].Value} buff of {buffData[i].Timing} active");
                        break;
                    }
                }
            }

            if (_skillSystem.IsActive) //スキルがアクティブ中ならバフを適用
            {
                power *= _data.SkillStrangth;
            }

            power *= _damageScale;

            _target.HitAttack(new(power));

            // スコア計算
            float score = power * _data.ComboScoreScale
                [_comboSystem.ComboCount.CurrentValue % _data.ComboScoreScale.Length];
            _scoreManager?.AddScore(Mathf.FloorToInt(power)); // スコアを加算。小数点以下は切り捨てる
        }

        /// <summary>
        ///     回避の一連のフロー
        /// </summary>
        private void AvoidFlow()
        {
            _isThisBeatInputed = true; //連打防止フラグを立てる

            OnSuccessAvoid?.Invoke();
            SoundEffectManager.PlaySoundEffect(_avoidSound);

            _animeManager.Avoid();
            _flowZoneSystem.SuccessResonance();
            _lastAvoidSuccessTiming = Time.time;
        }

        /// <summary>
        ///     回避が成功したかどうかを判定する
        /// </summary>
        /// <returns>成功しているかどうか</returns>
        private bool IsAvoidSuccess()
        {
            var chartData = _target.EnemyData.ChartData;
            var timing = MusicEngineHelper.GetBeatNearerSinceStart() % chartData.Chart.Length;

            //敵が攻撃しないならミス
            if (!chartData.IsEnemyAttack(timing))
            {
                SymphonyDebugLog.AddText($"Enemy not attack at timing {timing}");
                return false;
            }

            //避ける範囲内かどうか判定
            if (!MusicEngineHelper
                .IsTimingWithinAcceptableRange(_data.AvoidRange)) //回避可能タイミングの範囲外ならミス
            {
                SymphonyDebugLog.AddText($"Miss Avoid at timing {timing}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     フィニッシャーが可能かどうかを判定する
        /// </summary>
        /// <returns></returns>
        private bool IsFinisherable()
        {
            return _target.IsFinisherable;
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