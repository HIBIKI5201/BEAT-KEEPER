using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_HealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject _healthBarPrefab;

        [SerializeField, ReadOnly] private Image[] _bars;
        private EnemyManager[] _enemies;

        private void Start()
        {
            ServiceLocator.RegisterAfterLocate<BattleSceneManager>(RegisterEnemyEvent);
        }

        private async void RegisterEnemyEvent()
        {
            var manager = ServiceLocator.GetInstance<BattleSceneManager>();
            if (manager)
            {
                _enemies = manager.EnemyAdmin.Enemies;
                _bars = new Image[_enemies.Length]; 

                for (int i = 0; i < _enemies.Length; i++)
                {
                    int index = i; // クロージャー対策
                    await SymphonyTask.WaitUntil(() => _enemies[i].HealthSystem != null);

                    // ヘルスバーのイベントを登録
                    _enemies[i].HealthSystem.OnHealthChanged += h => OnEnemyHealthChange(h, index);

                    // ヘルスバーのUIを生成
                    var healthBar = Instantiate(_healthBarPrefab, transform);
                    healthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * 30 + 30);
                    _bars[i] = healthBar.transform.GetChild(0).GetComponent<Image>();
                }
            }
            else Debug.LogWarning("manager is null");
        }

        //ヘルスバーの更新
        void OnEnemyHealthChange(float health, int index)
        {
            _bars[index].fillAmount = health / _enemies[index].HealthSystem.MaxHealth;
        }
    }
}