using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using System;
using System.Threading;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     敵キャラクターの管理クラス
    /// </summary>
    public class EnemyManager : CharacterManagerB<EnemyData>, IEnemy, IDisposable
    {
        public event Action OnFinisherable;

        public event Action OnShootAttack;
        public event Action OnShootNormalAttack;
        public event Action OnShootChargeAttack;

        public event Action OnHitNockBackAttack
        {
            add => _onHitNockBackAttack.Event += value;
            remove => _onHitNockBackAttack.Event -= value;
        }

        public CharacterHealthSystem HealthSystem => _healthSystem;
        public bool IsFinisherable => _canFinisher;

        public void Dispose()
        {
            InputUnregister();
            _disposable?.Dispose();
        }

        public EnemyAnimeManager GetEnemyAnimeManager()
        {
            return _animeManager;
        }

        /// <summary>
        ///     入力の登録を行う
        /// </summary>
        public void InputRegister()
        {
            if (_bgmManager)
            {
                _bgmManager.OnNearChangedBeat += OnAttack;
                _bgmManager.OnJustChangedBeat += OnPrepareAttack;
            }
        }

        /// <summary>
        ///     入力の登録を解除する
        /// </summary>
        public void InputUnregister()
        {
            if (_bgmManager)
            {
                _bgmManager.OnNearChangedBeat -= OnAttack;
                _bgmManager.OnJustChangedBeat -= OnPrepareAttack;
            }
        }

        /// <summary>
        ///     戦闘を有効化する
        /// </summary>
        public void SetActive()
        {
            _bgmManager = ServiceLocator.GetInstance<BGMManager>();
            _target = ServiceLocator.GetInstance<PlayerManager>();

            if (!_bgmManager)
            {
                Debug.LogWarning($"{_data.name} has no music engine");
            }

            SetActiveModel(true);

            _disposable = new();
            
            //フェーズ変更時のイベント登録
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            phaseManager.CurrentPhaseProp
                .Skip(1) // 初期フェーズをスキップ
                .Subscribe(OnPhaseChange)
                .AddTo(_disposable);
            _phaseManager = phaseManager;
        }

        public void SetDiactive()
        {
            SetActiveModel(false);
            Dispose();
        }
        
        /// <summary>
        ///     モデルの表示・非表示を切り替える
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveModel(bool active)
        {
            Debug.Log(active);
            _modelParent.SetActive(active);
        }

        public override void HitAttack(AttackData data)
        {
            base.HitAttack(data);

            _healthSystem?.HealthChange(-data.Damage);

            _onHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));

            FinisherableCheck(); //フィニッシャー可能かどうかを確認

            if (data.IsNockback) //ノックバックする
            {
                _onHitNockBackAttack?.Invoke(); //クリティカルヒットイベントを発火
                Nockback();
                
                // ノックバック付きチャージ攻撃を受けた時のSE
                SoundEffectManager.PlaySoundEffect(_seData.MiddleDamage);
                return;
            }
            
            // プレイヤーから通常攻撃を受けた時のSE
            SoundEffectManager.PlaySoundEffect(_seData.SmallDamage);
        }

        public Transform NormalAttackRandomHit()
        {
            if (0 <= _normalAttackHitPositions.Length) return null;

            int index = UnityEngine.Random.Range(0, _normalAttackHitPositions.Length);
            Transform target = _normalAttackHitPositions[index];

            if (_normalAttackHitPerticle != null)
            { Instantiate(_normalAttackHitPerticle, target.position, target.rotation); }

            return target;
        }

        EnemyData IEnemy.EnemyData => _data;

        [SerializeField]
        private UnityEventWrapper _onHitNockBackAttack = new();

        [SerializeField, Tooltip("モデルの親オブジェクト")]
        private GameObject _modelParent;
        [SerializeField]
        private Transform[] _normalAttackHitPositions;
        [SerializeField]
        private GameObject _normalAttackHitPerticle;

        [SerializeField]
        private RingIndicatorData _indicatorData;
        
        [SerializeField]
        private EnemySEDataSO _seData;

        private BGMManager _bgmManager;

        private PlayerManager _target;
        private PhaseManager _phaseManager;

        private bool _canFinisher;
        private bool _isKnockback;

        private bool _isFlowZone;

        private int _normalAttackLength;
        private int _chargeAttackLength;

        private EnemyAnimeManager _animeManager;
        private CharacterHealthSystem _healthSystem;

        private CompositeDisposable _disposable = new CompositeDisposable();

        protected override async void Awake()
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                _animeManager = new(animator);
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no Animator");
            }

            _healthSystem = new(_data);

            _normalAttackLength =
                _indicatorData.GetRingData(ChartKindEnum.Normal).RingPrefab
                    .GetComponent<RingIndicatorBase>()
                    .EffectLength;
            _chargeAttackLength =
                _indicatorData.GetRingData(ChartKindEnum.Charge).RingPrefab
                    .GetComponent<RingIndicatorBase>()
                    .EffectLength;

            SetActiveModel(false); //初期はモデル表示を無くす

            PlayerManager playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            playerManager.FlowZoneSystem.IsFlowZone
                .Subscribe(value => _isFlowZone = value)
                .AddTo(destroyCancellationToken);
        }

        private void OnDestroy()
        {
            InputUnregister();
        }

        /// <summary>
        ///     フェーズが変わったときの処理
        /// </summary>
        private void OnPhaseChange(PhaseEnum phase)
        {
            if (phase == PhaseEnum.Battle) //戦闘フェーズが始まったら動き始める
            {
                InputRegister();
            }
            if (phase == PhaseEnum.Movie) //ムービーフェーズが始まったら動きを止める
            {
                InputUnregister();
                SetActiveModel(false); //モデルを非表示にする
            }
        }

        private void OnAttack()
        {   
            if (!_bgmManager) return;

            if (_target.IsStunning()) return; //プレイヤーがスタン中は攻撃しない

            int timing = MusicEngineHelper.GetBeatSinceStart();

            ChartData chartData = _data.GetChartDataByFlowZone(_isFlowZone);

            if (chartData.IsEnemyAttack(timing)) //攻撃タイミングかどうかを確認
            {
                # region デバッグログ
                Debug.Log($"{_data.name} " +
                    $"{_data.ChartData[timing].AttackKind} attack\n" +
                    $"timing : {timing}");
                #endregion

                OnShootAttack?.Invoke();

                ChartKindEnum attackKind = chartData[timing].AttackKind;

                int effectLength = _indicatorData.GetRingData(attackKind).EffectLength;
                float startTiming = Time.time - (float)MusicEngineHelper.DurationOfBeat * effectLength;
                if (_phaseManager.IsAnotherPhaseByTiming(startTiming)) return;

                if (attackKind == ChartKindEnum.Normal) //ノーマルアタック
                {
                    if (_isKnockback) return; //ノックバック中は攻撃しない

                    _target.HitAttack(new AttackData(1));
                    OnShootNormalAttack?.Invoke();
                    _animeManager.Attack();
                    
                    // 殴り攻撃のSE
                    SoundEffectManager.PlaySoundEffect(_seData.Punch);
                }
                else if (attackKind == ChartKindEnum.Charge) //チャージアタック
                {
                    if (!_isKnockback) //ノックバック中でない場合のみチャージアタックを行う
                    {
                        _target.HitAttack(new AttackData(1, true));
                        OnShootChargeAttack?.Invoke();
                        
                        // ビーム攻撃のSE
                        SoundEffectManager.PlaySoundEffect(_seData.BeamAttack);
                    }

                    _animeManager.ChargeAttack();
                }
            }
        }

        private void OnPrepareAttack()
        {
            if (_animeManager == null) return;

            int timing = MusicEngineHelper.GetBeatSinceStart();
            ChartData chartData = _data.GetChartDataByFlowZone(_isFlowZone);

            if (chartData[timing + _normalAttackLength].AttackKind == ChartKindEnum.Normal) //ノーマルアタックでない場合は何もしない
            {
                _animeManager.PreAttack();
            }
            else if (chartData[timing + _chargeAttackLength].AttackKind == ChartKindEnum.Charge) //チャージアタックでない場合は何もしない
            {
                _animeManager.PreChargeAttack();
                
                // ビームの溜めはじめのSE
                SoundEffectManager.PlaySoundEffect(_seData.BeamCharge);
            }
        }

        /// <summary>
        ///     フィニッシャー可能かどうかを確認する
        /// </summary>
        private void FinisherableCheck()
        {
            if (_canFinisher) return; //初回時のみ

            //フィニッシャー可能範囲の処理
            if (_healthSystem.Health / _healthSystem.MaxHealth
                <= _data.FinisherThreshold / 100) //フィニッシャー可能割合の判定
            {
                Debug.Log("Finisherable event triggered for " + _data.name);

                _canFinisher = true;
                OnFinisherable?.Invoke();
            }
        }

        private async void Nockback()
        {
            _isKnockback = true;
            _animeManager?.KnockBack(_isKnockback);
            await Awaitable.WaitForSecondsAsync(_data.NockbackTime, destroyCancellationToken);
            _isKnockback = false;
            _animeManager?.KnockBack(_isKnockback);
        }
    }
}
