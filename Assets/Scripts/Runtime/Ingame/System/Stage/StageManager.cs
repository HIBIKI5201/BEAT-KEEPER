using System;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Stsge
{
    public class StageManager : MonoBehaviour
    {
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
