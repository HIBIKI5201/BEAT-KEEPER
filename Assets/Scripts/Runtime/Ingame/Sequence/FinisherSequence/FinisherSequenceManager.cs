using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.Stsge;
using SymphonyFrameWork.System;
using System;
using System.Linq;
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

            var battleSceneManager = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();
            var enemyAdmin = battleSceneManager.EnemyAdmin;
            var lastEnemy = enemyAdmin.Enemies.Last();

            // 最後の敵がアクティブになったら、プレイヤーのフィニッシャーイベントを解除して、敵のフィニッシャーイベントを登録する
            enemyAdmin.OnNextEnemyActive += enemy =>
            {
                if (enemy == lastEnemy)
                {
                    EnemyFinisherEventRegister();
                }
            };

            PlayerFinisherEventRegister();
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
        private async void PlayerFinisherEventRegister()
        {
            PlayerManager playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            if (playerManager == null)
            {
                Debug.LogWarning("PlayerManager is not found.");
                return;
            }

            playerManager.OnFinisher += Finisher;
        }

        private async void EnemyFinisherEventRegister()
        {
            var battleSceneManager = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();
            StageEnemyAdmin enemyAdmin = battleSceneManager.EnemyAdmin;
            if (enemyAdmin == null)
            {
                Debug.LogWarning("StageEnemyAdmin is not found.");
                return;
            }

            EnemyManager enemy = enemyAdmin.GetActiveEnemy();
            enemy.HealthSystem.OnDeath += Finisher;
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
