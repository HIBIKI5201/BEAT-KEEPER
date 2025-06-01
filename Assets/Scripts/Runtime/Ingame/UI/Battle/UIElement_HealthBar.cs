using System.Linq;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_HealthBar : MonoBehaviour
    {
        [SerializeField] private Image[] _bars;

        private CharacterHealthSystem _enemyHealthSystem;

        private void Start()
        {
            if (_bars == null || _bars.Length < 1) Debug.LogWarning("bar is null");

            ServiceLocator.RegisterAfterLocate<BattleSceneManager>(RegisterEnemyEvent);

            async void RegisterEnemyEvent()
            {
                var manager = ServiceLocator.GetInstance<BattleSceneManager>();
                if (manager)
                {
                    var enemy = manager.EnemyAdmin.Enemies.First();
                    
                    await SymphonyTask.WaitUntil(() => enemy.HealthSystem != null);

                    _enemyHealthSystem = enemy.HealthSystem;
                    _enemyHealthSystem.OnHealthChanged += OnEnemyHealthChange;
                }
                else Debug.LogWarning("manager is null");
            }
        }

        /// <summary>
        ///     敵のヘルス変更時にバーを更新する
        /// </summary>
        /// <param name="health"></param>
        private void OnEnemyHealthChange(float health)
        {
            SetBarAmount(health / _enemyHealthSystem.MaxHealth);
        }

        /// <summary>
        ///     ヘルスバーを更新
        /// </summary>
        /// <param name="amount"></param>
        private void SetBarAmount(float amount)
        {
            float segment = 1f / 3f;
            
            float remain = Mathf.Clamp01(amount);

            for (int i = 0; i < _bars.Length; i++)
            {
                float fill = Mathf.Min(remain, segment);
                _bars[i].fillAmount = fill / segment;
                remain -= fill;
            }
        }
    }
}