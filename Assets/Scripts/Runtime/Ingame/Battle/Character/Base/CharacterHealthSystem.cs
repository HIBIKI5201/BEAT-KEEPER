using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     体力システム
    /// </summary>
    public class CharacterHealthSystem
    {
        private CharacterData _data;

        public float Health => _health;
        private float _health;
        
        public Action<float> OnHealthChanged;
        
        public CharacterHealthSystem(CharacterData data)
        {
            if (data)
            {
                _data = data;
                _health = data.MaxHealth;
            }
            else
            {
                Debug.LogWarning("データがありません");
            }
        }

        /// <summary>
        ///     体力を変更する
        /// </summary>
        /// <param name="value">変更量</param>
        /// <returns>死亡したかどうか</returns>
        public bool HealthChange(float value)
        {
            _health = Mathf.Clamp(value + _health, 0, _data.MaxHealth);

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
