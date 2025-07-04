﻿using BeatKeeper.Runtime.Ingame.Character;
using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class ComboSystem
    {
        public ComboSystem(PlayerData data) => _data = data;

        public ReadOnlyReactiveProperty<int> ComboCount => _comboCount;

        
        private readonly PlayerData _data;
        private ReactiveProperty<int> _comboCount = new();
        private float _lastAttackTime = Time.time;
        


        /// <summary>
        ///     攻撃時にコンボカウントを増やす
        /// </summary>
        public void Attack()
        {
            _comboCount.Value++;
            _lastAttackTime = Time.time;
        }

        /// <summary>
        ///     コンボをリセットする
        /// </summary>
        public void ComboReset() => _comboCount.Value = 0;

        public void Update()
        {
            //コンボ維持時間が終了するとリセット
            if (Time.time > _lastAttackTime + _data.ComboResetTime)
            {
                ComboReset();
            }
        }
    }
}
