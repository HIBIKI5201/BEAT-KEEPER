using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.UI;
using SymphonyFrameWork.System;
using System;
using System.Linq;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    /// <summary>
    ///     ステージ内の敵を管理するクラス
    /// </summary>
    public class StageEnemyAdmin : MonoBehaviour
    {
        public event Action<EnemyManager> OnNextEnemyActive;

        public EnemyManager[] Enemies => _enemies;
        public int ActiveEnemyIndex => _activeEnemyIndex;

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
            
            _activeEnemyIndex = nextIndex;

            // 次の敵をアクティブに設定
            SetActiveEnemy(nextIndex);
            // イベントを発火
            OnNextEnemyActive?.Invoke(_enemies[nextIndex]);
        }

        /// <summary>
        ///     アクティブな敵を設定する
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
            _enemies[_activeEnemyIndex].SetDiactive();

            // 新しい敵をアクティブにする
            _activeEnemyIndex = index;
            _enemies[index].SetActive();
        }

        private EnemyManager[] _enemies;

        private int _activeEnemyIndex; //最初の敵を出すために-1から始める

        private void Awake()
        {
            _enemies = GetComponentsInChildren<EnemyManager>();
        }

        private async void Start()
        {
            var ui = await ServiceLocator.GetInstanceAsync<InGameUIManager>();

            Array.ForEach(_enemies, ui.HealthBarInitialize); //ヘルスバーを初期化
        }
    }
}
