using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_HealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject _healthBarPrefab;

        private int _count = 0;

        public void RegisterEnemyEvent(EnemyManager enemy)
        {
            // ヘルスバーのUIを生成
            var healthBar = Instantiate(_healthBarPrefab, transform);
            healthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -_count * 30 + 30);
            var bar = healthBar.transform.GetChild(0).GetComponent<Image>();

            // ヘルスバーのイベントを登録
            enemy.HealthSystem.OnHealthChanged += h => OnEnemyHealthChange(h, bar, enemy);

            _count++;

        }

        //ヘルスバーの更新
        void OnEnemyHealthChange(float health, Image bar, EnemyManager enemy)
        {
            bar.fillAmount = health / enemy.HealthSystem.MaxHealth;
        }
    }
}