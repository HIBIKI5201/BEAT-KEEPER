using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     体力システム
    /// </summary>
    [Serializable]
    public class CharacterHealthSystem
    {
        public CharacterHealthSystem(EnemyData data)
        {
            _data = data;
            _health = data.MaxHealth;
        }

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        
        public float MaxHealth => _data.MaxHealth;
        public float Health => _health;

        private EnemyData _data;
        private float _health;

        /// <summary>
        ///     体力を変更する
        /// </summary>
        /// <param name="value">変更量</param>
        /// <returns>死亡したかどうか</returns>
        public bool HealthChange(float value)
        {
            _health = Mathf.Clamp(value + _health, 0, _data.MaxHealth);

            OnHealthChanged?.Invoke(_health);

            //体力が残っていないかどうか
            if (_health <= 0)
            {
                OnDeath?.Invoke();
                return true;
            }

            return false;
        }
    }
}