using System.Linq;
using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    public class StageEnemyAdmin : MonoBehaviour
    {
        private EnemyManager[] _enemies;
        public EnemyManager[] Enemies => _enemies;

        private void Awake()
        {
            _enemies = GetComponentsInChildren<EnemyManager>();
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
