using UnityEngine;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;
using UnityEngine.InputSystem;

namespace BeatKeeper
{
    /// <summary>
    /// チュートリアルスキップを管理するクラス
    /// </summary>
    public class SkipTutorialController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;

        [SerializeField, Tooltip("スキップする場合のホールド時間")]
        private float _holdDuration = 3f;

        private InputBuffer _inputBuffer;
        private bool _isSkipping = false; // 現在スキップ入力中かどうか
        private float _skipHoldTime = 0f; // 長押し時間の記録
        
        /// <summary>
        /// スキップ時間
        /// NOTE: TutorialManagerと同じ計算
        /// </summary>
        private double JumpTime => _director.playableAsset.duration* 0.95f;

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            _inputBuffer = await ServiceLocator.GetInstanceAsync<InputBuffer>();

            if (_director == null)
            {
                _director = GetComponent<PlayableDirector>();
            }

            // スキップのインプットアクションを登録する
            RegisterSkipInput();
        }

        /// <summary>
        /// Update
        /// </summary>
        private void Update()
        {
            // タイムラインアセットがnull
            // タイムラインの再生が終了している
            // チュートリアル再生中ではなかったらスキップ処理は行いたくないので早期return
            if (_director == null || _director.time >= JumpTime || !IsPlayingTutorial())
            {
                return;
            }

            if (_isSkipping)
            {
                // 時間の計測に経過時間を足していく
                _skipHoldTime += Time.unscaledDeltaTime;

                if (_skipHoldTime >= _holdDuration)
                {
                    // 長押し時間がしきい値を越えたらスキップ
                    // メソッドの呼び出しと変数のリセットを行う
                    SkipStart();

                    _isSkipping = false;
                    _skipHoldTime = 0f;
                }
            }
        }

        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            UnregisterSkipInput();
        }

        #endregion


        /// <summary>
        /// チュートリアルスキップ
        /// </summary>
        private void SkipStart()
        {
            if (_director == null)
            {
                Debug.LogError($"PlayableDirectorがnullのためスキップできません: {typeof(SkipTutorialController)}");
                return;
            }

            // タイムラインの時間を飛ばす
            _director.time = JumpTime;
            _director.Evaluate();
        }

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

        /// <summary>
        /// チュートリアルがプレイ中か
        /// </summary>
        private bool IsPlayingTutorial()
        {
            if (_director == null)
            {
                return false;
            }

            return _director.time > 0;
        }
    }
}
