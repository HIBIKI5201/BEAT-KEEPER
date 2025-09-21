using UnityEngine;
using UnityEngine.InputSystem;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;

namespace BeatKeeper
{
    /// <summary>
    /// ムービー/チュートリアルスキップ処理のベースクラス
    /// </summary>
    public abstract class BaseSkipController : MonoBehaviour
    {
        [Header("スキップ処理の設定")]
        [SerializeField, Tooltip("スキップする場合のホールド時間")] protected float _holdDuration = 3f;
        
        private InputBuffer _inputBuffer;
        private bool _isSkipping = false; // 現在スキップ入力中かどうか
        private float _skipHoldTime = 0f; // 長押し時間の記録

        /// <summary>
        /// スキップ処理
        /// </summary>
        protected abstract void OnSkip();
        
        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        protected virtual async void Start()
        {
            _inputBuffer = await ServiceLocator.GetInstanceAsync<InputBuffer>();
            
            // スキップのインプットアクションを登録する
            RegisterSkipInput();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected virtual void Update()
        {
            if (_isSkipping)
            {
                // 時間の計測に経過時間を足していく
                _skipHoldTime += Time.unscaledDeltaTime;
                
                if (_skipHoldTime >= _holdDuration)
                {
                    // 長押し時間がしきい値を越えたらスキップ
                    // メソッドの呼び出しと変数のリセットを行う
                    OnSkip();
                    
                    _isSkipping = false;
                    _skipHoldTime = 0f;
                }
            }
        }
        
        /// <summary>
        /// Destroy
        /// </summary>
        protected void OnDestroy()
        {
            UnregisterSkipInput();
        }

        #endregion
        
        /// <summary>
        /// スキップのインプットアクションを登録する
        /// </summary>
        private void RegisterSkipInput()
        {
            if (_inputBuffer != null && _inputBuffer.Quit != null)
            {
                _inputBuffer.Quit.started += OnSkipStarted;
                _inputBuffer.Quit.canceled += OnSkipCanceled;
            }
        }

        /// <summary>
        /// スキップのインプットアクション登録を解除する
        /// </summary>
        private void UnregisterSkipInput()
        {
            if (_inputBuffer != null && _inputBuffer.Quit != null)
            {
                _inputBuffer.Quit.started -= OnSkipStarted;
                _inputBuffer.Quit.canceled -= OnSkipCanceled;
            }
        }

        /// <summary>
        /// 長押しし始めの処理
        /// </summary>
        private void OnSkipStarted(InputAction.CallbackContext context)
        {
            _isSkipping = true;
            _skipHoldTime = 0f;
        }

        /// <summary>
        /// 長押しボタンを離した時の処理
        /// </summary>
        private void OnSkipCanceled(InputAction.CallbackContext context)
        {
            _isSkipping = false;
            _skipHoldTime = 0f;
        }
    }
}
