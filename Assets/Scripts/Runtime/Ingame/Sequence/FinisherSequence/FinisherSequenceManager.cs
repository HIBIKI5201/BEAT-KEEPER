using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceManager : MonoBehaviour
    {
        public event Action OnFinisherSequenceEnd;

        public int FinisherScore => _finisherScore;

        [SerializeField, Tooltip("フィニッシャー時の加算スコア")]
        private int _finisherScore = 2000;

        [SerializeField]
        private PlayableAsset _playableAsset;

        private PlayableDirector _playableDirector;

        private async void Start()
        {
            var movieManager = await ServiceLocator.GetInstanceAsync<MovieManager>();
            _playableDirector = movieManager.GetDirector(_playableAsset);

            if (_playableDirector)
            {
                _playableDirector.stopped += OnPlayableDirectorStopped;
            }

            FinisherEventRegister();
        }

        private void OnDestroy()
        {
            if (_playableDirector)
            {
                _playableDirector.stopped -= OnPlayableDirectorStopped;
            }
        }

        /// <summary>
        ///     Finisher時のイベントを購買する
        /// </summary>
        public async void FinisherEventRegister()
        {
            PlayerManager playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            if (playerManager == null)
            {
                Debug.LogWarning("PlayerManager is not found.");
                return;
            }

            playerManager.OnFinisher += Finisher;
        }

        /// <summary>
        ///     Finisher入力を受け取った際の処理
        /// </summary>
        /// <param name="context"></param>
        private void Finisher()
        {
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
