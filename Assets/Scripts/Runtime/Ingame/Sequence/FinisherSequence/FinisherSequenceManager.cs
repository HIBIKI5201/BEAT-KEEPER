using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    [RequireComponent(typeof(PlayableDirector))]
    public class FinisherSequenceManager : MonoBehaviour
    {
        public event Action OnFinisherSequenceEnd;

        public int FinisherScore => _finisherScore;

        [SerializeField, Tooltip("フィニッシャー時の加算スコア")]
        private int _finisherScore = 2000;

        private InputBuffer _inputBuffer;
        private EnemyManager _registeredEnemy;
        private PlayableDirector _playableDirector;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            if (!_playableDirector)
            {
                Debug.LogWarning("PlayableDirector component is missing on FinisherSequence.");
            }
        }

        private void Start()
        {
            if (_playableDirector)
            {
                _playableDirector.stopped += OnPlayableDirectorStopped;
            }

            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            
            //フェーズ変更時のイベントを登録
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnPhaseChanged)
                    .AddTo(destroyCancellationToken);
            }
        }

        private void OnDestroy()
        {
            if (_registeredEnemy != null)
                _registeredEnemy.OnFinisherable -= OnFinisherable;

            if (_inputBuffer)
                _inputBuffer.Finishier.started -= Finisher;

            if (_playableDirector)
                _playableDirector.stopped -= OnPlayableDirectorStopped;
        }

        /// <summary>
        ///     フェーズが変わった時のイベント
        /// </summary>
        /// <param name="phase"></param>
        private void OnPhaseChanged(PhaseEnum phase)
        {
            FinisherEventRegister();
        }

        /// <summary>
        ///     Finisher可能時のイベントを購買する
        /// </summary>
        public async void FinisherEventRegister()
        {
            if (_registeredEnemy) //既に登録されていたら解除
            {
                _registeredEnemy.OnFinisherable -= OnFinisherable;
            }
            
            //アクティブな敵を登録
            var battleScene = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();
            _registeredEnemy = battleScene.EnemyAdmin.GetActiveEnemy();
            _registeredEnemy.OnFinisherable += OnFinisherable;
        }

        /// <summary>
        ///     Finisher可能になったらフィニッシャー入力を受け付ける
        /// </summary>
        private void OnFinisherable()
        {
            _inputBuffer.Finishier.started += Finisher;

            var player = ServiceLocator.GetInstance<PlayerManager>();
            if (player)
            {
                player.InputUnregister(); // プレイヤーの入力を一時的に無効化
            }

            var text = GetComponentInChildren<Text>();
            if (text)
                text.color = Color.white; // テキストの色を白に変更
        }

        /// <summary>
        ///     Finisher入力を受け取った際の処理
        /// </summary>
        /// <param name="context"></param>
        private void Finisher(InputAction.CallbackContext context)
        {
            Debug.Log("Finisher Sequence Start");

            _inputBuffer.Finishier.started -= Finisher;

            _playableDirector.Play();
        }

        /// <summary>
        ///     Timelineの再生が停止した際の処理
        /// </summary>
        /// <param name="director"></param>
        private void OnPlayableDirectorStopped(PlayableDirector director) =>
            OnFinisherSequenceEnd?.Invoke();
    }
}
