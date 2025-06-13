using System;
using System.Linq;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.UI;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    public class StageEnemyAdmin : MonoBehaviour
    {
        private EnemyManager[] _enemies;
        public EnemyManager[] Enemies => _enemies;

        private int _activeEnemyIndex; //最初の敵を出すために-1から始める

        private void Awake()
        {
            _enemies = GetComponentsInChildren<EnemyManager>();
        }

        private void Start()
        {
            var ui = ServiceLocator.GetInstance<InGameUIManager>();

            Array.ForEach(_enemies, ui.HealthBarInitialize); //ヘルスバーを初期化
            
            // 最初の敵をアクティブにする
            _enemies.First()?.SetActive();
        }

        /// <summary>
        ///     最も近い敵を返す
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public EnemyManager GetActiveEnemy()
        {
            return _enemies[_activeEnemyIndex];
        }

        /// <summary>
        ///     次の敵をアクティブ化する
        /// </summary>
        public void NextEnemyActive()
        {
            // 次の敵のインデックスを計算
            int nextIndex = (_activeEnemyIndex + 1);
            
            if (nextIndex >= _enemies.Length) return;

            // 次の敵をアクティブに設定
            SetActiveEnemy(nextIndex);
            
            _activeEnemyIndex = nextIndex;
        }

        /// <summary>
        ///     アクティブな敵を設定する
        /// </summary>
        /// <param name="index"></param>
        private void SetActiveEnemy(int index)
        {
            if (index < 0 || index >= _enemies.Length)
            {
                Debug.LogWarning($"Index {index} is out of range for enemies.");
                return;
            }

            // 既存のアクティブな敵を非アクティブにする
            Destroy(_enemies[_activeEnemyIndex].gameObject);

            // 新しい敵をアクティブにする
            _activeEnemyIndex = index;
            _enemies[index].SetActive();
        }
    }
}
