using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     譜面のリング表示をするマネージャー
    /// </summary>
    public class UIElement_ChartRingManager : MonoBehaviour
    {
        public HashSet<RingIndicatorBase> ActiveRingIndicators => _activeRingIndicator;

        [SerializeField]
        private RingIndicatorData _ringIndicatorData;

        [SerializeField, Tooltip("攻撃予告")]
        private AudioClip _apearSound;

        private StageEnemyAdmin _enemies;
        private BGMManager _musicEngineHelper;

        private Action _onBeat;

        private PlayerManager _player;
        private readonly Dictionary<ChartKindEnum, ObjectPool<RingIndicatorBase>> _ringPools = new();
        private readonly HashSet<RingIndicatorBase> _activeRingIndicator = new();
        private int[] _appearTiming;
        private AudioSource _soundEffectSource;

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

            _soundEffectSource = AudioManager.GetAudioSource(AudioGroupTypeEnum.SE.ToString());

            ObjectPoolInitialize();
        }

        private void OnDestroy()
        {
            foreach(var data in _activeRingIndicator)
            {
                Destroy(data.gameObject);
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
            var chart = _targetData.ChartData.Chart;

            //新しいリングを監視
            for (int i = 0; i < _ringIndicatorData.RingDatas.Length; i++)
            {
                //インジケーターのデータを取得
                var data = _ringIndicatorData.RingDatas[i];
                //インジケーターに対応する譜面を取得
                var element = chart[(timing + _appearTiming[i]) % chart.Length];

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
                        element.Position, (timing + _appearTiming[i]) % chart.Length);

                    _soundEffectSource?.PlayOneShot(_apearSound);
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
            List<RingIndicatorBase> rings = _activeRingIndicator.ToList();
            foreach (var ring in rings)
            {
                ring.End();
            }

            _activeRingIndicator.Clear();
        }

        /// <summary>
        ///     全てのインジケーターの残り時間をチェックする
        /// </summary>
        public void CheckAllRingIndicatorRemainTime()
        {
            for(int i = 0; i < _activeRingIndicator.Count; i++)
            {
                var ring = _activeRingIndicator.ToArray()[i];
                ring.CheckRemainTime();
            }
        }

        /// <summary>
        ///     ビートの購買を解除する
        /// </summary>
        private void UnregisterOnJustBeat()
        {
            if (_musicEngineHelper)
            {
                _musicEngineHelper.OnJustChangedBeat -= OnJustBeat;
            }
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
                                manager.OnInit(_player, this);
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
                        manager => { if (Application.isPlaying) Destroy(manager); },
                        defaultCapacity: data.DefaultCapacity,
                        maxSize: 10));
            }
        }
    }
}