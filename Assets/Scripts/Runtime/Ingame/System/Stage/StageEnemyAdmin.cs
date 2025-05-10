using System.Linq;
using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Stsge
{
    public class StageEnemyAdmin
    {
        public EnemyManager[] Enemies => _enemies;
        private EnemyManager[] _enemies;
        
        public StageEnemyAdmin(Transform parent)
        {
            _enemies = parent.GetComponentsInChildren<EnemyManager>();
        }

        /// <summary>
        ///     最も近い敵を返す
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public EnemyManager FindClosestEnemy(Vector3 position)
        {
            return _enemies
                .OrderBy(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault();
        }
    }
}
