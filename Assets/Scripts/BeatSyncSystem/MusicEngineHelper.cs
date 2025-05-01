using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// MusicEngineの補助クラス
    /// </summary>
    public class MusicEngineHelper : MonoBehaviour
    {
        public static MusicEngineHelper Instance { get; private set; }

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

        #region プロパティ

        private readonly ReactiveProperty<bool> _isNearChangedBarTime = new(false);

        /// <summary>小節の切り替わりタイミングが近いかどうか</summary>
        public ReadOnlyReactiveProperty<bool> IsNearChangedBarTime => _isNearChangedBarTime;

        private readonly ReactiveProperty<bool> _isNearChangedBeatTime = new(false);

        /// <summary>拍の切り替わりタイミングが近いかどうか</summary>
        public ReadOnlyReactiveProperty<bool> IsNearChangedBeatTime => _isNearChangedBeatTime;

        #endregion

        // タイミングを指定して実行するアクションのディクショナリ
        private Dictionary<(int bar, int beat, int unit), Action> _timingActions = new();


        private void Awake()
        {
            // シングルトンパターンを採用
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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
                _isNearChangedBarTime.Value = false;
                OnJustChangedBar?.Invoke();
                return;
            }

            // 小節の切り替わりが近いかチェック
            bool isNear = Music.IsNearChangedBar();
            if (isNear && !_isNearChangedBarTime.Value)
            {
                _isNearChangedBarTime.Value = true;
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
                _isNearChangedBeatTime.Value = false;
                OnJustChangedBeat?.Invoke();
                ProcessTimingActions(); // タイミングアクションの実行
                return;
            }

            // 拍の切り替わりが近いかチェック
            bool isNear = Music.IsNearChangedBeat();
            if (isNear && !_isNearChangedBeatTime.Value)
            {
                _isNearChangedBeatTime.Value = true;
                OnNearChangedBeat?.Invoke();
            }
        }

        /// <summary>現在の小節数を取得する</summary>
        public int GetCurrentBarCount() => Music.Just.Bar;

        /// <summary>現在の拍数を取得する</summary>
        public int GetCurrentBeatCount() => Music.Just.Beat;

        /// <summary>現在の16分音符位置を取得する</summary>
        public int GetCurrentUnitCount() => Music.Just.Unit;

        /// <summary>
        /// 特定のタイミングで実行するアクションを辞書に登録
        /// </summary>
        public void RegisterTimingAction(int bar, int beat, int unit, Action action)
        {
            _timingActions.Add((bar, beat, unit), action);
        }

        /// <summary>
        /// 指定IDのタイミングアクションを削除
        /// </summary>
        public void UnregisterTimingAction(int bar, int beat, int unit)
        {
            _timingActions.Remove((bar, beat, unit));
        }
        
        /// <summary>
        /// 指定IDのタイミングアクションを削除
        /// </summary>
        public void UnregisterTimingAction((int, int, int) timing)
        {
            _timingActions.Remove(timing);
        }

        /// <summary>
        /// すべてのタイミングアクションをクリア
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
            
            if (_timingActions.ContainsKey(currentTiming))
            {
                try
                {
                    _timingActions[currentTiming]?.Invoke(); // 辞書に登録されているアクションを実行
                }
                catch (Exception ex)
                {
                    Debug.LogError($"TimingAction実行エラー [ID:{currentTiming}]: {ex}");
                }
                
                UnregisterTimingAction(currentTiming); // アクションを削除
            }
        }
    }
}
    

