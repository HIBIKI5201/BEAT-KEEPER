using BeatKeeper.Runtime.Ingame.Stage;
using BeatKeeper.Runtime.Ingame.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    public class BattleSceneManager : SceneManagerB
    {
        public StageEnemyAdmin EnemyAdmin => _enemyAdmin;
        private StageEnemyAdmin _enemyAdmin;
        
        [SerializeField]
        private Transform _enemiesParent;

        private void Awake()
        {
            if (_enemiesParent)
            {
                _enemyAdmin = new (_enemiesParent);
            }
            else
            {
                Debug.LogWarning("EnemiesParent is null");
            }
        }
    }
}
