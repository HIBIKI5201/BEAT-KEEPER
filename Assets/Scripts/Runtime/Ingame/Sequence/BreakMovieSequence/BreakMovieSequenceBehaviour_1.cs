using BeatKeeper.Runtime.Ingame.Battle;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class BreakMovieSequenceBehaviour_1 : PlayableBehaviour
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            //次の敵をアクティブ化する
            var enemyAdmin = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
            if (enemyAdmin)
            {
                enemyAdmin.NextEnemyActive();
            }
            
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.NextPhase();
            }
        }
    }
}
