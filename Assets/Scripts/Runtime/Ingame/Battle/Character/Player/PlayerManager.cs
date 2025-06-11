using BeatKeeper.Runtime.Ingame.Battle;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     プレイヤーのマネージャークラス
    /// </summary>
    public class PlayerManager : CharacterManagerB<PlayerData>, IDisposable
    {
        public CinemachineCamera PlayerCamera => _playerCamera;
        public ComboSystem ComboSystem => _comboSystem;
        public SpecialSystem SpecialSystem => _specialSystem;
        public FlowZoneSystem FlowZoneSystem => _flowZoneSystem;

        [SerializeField] private BattleBuffTimelineData _battleBuffData;

        private InputBuffer _inputBuffer;
        private MusicEngineHelper _musicEngine;
        private ScoreManager _scoreManager;
        private CinemachineCamera _playerCamera;

        private IEnemy _target;
        private bool _isBattle;
        private float _chargeAttackTimer;
        private float _avoidSuccessTiming;
        private bool _willPerfectAttack;

        private PlayerAnimeManager _animeManager;
        private ComboSystem _comboSystem;
        private SpecialSystem _specialSystem;
        private FlowZoneSystem _flowZoneSystem;

        #region イベント
        public event Action OnShootComboAttack;
        public event Action OnPerfectAttack;
        public event Action OnGoodAttack;

        public event Action OnShootChargeAttack;
        public event Action OnFullChargeAttack;

        public event Action OnNonFullChargeAttack;
        public event Action OnNormalAvoid;
        public event Action OnJustAvoid;
        #endregion

        #region モック用の機能

        [Obsolete("モック用"), SerializeField, Tooltip("攻撃のダメージ倍率"), Min(0.1f)]
        private float _damageScale = 1;

        [Obsolete("モック用"), SerializeField] private ParticleSystem _particleSystem;

        #endregion

        #region ライフサイクル

        protected override async void Awake()
        {
            //システムの初期化
            _comboSystem = new ComboSystem(_data);
            _specialSystem = new SpecialSystem();
            _flowZoneSystem =
                new FlowZoneSystem(
                    await ServiceLocator.GetInstanceAsync<MusicEngineHelper>(),
                    16);

            //コンポーネント取得
            if (TryGetComponent(out Animator animator))
                _animeManager = new(animator);
            else Debug.LogWarning("Character animator component not found");

            _playerCamera = GetComponentInChildren<CinemachineCamera>();
            if (_playerCamera)
            {
                //カメラを敵に向ける
                _playerCamera.LookAt = (await ServiceLocator.GetInstanceAsync<BattleSceneManager>()).EnemyAdmin.GetActiveEnemy().transform;
            }

            OnShootComboAttack += _particleSystem.Play;
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _scoreManager = ServiceLocator.GetInstance<ScoreManager>();
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();

            _musicEngine.OnJustChangedBeat += OnBeat;
            _musicEngine.OnNearChangedBeat += OnNearBeat;

            InputRegister();

            if (!_musicEngine)
                Debug.LogWarning("Music engine is null");

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
        }

        private void Update()
        {
            _comboSystem.Update();
        }

        public void Dispose()
        {
            _musicEngine.OnJustChangedBeat -= OnBeat;
            _musicEngine.OnNearChangedBeat -= OnNearBeat;

            InputUnregister();
        }

        #endregion

        #region 入力の購買

        /// <summary>
        ///     入力を購買する
        /// </summary>
        public void InputRegister()
        {
            if (_inputBuffer)
            {
                _inputBuffer.Move.performed += OnMove;
                _inputBuffer.Move.canceled += OnMove;
                _inputBuffer.Interact.started += OnChargeAttack;
                _inputBuffer.Interact.canceled += OnChargeAttack;
                _inputBuffer.Attack.started += OnAttack;
                _inputBuffer.Special.started += OnSpecial;
                _inputBuffer.Finishier.started += OnFinisher;
                _inputBuffer.Avoid.started += OnAvoid;
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
                _inputBuffer.Move.performed -= OnMove;
                _inputBuffer.Move.canceled -= OnMove;
                _inputBuffer.Interact.started -= OnChargeAttack;
                _inputBuffer.Interact.canceled -= OnChargeAttack;
                _inputBuffer.Attack.started -= OnAttack;
                _inputBuffer.Special.started -= OnSpecial;
                _inputBuffer.Finishier.started -= OnFinisher;
                _inputBuffer.Avoid.started -= OnAvoid;
            }
            else
            {
                Debug.LogWarning("Input buffer is null");
            }
        }
        #endregion

        /// <summary>
        ///     攻撃を受けた際の処理
        /// </summary>
        /// <param name="data"></param>
        public override void HitAttack(AttackData data)
        {
            //無敵時間判定
            if (_avoidSuccessTiming + _data.AvoidInvincibilityTime * MusicEngineHelper.DurationOfBeat > Time.time)
            {
                Debug.Log("During Avoid Invincibility Time");
                return;
            }

            base.HitAttack(data);
            OnHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));

            _comboSystem.ComboReset();
        }

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
                var stage = ServiceLocator.GetInstance<BattleSceneManager>();
                _target = stage.EnemyAdmin.GetActiveEnemy();
            }
            else
            {
                _flowZoneSystem.ResetFlowZone();
            }
        }

        /// <summary>
        ///     移動入力を受け取った際の処理
        /// </summary>
        /// <param name="context"></param>
        private void OnMove(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();
            _animeManager.MoveVector(dir);
        }

        /// <summary>
        ///     コンボ攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_target == null) return;

            SymphonyDebugLog.AddText($"{_data.Name} is attacking");

            //攻撃が成功したか
            bool isPerfectHit = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.PerfectRange);
            bool isGoodHit = MusicEngineHelper.IsTimingWithinAcceptableRange(_data.GoodRange);

            //評価のログ
            SymphonyDebugLog.AddText($"{(isPerfectHit ? "perfect" : (isGoodHit ? "good" : "miss"))}attack");

            if (isPerfectHit || isGoodHit) //missじゃない時は攻撃処理
            {
                OnShootComboAttack?.Invoke();
                _comboSystem.Attack();

                if (isPerfectHit)
                {
                    if (0 < (float)Music.UnitFromJust - 0.5f) //ビート前なら予約
                    {
                        _willPerfectAttack = true;
                    }
                    else //ビート後なら即座に実行
                    {
                        PerfectAttack();
                    }
                }
                else if (isGoodHit) //Goodなら即座に実行する
                {
                    OnGoodAttack?.Invoke();
                    AttackEnemy();
                }
            }
            else
            {
                _comboSystem.ComboReset();
            }

            SymphonyDebugLog.TextLog();
        }

        /// <summary>
        ///     溜め攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnChargeAttack(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;

            switch (context.phase)
            {
                case InputActionPhase.Started: //チャージ開始
                    Debug.Log($"{_data.Name} start charge attack");
                    _chargeAttackTimer = Time.time;
                    break;

                case InputActionPhase.Canceled:
                    OnShootChargeAttack?.Invoke();

                    if (_chargeAttackTimer + MusicEngineHelper.DurationOfBeat * _data.ChargeAttackTime <
                        Time.time) //フルチャージかどうか

                    {
                        OnFullChargeAttack?.Invoke();
                        Debug.Log($"{_data.Name} is full charge attacking");
                    }
                    else
                    {
                        Debug.Log($"{_data.Name} is non full charge attacking");
                        OnNonFullChargeAttack?.Invoke();
                    }

                    break;
            }
        }

        /// <summary>
        ///     必殺技
        /// </summary>
        /// <param name="context"></param>
        private void OnSpecial(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_target == null) return;

            if (1 <= _specialSystem.SpecialEnergy.CurrentValue)
            {
                _specialSystem.ResetSpecialEnergy();
                _target.HitAttack(new AttackData(2000, true));
            }
        }

        /// <summary>
        ///     フィニッシャー
        /// </summary>
        /// <param name="context"></param>
        private void OnFinisher(InputAction.CallbackContext context)
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

            OnNormalAvoid?.Invoke();

            _animeManager.Avoid();

            Debug.Log($"{_data.Name} is avoiding");

            var timing = MusicEngineHelper.GetBeatsSinceStart() % 32;

            //nターン後までに攻撃があるかどうか
            (bool willAttack, ChartKindEnum enemyAttackKind) = IsSuccessAvoid(timing);

            if (willAttack)
            {
                //SuperとCharge攻撃は回避できない
                if ((enemyAttackKind & (ChartKindEnum.Super | ChartKindEnum.Charge)) != 0)
                {
                    Debug.Log($"Enemy's attack of {enemyAttackKind} can't be avoided");
                    return;
                }

                Debug.Log($"Success Avoid");
                OnJustAvoid?.Invoke();

                _flowZoneSystem.SuccessResonance();
                _avoidSuccessTiming = Time.time;
            }
        }

        /// <summary>
        ///     ビートが変わった際の処理
        /// </summary>
        private void OnBeat()
        {
            if (_willPerfectAttack) //もしパーフェクト攻撃が予約されていれば実行
            {
                SymphonyDebugLog.DirectLog("Perfect attack executed");

                PerfectAttack();
            }
        }

        private void OnNearBeat() => _willPerfectAttack = false;

        /// <summary>
        ///     パーフェクト攻撃を行う
        /// </summary>
        private void PerfectAttack()
        {
            OnPerfectAttack?.Invoke();
            _specialSystem.AddSpecialEnergy(0.05f);

            AttackEnemy(_data.PerfectCriticalDamage);
        }

        /// <summary>
        ///     敵に攻撃を行う
        /// </summary>
        /// <param name="damageScale"></param>
        private void AttackEnemy(float damageScale = 1)
        {
            //コンボに応じたダメージ
            var power = _data.ComboAttackPower;

            power *= damageScale;

            if (_battleBuffData) //タイムラインバフ
            {
                var buffData = _battleBuffData.Data;

                var timing = MusicEngineHelper.GetBeatsSinceStart();

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

            power *= _damageScale;

            _target.HitAttack(new(power));

            // スコア計算
            float score = power * _data.ComboScoreScale
                [_comboSystem.ComboCount.CurrentValue % _data.ComboScoreScale.Length];
            _scoreManager?.AddScore(Mathf.FloorToInt(power)); // スコアを加算。小数点以下は切り捨てる
        }

        /// <summary>
        ///     回避が成功したかどうかを判定する
        /// </summary>
        /// <param name="timing"></param>
        /// <returns></returns>
        public (bool, ChartKindEnum) IsSuccessAvoid(int timing)
        {
            bool willAttack = false;
            for (int i = timing; i < timing + 3; i++)
            {
                willAttack |= _target.EnemyData.ChartData.IsEnemyAttack(i);

                if (willAttack)
                {
                    return (true, _target.EnemyData.ChartData.Chart[i].AttackKind);
                }
            }

            return (false, ChartKindEnum.None);
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