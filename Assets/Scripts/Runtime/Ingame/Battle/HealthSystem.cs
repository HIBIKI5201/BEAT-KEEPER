using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     体力システム
    /// </summary>
    public class HealthSystem
    {
        private CharacterData _data;

        public float Health => _health;
        private float _health;
        public Action<float> OnHealthChanged;
        
        public HealthSystem(CharacterData data)
        {
            _data = data;
            _health = data.MaxHealth;
        }

        /// <summary>
        ///     体力を変更する
        /// </summary>
        /// <param name="value">変更量</param>
        /// <returns>死亡したかどうか</returns>
        public bool HealthChange(float value)
        {
            _health += value;

            //体力が残っているか
            if (_health <= 0)
            {
                OnHealthChanged?.Invoke(0);
                return true;
            }

            //体力が上限値を超えていたら直す
            if (_data.MaxHealth < _health)
            {
                _health = _data.MaxHealth;
            }
            
            OnHealthChanged?.Invoke(_health);
            return false;
        }
    }
}
