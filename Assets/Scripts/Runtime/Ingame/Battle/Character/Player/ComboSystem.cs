using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class ComboSystem
    {
        private PlayerData _data;
        
        public int ComboCount => _comboCount;
        private int _comboCount;
        
        private float _lastAttackTime = Time.time;

        public ComboSystem(PlayerData data) => _data = data;

        /// <summary>
        ///     攻撃時にコンボカウントを増やす
        /// </summary>
        public void Attack()
        {
            _comboCount++;
            _lastAttackTime = Time.time;
        }

        /// <summary>
        ///     コンボをリセットする
        /// </summary>
        public void ComboReset() => _comboCount = 0;

        public void Update()
        {
            //コンボ維持時間が終了するとリセット
            if (Time.time < _lastAttackTime + _data.ComboResetTime)
            {
                ComboReset();
            }
        }
    }
}
