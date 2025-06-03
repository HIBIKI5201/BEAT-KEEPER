using BeatKeeper.Runtime.Ingame.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    public class BattleSceneManager : SceneManagerB
    {
        [SerializeField]
        private StageEnemyAdmin _enemyAdmin;
        public StageEnemyAdmin EnemyAdmin => _enemyAdmin;
    }
}