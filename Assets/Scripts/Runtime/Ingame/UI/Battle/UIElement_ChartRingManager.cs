using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
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
        [SerializeField, Tooltip("リングのデータ群")] private RingData[] _ringDatas;

        private StageEnemyAdmin _enemies;
        private MusicEngineHelper _musicEngineHelper;

        private EnemyData _targetData;
        private Dictionary<ChartKindEnum, ObjectPool<UIElement_RingIndicator>> _ringPools = new();

        private Action _onBeat;
        private void Start()
        {
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            phaseManager.CurrentPhaseProp.Subscribe(OnChangePhase).AddTo(destroyCancellationToken);


            #region 各リングのオブジェクトプールを初期化
            for (int i = 0; i < _ringDatas.Length; i++)
            {
                var index = i;
                var data = _ringDatas[i];

                if (!data.RingPrefab)
                    return;

                data.ApearTiming = data.RingPrefab.GetComponent<UIElement_RingIndicator>()?.EffectLength ?? 5;

                _ringPools.Add(
                    data.AttackKind,
                    new(
                        createFunc: () =>
                        {
                            var go = Instantiate(data.RingPrefab);
                            go.transform.SetParent(transform);

                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                manager.gameObject.SetActive(false);
                                return manager;
                            }
                            else
                            {
                                SymphonyDebugLog.DirectLog("このプレハブはリングインジケーターがアタッチされていません", SymphonyDebugLog.LogKind.Warning);
                                return null;
                            }
                        },
                        actionOnGet: go =>
                        {
                            go.gameObject.SetActive(true);
                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                _onBeat += manager.AddCount;
                            }
                        },
                        actionOnRelease: go =>
                        {
                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                _onBeat -= manager.AddCount;
                            }
                        },
                        actionOnDestroy: go => Destroy(go),
                        defaultCapacity: data.DefaultCapacity,
                        maxSize: 10));
            }
            #endregion
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
            if (_musicEngineHelper)
            {
                _musicEngineHelper.OnJustChangedBeat -= OnBeat;
            }
        }

        /// <summary>
        ///     ビートが変わったときの処理
        /// </summary>
        private void OnBeat()
        {
            if (_enemies == null) return;

            var timing = MusicEngineHelper.GetBeatsSinceStart();

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
                    ring.OnGet(() => op.Release(ring), //終了時のイベントを設定
                        element.Position);
                }
            }
        }

        private void OnGUI()
        {
            if (_musicEngineHelper)
            {
                var currentBeat = MusicEngineHelper.GetBeatsSinceStart();

                GUI.Label(new Rect(10, 10, 200, 20), $"Current Beat: {currentBeat}");
            }
        }
    }

    [Serializable]
    public class RingData
    {
        [HideInInspector] public int ApearTiming;

        [SerializeField] private ChartKindEnum _attackKind;
        public ChartKindEnum AttackKind => _attackKind;

        [SerializeField] private GameObject _ringPrefab;
        public GameObject RingPrefab => _ringPrefab;

        [SerializeField, Tooltip("リングの事前用意数（ある程度の同時出現数を入力）")] private int _defaultCapacity = 3;
        public int DefaultCapacity => _defaultCapacity;
    }
}
