using System.Linq;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _bar;
        private CharacterHealthSystem _enemyHealthSystem;

        private void Start()
        {
            if (!_bar) Debug.LogWarning("bar is null");

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

        private void OnEnemyHealthChange(float health)
        {
            SetBarAmount(health / _enemyHealthSystem.MaxHealth);
        }

        private void SetBarAmount(float amount)
        {
            if (_bar) _bar.fillAmount = amount;
        }
    }
}