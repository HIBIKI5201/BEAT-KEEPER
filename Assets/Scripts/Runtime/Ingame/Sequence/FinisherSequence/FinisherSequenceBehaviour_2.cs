using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Stsge;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceBehaviour_2 : PlayableBehaviour
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
                scoreManager.AddScore(2000);
            }
        }
    }
}
