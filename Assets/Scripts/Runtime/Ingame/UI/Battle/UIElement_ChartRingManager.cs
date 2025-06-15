using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     譜面のリング表示をするマネージャー
    /// </summary>
    public class UIElement_ChartRingManager : MonoBehaviour
    {
        [SerializeField][Tooltip("リングのデータ群")] private RingData[] _ringDatas;
        private StageEnemyAdmin _enemies;
        private BGMManager _musicEngineHelper;

        private Action _onBeat;

        private PlayerManager _player;
        private readonly Dictionary<ChartKindEnum, ObjectPool<RingIndicatorBase>> _ringPools = new();
        private readonly HashSet<RingIndicatorBase> _activeRingIndicator = new();

        private EnemyData _targetData;

        private async void Start()
        {
            _player = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<BGMManager>();
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            phaseManager.CurrentPhaseProp.Subscribe(OnChangePhase).AddTo(destroyCancellationToken);


            #region 各リングのオブジェクトプールを初期化

            for (var i = 0; i < _ringDatas.Length; i++)
            {
                var index = i;
                var data = _ringDatas[i];

                if (!data.RingPrefab)
                    return;

                data.ApearTiming = data.RingPrefab.GetComponent<RingIndicatorBase>()?.EffectLength ?? 5;

                _ringPools.Add(
                    data.AttackKind,
                    new ObjectPool<RingIndicatorBase>(
                        () =>
                        {
                            var go = Instantiate(data.RingPrefab);
                            go.transform.SetParent(transform);

                            if (go.TryGetComponent<RingIndicatorBase>(out var manager))
                            {
                                manager.gameObject.SetActive(false);
                                manager.OnInit(_player);
                                return manager;
                            }

                            SymphonyDebugLog.DirectLog("このプレハブはリングインジケーターがアタッチされていません",
                                SymphonyDebugLog.LogKind.Warning);
                            return null;
                        },
                        manager =>
                        {
                            manager.gameObject.SetActive(true);
                            _onBeat += manager.AddCount;
                        },
                        manager =>
                        {
                            manager.gameObject.SetActive(false);
                            _onBeat -= manager.AddCount;
                        },
                        Destroy,
                        defaultCapacity: data.DefaultCapacity,
                        maxSize: 10));
            }

            #endregion
        }

        private void OnDestroy()
        {
            foreach(var data in _activeRingIndicator)
            {
                Destroy(data.gameObject);
            }
        }

        private void OnGUI()
        {
            if (_musicEngineHelper)
            {
                var currentBeat = MusicEngineHelper.GetBeatSinceStart();

                GUI.Label(new Rect(10, 10, 200, 20), $"Current Beat: {currentBeat}");
            }
        }

        /// <summary>
        ///     フェーズが変わったときの処理
        /// </summary>
        /// <param name="phase"></param>
        private void OnChangePhase(PhaseEnum phase)
        {
            if (phase == PhaseEnum.Battle) //譜面を取得してビートの購買を開始

            {
                _enemies = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
                var enemy = _enemies.GetActiveEnemy();
                _targetData = enemy.Data;

                _musicEngineHelper.OnJustChangedBeat += OnBeat;
                enemy.OnFinisherable += UnregisterOnBeat;
            }
        }

        /// <summary>
        ///     ビートの購買を解除する
        /// </summary>
        private void UnregisterOnBeat()
        {
            if (_musicEngineHelper) _musicEngineHelper.OnJustChangedBeat -= OnBeat;
        }

        /// <summary>
        ///     ビートが変わったときの処理
        /// </summary>
        private void OnBeat()
        {
            if (_enemies == null) return;

            var timing = MusicEngineHelper.GetBeatSinceStart();

            _onBeat?.Invoke(); //リングのカウントを更新 

            //新しいリングを監視
            foreach (var data in _ringDatas)
            {
                var element = _targetData.ChartData.Chart[(timing + data.ApearTiming) % 32];

                if (element.AttackKind != data.AttackKind)
                    continue;

                //リングを生成
                if (_ringPools.TryGetValue(data.AttackKind, out var op))
                {
                    var ring = op.Get(); //リングを取得
                    _activeRingIndicator.Add(ring);
                    ring.OnGet(() => //終了時のイベントを設定
                    {
                        op.Release(ring); //オブジェクトを非アクティブに
                        _activeRingIndicator.Remove(ring); //アクティブリストから除外
                    },
                        element.Position);
                }
            }
        }
    }

    [Serializable]
    public class RingData
    {
        [HideInInspector] public int ApearTiming;

        [SerializeField] private ChartKindEnum _attackKind;

        [SerializeField] private GameObject _ringPrefab;

        [SerializeField]
        [Tooltip("リングの事前用意数（ある程度の同時出現数を入力）")]
        private int _defaultCapacity = 3;

        public ChartKindEnum AttackKind => _attackKind;
        public GameObject RingPrefab => _ringPrefab;
        public int DefaultCapacity => _defaultCapacity;
    }
}