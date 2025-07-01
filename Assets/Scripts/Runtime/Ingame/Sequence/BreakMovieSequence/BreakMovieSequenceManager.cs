using BeatKeeper.Runtime.Ingame.Battle;
using SymphonyFrameWork.System;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    [RequireComponent(typeof(PlayableDirector))]
    public class BreakMovieSequenceManager : MonoBehaviour
    {
        public event Action OnBreakMovieSequenceEnd;

        [SerializeField]
        private PlayableAsset[] playables;

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
            var finisher = transform.parent.GetComponentInChildren<FinisherSequenceManager>();
            if (finisher)
            {
                finisher.OnFinisherSequenceEnd += OnFinisherSequenceEnd;
            }
        }

        /// <summary>
        ///     フィニッシャーシーケンス終了時にブレイクムービーシーケンスを開始する
        /// </summary>
        private void OnFinisherSequenceEnd()
        {
            if (!_playableDirector) return;

            StageEnemyAdmin enemyAdmin = ServiceLocator.GetInstance<BattleSceneManager>().EnemyAdmin;
            int index = enemyAdmin.ActiveEnemyIndex;

            _playableDirector.playableAsset = playables[index];
            _playableDirector.Play();

            _playableDirector.stopped += OnPlayableDirectorStopped;
        }

        /// <summary>
        ///     Timelineの再生が終了したときに呼ばれる
        /// </summary>
        /// <param name="director"></param>
        private void OnPlayableDirectorStopped(PlayableDirector director) =>
            OnBreakMovieSequenceEnd?.Invoke();
    }
}
