using System;
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
    }
}
