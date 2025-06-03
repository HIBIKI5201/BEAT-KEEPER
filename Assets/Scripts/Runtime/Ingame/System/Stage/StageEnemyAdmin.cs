using System;
using System.Linq;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    public class StageEnemyAdmin : MonoBehaviour
    {
        private EnemyManager[] _enemies;
        public EnemyManager[] Enemies => _enemies;

        private int _activeEnemyIndex;

        private void Awake()
        {
            _enemies = GetComponentsInChildren<EnemyManager>();
        }

        private async void Start()
        {
            var ui = ServiceLocator.GetInstance<InGameUIManager>();

            Array.ForEach(_enemies, ui.HealthBar.RegisterEnemyEvent);
        }

        /// <summary>
        ///     最も近い敵を返す
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public EnemyManager GetActiveEnemy(Vector3 position)
        {
            return _enemies[_activeEnemyIndex];
        }

        /// <summary>
        /// /     アクティブな敵を設定する
        /// </summary>
        /// <param name="index"></param>
        public void SetActiveEnemy(int index)
        {
            if (index < 0 || index >= _enemies.Length)
            {
                Debug.LogWarning($"Index {index} is out of range for enemies.");
                return;
            }

            // 既存のアクティブな敵を非アクティブにする
            _enemies[_activeEnemyIndex].gameObject.SetActive(false);

            // 新しい敵をアクティブにする
            _activeEnemyIndex = index;
            _enemies[index].gameObject.SetActive(true);
        }
    }
}
