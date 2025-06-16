using BeatKeeper.Runtime.Ingame.Battle;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceBehaviour_2 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var enemy = ServiceLocator.GetInstance<BattleSceneManager>()
                .EnemyAdmin.GetActiveEnemy();

            if (enemy)
            {
                var remainHealth = enemy.HealthSystem.Health;
                enemy.HitAttack(new(remainHealth));

                var scoreManager = ServiceLocator.GetInstance<ScoreManager>();
                if (scoreManager)
                {
                    if (_owner.TryGetComponent<FinisherSequenceManager>(out var manager))
                        scoreManager.AddScore(manager.FinisherScore);
                }
            }
        }
    }
}
