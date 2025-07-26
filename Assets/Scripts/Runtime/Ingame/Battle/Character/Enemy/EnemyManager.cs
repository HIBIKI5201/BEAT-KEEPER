using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using System;
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

        public CharacterHealthSystem HealthSystem => _healthSystem;
        public bool IsFinisherable => _canFinisher;

        public void Dispose()
        {
            InputUnregister();
        }

        /// <summary>
        ///     入力の登録を行う
        /// </summary>
        public void InputRegister()
        {
            if (_bgmManager)
            {
                _bgmManager.OnNearChangedBeat += OnAttack;
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

            //フェーズ変更時のイベント登録
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            phaseManager.CurrentPhaseProp
                .Subscribe(OnPhaseChange)
                .AddTo(destroyCancellationToken);
        }

        /// <summary>
        ///     モデルの表示・非表示を切り替える
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveModel(bool active)
        {
            _modelParent.SetActive(active);
        }

        public override void HitAttack(AttackData data)
        {
            base.HitAttack(data);

            _healthSystem?.HealthChange(-data.Damage);

            OnHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));

            FinisherableCheck(); //フィニッシャー可能かどうかを確認

            if (data.IsNockback) //ノックバックする
            {
                Nockback();
            }
        }

        public Transform NormalAttackRandomHit()
        {
            int index = UnityEngine.Random.Range(0, _normalAttackHitPositions.Length);
            Transform target = _normalAttackHitPositions[index];

            if (_normalAttackHitPerticle != null)
                { Instantiate(_normalAttackHitPerticle, target.position, target.rotation); }

            return target;
        }

        EnemyData IEnemy.EnemyData => _data;

        [SerializeField, Tooltip("モデルの親オブジェクト")]
        private GameObject _modelParent;
        [SerializeField]
        private Transform[] _normalAttackHitPositions;
        [SerializeField]
        private GameObject _normalAttackHitPerticle;

        private BGMManager _bgmManager;

        private PlayerManager _target;

        private bool _canFinisher;
        private bool _isKnockback;

        private EnemyAnimeManager _animeManager;
        private CharacterHealthSystem _healthSystem;

        #region モック用の機能

        [SerializeField, Obsolete("モック用")] private ParticleSystem _particleSystem;

        #endregion

        protected override void Awake()
        {
            if (TryGetComponent(out Animator animator))
            {
                _animeManager = new(animator);
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no Animator");
            }

            _healthSystem = new(_data);

            SetActiveModel(false); //初期はモデル表示を無くす
        }

        private void OnDestroy()
        {
            InputRegister();
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
        }

        private void OnAttack()
        {
            if (!_bgmManager) return;
            if (_isKnockback) return; //ノックバック中は攻撃しない

            if (_target.IsStunning()) return; //プレイヤーがスタン中は攻撃しない

            var timing = MusicEngineHelper.GetBeatSinceStart();

            if (_data.ChartData.IsEnemyAttack(timing)) //攻撃タイミングかどうかを確認
            {
                # region デバッグログ
                Debug.Log($"{_data.name} " +
                    $"{_data.ChartData.Chart[(timing) % _data.ChartData.Chart.Length].AttackKind} attack\n" +
                    $"timing : {timing}");
                #endregion

                OnShootAttack?.Invoke();

                var attackKind = _data.ChartData.Chart[timing % _data.ChartData.Chart.Length].AttackKind;

                if (attackKind == ChartKindEnum.Normal) //ノーマルアタック
                {
                    _target.HitAttack(new AttackData(1));
                    OnShootNormalAttack?.Invoke();
                }
                else if (attackKind == ChartKindEnum.Charge) //チャージアタック
                {
                    _target.HitAttack(new AttackData(1, true));
                    OnShootChargeAttack?.Invoke();
                }

                if (_particleSystem)
                {
                    _particleSystem?.Play();
                }
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
            _animeManager?.KnockBack();
            await Awaitable.WaitForSecondsAsync(_data.NockbackTime, destroyCancellationToken);
            _isKnockback = false;
        }
    }
}
