using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.Character;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using System;
using UnityEngine;
using UnityEngine.Playables;
using System.Threading.Tasks;
using System.Linq;

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
        private StageEnemyAdmin _enemyAdmin;

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

            _enemyAdmin = enemyAdmin;
        }

        private void OnNextEnemyActive(EnemyManager enemy)
        {
            //最後の敵でなければ登録
            //最後の敵はフィニッシャーシーケンスで登録されるため
            if (_enemyAdmin.Enemies.Last() != enemy)
            {
                EnemyDeathEventRegister(enemy);
            }
        }

        private void EnemyDeathEventRegister(EnemyManager enemy)
        {
            enemy.HealthSystem.OnDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath()
        {
            _ = PlayBreakMovie();
        }

        /// <summary>
        ///     フィニッシャーシーケンス終了時にブレイクムービーシーケンスを開始する
        /// </summary>
        private void OnFinisherSequenceEnd()
        {
            _ = PlayBreakMovie();
        }

        /// <summary>
        ///     ブレイクムービーを再生する
        /// </summary>
        private async Task PlayBreakMovie()
        {
            if (!_playableDirector) return;

            if (_phaseManager)
            {
                _phaseManager.TransitionTo(PhaseEnum.Movie);
            }
            
            StageEnemyAdmin enemyAdmin = ServiceLocator.GetInstance<BattleSceneManager>().EnemyAdmin;
            int index = enemyAdmin.ActiveEnemyIndex;

            var movieManager = await ServiceLocator.GetInstanceAsync<MovieManager>();
            var director = movieManager.GetDirector(playables[index]);
            if (director != null)
            {
                director.Play();
                director.stopped += OnPlayableDirectorStopped;
            }
            else
            {
                _playableDirector.playableAsset = playables[index];
                _playableDirector.Play();

                _playableDirector.stopped += OnPlayableDirectorStopped;
            }
        }

        /// <summary>
        ///     Timelineの再生が終了したときに呼ばれる
        /// </summary>
        /// <param name="director"></param>
        private void OnPlayableDirectorStopped(PlayableDirector director) =>
            OnBreakMovieSequenceEnd?.Invoke();
    }
}
