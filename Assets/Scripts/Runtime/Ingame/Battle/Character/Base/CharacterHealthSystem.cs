using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     体力システム
    /// </summary>
    public class CharacterHealthSystem
    {
        private readonly float _maxHealth;

        public float Health => _health;
        private float _health;

        public event Action<float> OnHealthChanged;

        public CharacterHealthSystem(float maxHealth)
        {
            _maxHealth = maxHealth;
            _health = maxHealth;
        }

        /// <summary>
        ///     体力を変更する
        /// </summary>
        /// <param name="value">変更量</param>
        /// <returns>死亡したかどうか</returns>
        public bool HealthChange(float value)
        {
            _health = Mathf.Clamp(value + _health, 0, _maxHealth);

            OnHealthChanged?.Invoke(_health);

            //体力が残っているか
            if (_health <= 0)
            {
                return true;
            }

            return false;
        }
    }
}