using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.Character;
using Cysharp.Threading.Tasks;
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
        
        private PhaseManager _phaseManager;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            if (!_playableDirector)
            {
                Debug.LogWarning("PlayableDirector component is missing on FinisherSequence.");
            }
        }

        private async void Start()
        {
            var finisher = transform.parent.GetComponentInChildren<FinisherSequenceManager>();
            if (finisher)
            {
                finisher.OnFinisherSequenceEnd += OnFinisherSequenceEnd;
            }

            var battleSceneManager = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();
            StageEnemyAdmin enemyAdmin = battleSceneManager.EnemyAdmin;
            if (enemyAdmin)
            {
                enemyAdmin.OnNextEnemyActive += OnNextEnemyActive;

                EnemyManager firstEnemy = enemyAdmin.GetActiveEnemy();
                firstEnemy.HealthSystem.OnDeath += OnEnemyDeath;
            }
            
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
        }

        private void OnNextEnemyActive(EnemyManager enemy)
        {
            enemy.HealthSystem.OnDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath()
        {
            PlayBreakMovie();
        }

        /// <summary>
        ///     フィニッシャーシーケンス終了時にブレイクムービーシーケンスを開始する
        /// </summary>
        private void OnFinisherSequenceEnd()
        {
            PlayBreakMovie();
        }

        /// <summary>
        ///     ブレイクムービーを再生する
        /// </summary>
        private void PlayBreakMovie()
        {
            if (!_playableDirector) return;

            if (_phaseManager)
            {
                _phaseManager.TransitionTo(PhaseEnum.Movie);
            }
            
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
