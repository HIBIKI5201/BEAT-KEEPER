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

            // 次の敵をアクティブに設定
            SetActiveEnemy(nextIndex);
            // イベントを発火
            OnNextEnemyActive?.Invoke(_enemies[nextIndex]);

            _activeEnemyIndex = nextIndex;
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

            // 最初の敵をアクティブにする
            _enemies.First()?.SetActive();
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
