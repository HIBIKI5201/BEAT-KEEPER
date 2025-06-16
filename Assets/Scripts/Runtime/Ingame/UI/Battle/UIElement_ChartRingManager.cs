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
        [SerializeField]
        private RingIndicatorData _ringIndicatorData;

        private StageEnemyAdmin _enemies;
        private BGMManager _musicEngineHelper;

        private Action _onBeat;

        private PlayerManager _player;
        private readonly Dictionary<ChartKindEnum, ObjectPool<RingIndicatorBase>> _ringPools = new();
        private readonly HashSet<RingIndicatorBase> _activeRingIndicator = new();
        private int[] _appearTiming;

        private EnemyData _targetData;

        private async void Start()
        {
            _player = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<BGMManager>();
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnChangePhase).AddTo(destroyCancellationToken);
            }

            ObjectPoolInitialize();
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

                _musicEngineHelper.OnJustChangedBeat += OnJustBeat;
                enemy.OnFinisherable += OnFinisherable;
            }
        }

        /// <summary>
        ///     ビートが変わったときの処理
        /// </summary>
        private void OnJustBeat()
        {
            if (_enemies == null) return;

            var timing = MusicEngineHelper.GetBeatSinceStart();

            _onBeat?.Invoke(); //リングのカウントを更新 

            //新しいリングを監視
            for(int i = 0; i < _ringIndicatorData.RingDatas.Length; i++)
            {
                //インジケーターのデータを取得
                var data = _ringIndicatorData.RingDatas[i]; 

                //インジケーターに対応する譜面を取得
                var element = _targetData.ChartData
                    .Chart[(timing + _appearTiming[i]) % 32];

                //譜面がインジケーターと同じか判定
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

        /// <summary>
        ///     フィニッシャー待機状態になった時の処理
        /// </summary>
        private void OnFinisherable()
        {
            UnregisterOnJustBeat();
            ReleaseAllActiveIndicator();
        }

        /// <summary>
        ///     全てのアクティブなインジケーターを非アクティブ化する
        /// </summary>
        public void ReleaseAllActiveIndicator()
        {
            foreach (var ring in _activeRingIndicator)
            {
                ring.End();
            }

            _activeRingIndicator.Clear();
        }

        /// <summary>
        ///     ビートの購買を解除する
        /// </summary>
        private void UnregisterOnJustBeat()
        {
            if (_musicEngineHelper) _musicEngineHelper.OnJustChangedBeat -= OnJustBeat;
        }

        /// <summary>
        ///     オブジェクトプールを初期化する
        /// </summary>
        private void ObjectPoolInitialize()
        {
            //タイミングを記憶する配列を初期化
            _appearTiming = new int[_ringIndicatorData.RingDatas.Length];

            //各オブジェクトプールを初期化し辞書に格納していく
            for (var i = 0; i < _ringIndicatorData.RingDatas.Length; i++)
            {
                var data = _ringIndicatorData.RingDatas[i];

                if (!data.RingPrefab)
                    return;

                _appearTiming[i] = data.RingPrefab.GetComponent<RingIndicatorBase>()?.EffectLength ?? 5;

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
        }
    }
}