using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public class StageEnemyAdmin
    {
        public EnemyManager[] Enemies => _enemies;
        private EnemyManager[] _enemies;
        
        public StageEnemyAdmin(Transform parent)
        {
            _enemies = parent.GetComponentsInChildren<EnemyManager>();
        }
    }
}
