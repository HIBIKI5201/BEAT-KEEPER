using System;
using System.Collections.Generic;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// MusicEngineの補助クラス
    /// </summary>
    public class MusicEngineHelper : MonoBehaviour
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

        private readonly ReactiveProperty<bool> _isNearBarChange = new(false);

        /// <summary>小節の切り替わりタイミングが近いかどうか</summary>
        public ReadOnlyReactiveProperty<bool> IsNearBarChange => _isNearBarChange;

        private readonly ReactiveProperty<bool> _isNearBeatChange = new(false);

        /// <summary>拍の切り替わりタイミングが近いかどうか</summary>
        public ReadOnlyReactiveProperty<bool> IsNearBeatChange => _isNearBeatChange;

        #endregion
        
        private const int MAX_REPEAT_COUNT = 100; //TODO: 繰り返し回数の上限を適切な値が渡せるように修正する
        
        // タイミングを指定して実行するアクションのディクショナリ
        private Dictionary<(int bar, int beat, int unit), Dictionary<Guid, TimingActionInfo>> _timingActions = new();

        private void Awake()
        {
            ServiceLocator.SetInstance(this, ServiceLocator.LocateType.Singleton); // サービスロケーターに登録
        }

        private void Update()
        {
            CheckAndNotifyBarChanges();
            CheckAndNotifyBeatChanges();
        }

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
            // 拍の切り替わりチェック
            if (Music.IsJustChangedBeat())
            {
                _isNearBeatChange.Value = false;
                OnJustChangedBeat?.Invoke();
                ProcessTimingActions(); // タイミングアクションの実行
                return;
            }

            // 拍の切り替わりが近いかチェック
            bool isNear = Music.IsNearChangedBeat();
            if (isNear && !_isNearBeatChange.Value)
            {
                _isNearBeatChange.Value = true;
                OnNearChangedBeat?.Invoke();
            }
        }

        /// <summary>現在の小節数を取得する</summary>
        public int GetCurrentBarCount() => Music.Just.Bar;

        /// <summary>現在の拍数を取得する</summary>
        public int GetCurrentBeatCount() => Music.Just.Beat;

        /// <summary>現在の16分音符位置を取得する</summary>
        public int GetCurrentUnitCount() => Music.Just.Unit;

        #region タイミングアクション関連の処理・メソッド
        
        /// <summary>
        /// 特定のタイミングで1回実行するアクションを辞書に登録
        /// </summary>
        /// <returns>タイミングイベント解除用のID</returns>>
        public Guid RegisterTimingAction(int bar, int beat, int unit, Action action)
        {
            var timing = (bar, beat, unit);
            var actionId = Guid.NewGuid(); // 解除のためのIDを作成
            var actionInfo = new TimingActionInfo(action, false);
            
            // キーが既に辞書に存在する場合は、そのDictionaryにactionInfoを追加し、
            // 存在しない場合は、そのactionInfoを含む新しいDictionaryを作成して辞書に追加する
            if (!_timingActions.TryAdd(timing, new Dictionary<Guid, TimingActionInfo> { [actionId] = actionInfo }))
            {
                _timingActions[timing][actionId] = actionInfo;
            }
            
            return actionId;
        }
        
        /// <summary>
        /// 特定のタイミングで1回実行するアクションを辞書に登録
        /// </summary>
        /// <returns>タイミングイベント解除用のID</returns>>
        public Guid RegisterTimingAction((int,int,int)timing, Action action)
        {
            var actionId = Guid.NewGuid(); // 解除のためのIDを作成
            var actionInfo = new TimingActionInfo(action, false);
            
            // キーが既に辞書に存在する場合は、そのDictionaryにactionInfoを追加し、
            // 存在しない場合は、そのactionInfoを含む新しいDictionaryを作成して辞書に追加する
            if (!_timingActions.TryAdd(timing, new Dictionary<Guid, TimingActionInfo> { [actionId] = actionInfo }))
            {
                _timingActions[timing][actionId] = actionInfo;
            }
            
            return actionId;
        }

        /// <summary>
        /// StartBarから繰り返し実行するアクションを辞書に登録
        /// </summary>
        /// <param name="barCycle">繰り返す間隔</param>
        /// <param name="repeatCount">繰り返す回数。デフォルトは最大数</param>
        /// <returns></returns>
        public Guid RegisterBarCycleAction(int barCycle, int bar, int beat, int unit, Action action,
            int repeatCount = MAX_REPEAT_COUNT)
        {
            if (barCycle <= 0)
            {
                throw new ArgumentException("barCycleは1以上の値を指定してください", nameof(barCycle));
            }
            
            var actionId = Guid.NewGuid(); // 解除のためのIDを作成
            var actionInfo = new TimingActionInfo(action, true);

            for (int i = 0; i < repeatCount; i++)
            {
                int targetBar = bar + (i * barCycle);
                var timing = (targetBar, beat, unit); // 新しくタイミングを作成
                
                // キーが既に辞書に存在する場合は、そのDictionaryにactionInfoを追加し、
                // 存在しない場合は、そのactionInfoを含む新しいDictionaryを作成して辞書に追加する
                if (!_timingActions.TryAdd(timing, new Dictionary<Guid, TimingActionInfo> { [actionId] = actionInfo }))
                {
                    _timingActions[timing][actionId] = actionInfo;
                }
            }
            
            return actionId;
        }

        /// <summary>
        /// 指定IDのタイミングアクションを削除
        /// </summary>
        public void UnregisterTimingAction(int bar, int beat, int unit, Guid actionId)
        {
            var timing = (bar, beat, unit);
            
            if (_timingActions.TryGetValue(timing, out var actions))
            {
                actions.Remove(actionId);
            }
        }
        
        /// <summary>
        /// 指定IDのタイミングアクションを削除
        /// </summary>
        public void UnregisterTimingAction((int, int, int) timing, Guid actionId)
        {
            if (_timingActions.TryGetValue(timing, out var actions))
            {
                actions.Remove(actionId);
            }
        }
        
        /// <summary>
        /// 指定の拍のタイミングアクションをすべて削除
        /// </summary>
        public void ClearTimingAction(int bar, int beat, int unit)
        {
            var timing = (bar, beat, unit);
            
            if (_timingActions.TryGetValue(timing, out var actions))
            {
                actions.Clear();
            }
        }

        /// <summary>
        /// 指定の拍のタイミングアクションをすべて削除
        /// </summary>
        public void ClearTimingActions((int, int, int) timing)
        {
            if (_timingActions.TryGetValue(timing, out var actions))
            {
                actions.Clear();
            }
        }

        /// <summary>
        /// 指定のIDの繰り返しアクションをすべて削除
        /// </summary>
        public void ClearRepeatAction(Guid actionId)
        {
            // 削除対象のタイミングキーを記録するリスト
            List<(int, int, int)> timingsToCleanup = new List<(int, int, int)>();
    
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
        /// 登録されたすべてのタイミングアクションをクリア
        /// </summary>
        public void ClearAllTimingActions()
        {
            _timingActions.Clear();
        }

        /// <summary>
        /// タイミングアクションの処理（拍が切り替わるごとに処理を行う）
        /// </summary>
        private void ProcessTimingActions()
        {
            // 現在のタイミング情報をタプルに変換して取得
            (int, int, int) currentTiming = (Music.Just.Bar, Music.Just.Beat, Music.Just.Unit);
            
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
                        Debug.LogError($"TimingAction実行エラー [Timing:{currentTiming}, ID:{actionId}]: {ex}");
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
        
        private void OnDestroy()
        {   
            ServiceLocator.DestroyInstance(this);
        }
        
        /// <summary>
        /// タイミングアクションの情報を格納する構造体
        /// </summary>
        private struct TimingActionInfo
        {
            public Action Action { get; }
            public bool IsRepeating { get; }
            
            public TimingActionInfo(Action action, bool isRepeating)
            {
                Action = action;
                IsRepeating = isRepeating;
            }
        }
    }
}
    

