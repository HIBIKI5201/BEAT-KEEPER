using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartSequenceManager : MonoBehaviour
    {
        [SerializeField] TutorialManager _tutorialManager;
        [SerializeField] PlayableAsset _playableAsset;
        
        [Header("スキップ処理")]
        [SerializeField] private float _skipTiming;
        
        [SerializeField] private string _bgmName = "Phase1";

        [SerializeField, Tooltip("スキップする場合のホールド時間")] private float _holdDuration = 3f;
        
        private PlayableDirector _director;
        
        private InputBuffer _inputBuffer;

        private bool _isSkipping = false; // 現在スキップ入力中かどうか
        private float _skipHoldTime = 0f; // 長押し時間の記録
        
        private async void Start()
        {
            var multiSceneManager = ServiceLocator.GetInstance<MultiSceneManager>();
            if (multiSceneManager)
            {
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Stage);
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Battle);
            }

            var movieManager = await ServiceLocator.GetInstanceAsync<MovieManager>();
            var director = movieManager.GetDirector(_playableAsset);
            var phaseManager = await ServiceLocator.GetInstanceAsync<PhaseManager>();
            _inputBuffer = await ServiceLocator.GetInstanceAsync<InputBuffer>();
            
            //スタートシーケンスの再生終了時にチュートリアルシーケンスを再生する
            director.stopped += (_) =>
            {
                ServiceLocator.GetInstance<BattleSceneManager>()
                    .EnemyAdmin.SetActiveEnemy(0);
                _tutorialManager.StartTutorial();
                phaseManager.TransitionTo(PhaseEnum.Tutorial);
            };
            director.Play();
            //現在最初にBGMが再生されていないのでコメントアウト
            //director.playableGraph.GetRootPlayable(0).SetSpeed((float)Music.CurrentTempo / 60);

            SaveDirector(director);

            // スキップのインプットアクションを登録する
            RegisterSkipInput();
        }

        private void Update()
        {
            // タイムラインアセットがnullもしくはタイムラインの再生が終了していたらスキップ処理は行いたくないので早期return
            if (_director == null || _director.time >= _skipTiming)
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

        private void OnDestroy()
        {
            UnregisterSkipInput();
        }
        
        [ContextMenu(nameof(SkipStart))]
        private void SkipStart()
        {
            if (_director == null)
            {
                Debug.LogError("PlayableDirector is not set. Please run the scene in the editor to set it.");
                return;
            }
            
            if (!Music.IsPlaying)
            {
                // BGMが再生されていなかったらBGMを再生
                var bgmManager = ServiceLocator.GetInstance<BGMManager>();
                if (bgmManager)
                {
                    bgmManager.ChangeBGM(_bgmName);
                }
            }
            
            // タイムラインの時間をとばす
            _director.time = _skipTiming;
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
        
        [Conditional("UNITY_EDITOR")]
        private void SaveDirector(PlayableDirector director)
        {
            _director = director;
        }
    }
}
