﻿using BeatKeeper.Runtime.Ingame.Character;
using CriWare;
using R3;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// フェーズ情報を元にBGMを変更するためのクラス
    /// </summary>
    public class BGMManager : MonoBehaviour
    {
        #region イベント

        /// <summary>小節が切り替わった時に発火するイベント</summary>
        public event Action OnJustChangedBar;

        /// <summary>拍が切り替わった時に発火するイベント</summary>
        public event Action OnJustChangedBeat;

        /// <summary>小節の切り替わりが近づいた時に発火するイベント</summary>
        public event Action OnNearChangedBar;

        /// <summary>拍の切り替わりが近づいた時に発火するイベント</summary>
        public event Action OnNearChangedBeat;

        #endregion

        #region リアクティブプロパティ

        public ReadOnlyReactiveProperty<bool> IsNearBarChange => _isNearBarChange;
        public ReadOnlyReactiveProperty<bool> IsNearBeatChange => _isNearBeatChange;

        #endregion


        #region BGMの管理

        /// <summary>
        ///     BGMを変更する
        /// </summary>
        /// <param name="name"></param>
        public void ChangeBGM(string name)
        {
            Music.Play(name);

            if (Music.Current.TryGetComponent<CriAtomSource>(out var source))
            {
                _atomSource = source;
                _lastJustBeat = 0;
                _lastNearBeat = 0;
                ChangeSelectLayer(0);
            }

            Debug.Log($"{nameof(BGMManager)} BGMを変更しました");
        }

        /// <summary>
        ///     BGMのレイヤーを変更する
        /// </summary>
        /// <param name="name"></param>
        public void ChangeSelectLayer(int index)
        {
            StringBuilder label = new(_selectorLabelName);

            if (index <= 4) //レイヤーを変更する
            {
                label.Append(index.ToString("0"));
            }
            else
            {
                float beat = MusicEngineHelper.GetBeatSinceStart();

                //今の範囲を計算
                int layer = 
                    (Mathf.CeilToInt(
                        (beat - 16) //イントロの分を減らす
                        % 64f //ループ分を削る
                        / 16f) //拍から節に変換
                    + 1) // 最低でも1以上になる
                    * 4 + 1; //レイヤー値の4n+1に合わせる

                //遷移先のレイヤー名を取得
                label.Append(layer.ToString("0"));
            }

            _atomSource.player.SetSelectorLabel(_selectorName, label.ToString());
            _atomSource.player.UpdateAll();
            Debug.Log($"{nameof(BGMManager)} BGMのレイヤーを{index}に変更しました。\nセレクター名 {label.ToString()}");
        }

        #endregion

        #region タイミングアクション追加

        /// <summary>
        /// 特定のタイミングで1回実行するアクションを辞書に登録
        /// </summary>
        /// <returns>タイミングイベント解除用のID</returns>>
        public Guid RegisterTimingAction(int bar, int beat, int unit, Action action)
        {
            return RegisterTimingAction(new TimingKey(bar, beat, unit), action);
        }

        /// <summary>
        /// 特定のタイミングで1回実行するアクションを辞書に登録
        /// </summary>
        /// <returns>タイミングイベント解除用のID</returns>>
        public Guid RegisterTimingAction(TimingKey timing, Action action)
        {
            var actionId = Guid.NewGuid(); // 解除のためのIDを作成
            var actionInfo = new TimingActionInfo(action, false);

            AddActionToTiming(timing, actionId, actionInfo);

            return actionId;
        }

        /// <summary>
        /// StartBarから繰り返し実行するアクションを辞書に登録
        /// </summary>
        /// <param name="barCycle">繰り返す間隔</param>
        /// <param name="repeatCount">繰り返す回数。デフォルトは最大数</param>
        /// <returns></returns>
        public Guid RegisterBarCycleAction(int barCycle, int bar, int beat, int unit, Action action,
            int repeatCount = DEFAULT_MAX_REPEAT_COUNT)
        {
            return RegisterBarCycleAction(barCycle, new TimingKey(bar, beat, unit), action, repeatCount);
        }

        /// <summary>
        /// StartBarから繰り返し実行するアクションを辞書に登録
        /// </summary>
        /// <param name="barCycle">繰り返す間隔</param>
        /// <param name="repeatCount">繰り返す回数。デフォルトは最大数</param>
        /// <returns></returns>
        public Guid RegisterBarCycleAction(int barCycle, TimingKey timing, Action action,
            int repeatCount = DEFAULT_MAX_REPEAT_COUNT)
        {
            if (barCycle <= 0)
            {
                throw new ArgumentException("barCycleは1以上の値を指定してください", nameof(barCycle));
            }

            var actionId = Guid.NewGuid(); // 解除のためのIDを作成
            var actionInfo = new TimingActionInfo(action, true);

            for (int i = 0; i < repeatCount; i++)
            {
                int targetBar = timing.Bar + (i * barCycle);
                var targetTiming = new TimingKey(targetBar, timing.Beat, timing.Unit); // 新しくタイミングを作成

                AddActionToTiming(targetTiming, actionId, actionInfo); // 辞書に追加
            }

            return actionId;
        }

        #endregion

        #region タイミングアクション削除

        /// <summary>
        /// 指定IDのタイミングアクションを削除
        /// </summary>
        public void UnregisterTimingAction(int bar, int beat, int unit, Guid actionId)
        {
            UnregisterTimingAction(new TimingKey(bar, beat, unit), actionId);
        }

        /// <summary>
        /// 指定IDのタイミングアクションを削除
        /// </summary>
        public void UnregisterTimingAction(TimingKey timing, Guid actionId)
        {
            if (_timingActions.TryGetValue(timing, out var actions))
            {
                actions.Remove(actionId);

                // アクションが空になった場合はタイミングエントリーも削除
                if (actions.Count == 0)
                {
                    _timingActions.Remove(timing);
                }
            }
        }

        /// <summary>
        /// 指定のIDの繰り返しアクションをすべて削除
        /// </summary>
        public void UnregisterRepeatAction(Guid actionId)
        {
            // 削除対象のタイミングキーを記録するリスト
            List<TimingKey> timingsToCleanup = new List<TimingKey>();

            // すべてのタイミングエントリーをチェック
            foreach (var entry in _timingActions)
            {
                var timing = entry.Key;
                var actionDict = entry.Value;

                // 指定したIDのアクションがあれば削除
                if (actionDict.Remove(actionId))
                {
                    // アクションdictionaryが空になった場合は、後でタイミングエントリー自体を削除するためにリストに追加
                    if (actionDict.Count == 0)
                    {
                        timingsToCleanup.Add(timing);
                    }
                }
            }

            // 空になったタイミングエントリーを削除
            foreach (var timing in timingsToCleanup)
            {
                _timingActions.Remove(timing);
            }
        }

        /// <summary>
        /// 指定の拍のタイミングアクションをすべて削除
        /// </summary>
        public void ClearTimingActionsAt(int bar, int beat, int unit)
        {
            ClearTimingActionsAt(new TimingKey(bar, beat, unit));
        }

        /// <summary>
        /// 指定の拍のタイミングアクションをすべて削除
        /// </summary>
        public void ClearTimingActionsAt(TimingKey timing)
        {
            _timingActions.Remove(timing);
        }

        /// <summary>
        /// 登録されたすべてのタイミングアクションをクリア
        /// </summary>
        public void ClearAllTimingActions()
        {
            _timingActions.Clear();
        }

        #endregion

        private const int DEFAULT_MAX_REPEAT_COUNT = 100; //TODO: 繰り返し回数の上限を適切な値が渡せるように修正する

        // タイミングを指定して実行するアクションのディクショナリ
        private readonly Dictionary<TimingKey, Dictionary<Guid, TimingActionInfo>> _timingActions = new();

        [SerializeField]
        private string _selectorName = "Selector_for_Battle";
        [SerializeField]
        private string _selectorLabelName = "SelectorLabel_Layer";
        [SerializeField]
        private string _selectorFlowZoneLabelName = "SelectorLabel_Flowzone_from";

        [Tooltip("小節の切り替わりタイミングが近いかどうか")]
        private readonly ReactiveProperty<bool> _isNearBarChange = new(false);
        [Tooltip("拍の切り替わりタイミングが近いかどうか")]
        private readonly ReactiveProperty<bool> _isNearBeatChange = new(false);

        private CriAtomSource _atomSource;

        /// <summary>
        /// BGMレイヤーを管理するためのFlowZoneSystemの参照
        /// </summary>
        private FlowZoneSystem _flowZoneSystem;

        private CompositeDisposable _disposable = new();

        private int _lastJustBeat;
        private int _lastNearBeat;

        private async void Start()
        {
            PlayerManager playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();

            await SymphonyTask.WaitUntil(() => playerManager.FlowZoneSystem != null);
            _flowZoneSystem = playerManager.FlowZoneSystem;

            // リズム共鳴のリアクティブプロパティを購読
            _flowZoneSystem.ResonanceCount
                .Subscribe(OnChangeResonanceCount)
                .AddTo(_disposable);
        }

        private void OnChangeResonanceCount(int value)
        {
            ChangeSelectLayer((int)GetLayerEnum(value));
        }

        /// <summary>
        /// 受け取ったValueを変換し変更するBGMレイヤーを決定する
        /// </summary>
        private BGMLayerEnum GetLayerEnum(int value)
        {
            // 共鳴カウントを最大値で正規化し、レイヤー数（0-5の6段階）にスケーリング
            //int layerIndex = value * (Enum.GetValues(typeof(BGMLayerEnum)).Length - 1) / FlowZoneSystem.MAX_COUNT;
            int layerIndex = value * 5 / FlowZoneSystem.MAX_COUNT;
            return (BGMLayerEnum)layerIndex;
        }

        #region ライフサイクル

        private void Update()
        {
            CheckAndNotifyBarChanges();
            CheckAndNotifyBeatChanges();
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        #endregion

        #region タイミング変更の検知（Update内で呼んでいるメソッド2種）

        /// <summary>
        /// 小節情報の処理
        /// </summary>
        private void CheckAndNotifyBarChanges()
        {
            // 小節の切り替わりチェック
            if (Music.IsJustChangedBar())
            {
                _isNearBarChange.Value = false;
                OnJustChangedBar?.Invoke();
                return;
            }

            // 小節の切り替わりが近いかチェック
            bool isNear = Music.IsNearChangedBar();
            if (isNear && !_isNearBarChange.Value)
            {
                _isNearBarChange.Value = true;
                OnNearChangedBar?.Invoke();
            }
        }

        /// <summary>
        /// 拍情報の処理
        /// </summary>
        private void CheckAndNotifyBeatChanges()
        {
            int just = MusicEngineHelper.GetBeatSinceStart();
            int near = MusicEngineHelper.GetBeatNearerSinceStart();


            // 拍の切り替わりチェック
            if (_lastJustBeat != just)
            {
                _lastJustBeat = just;
                _isNearBeatChange.Value = false;
                OnJustChangedBeat?.Invoke();
                ProcessTimingActions(); // タイミングアクションの実行
                return;
            }

            // 拍の切り替わりが近いかチェック
            if (_lastNearBeat != near && !_isNearBeatChange.Value)
            {
                _lastNearBeat = near;
                _isNearBeatChange.Value = true;
                OnNearChangedBeat?.Invoke();
            }
        }

        #endregion

        /// <summary>
        /// タイミングアクションを辞書に追加する
        /// </summary>
        private void AddActionToTiming(TimingKey timing, Guid actionId, TimingActionInfo actionInfo)
        {
            // キーが既に辞書に存在する場合は、そのDictionaryにactionInfoを追加し、
            // 存在しない場合は、そのactionInfoを含む新しいDictionaryを作成して辞書に追加する
            if (!_timingActions.TryAdd(timing, new Dictionary<Guid, TimingActionInfo> { [actionId] = actionInfo }))
            {
                _timingActions[timing][actionId] = actionInfo;
            }
        }

        #region タイミングアクションの実行処理

        /// <summary>
        /// 拍が切り替わるタイミングで登録されたタイミングアクションを処理
        /// </summary>
        private void ProcessTimingActions()
        {
            var currentTiming = MusicEngineHelper.GetCurrentTiming();

            if (_timingActions.TryGetValue(currentTiming, out var actionDict))
            {
                // 削除が必要なアクション（＝繰り替えさないアクション）のIDを記録するリスト
                List<Guid> actionsToRemove = new List<Guid>();

                // 登録されたアクションを全て実行する
                foreach (var actionEntry in actionDict)
                {
                    var actionId = actionEntry.Key;
                    var actionInfo = actionEntry.Value;

                    try
                    {
                        actionInfo.Action?.Invoke(); // アクション実行

                        if (!actionInfo.IsRepeating)
                        {
                            actionsToRemove.Add(actionId); // 繰り返さないアクションを削除リストに追加
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"TimingAction実行エラー [Timing:{currentTiming.ToString()}, ID:{actionId.ToString()}]: {ex}");
                        actionsToRemove.Add(actionId); // エラーが発生したアクションも削除
                    }
                }

                // 削除リストに含まれるアクションを削除
                foreach (var actionId in actionsToRemove)
                {
                    actionDict.Remove(actionId);
                }

                // アクションが空になった場合はタイミングエントリーも削除
                if (actionDict.Count == 0)
                {
                    _timingActions.Remove(currentTiming);
                }
            }
        }

        #endregion

        /// <summary>
        ///     タイミングアクションの情報を格納する構造体
        /// </summary>
        private readonly struct TimingActionInfo
        {
            /// <summary>実行するアクション</summary>
            public Action Action { get; }
            /// <summary>繰り返し実行するかどうか</summary>
            public bool IsRepeating { get; }

            public TimingActionInfo(Action action, bool isRepeating)
            {
                Action = action;
                IsRepeating = isRepeating;
            }
        }
    }
}