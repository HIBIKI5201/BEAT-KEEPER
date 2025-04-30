using UnityEngine;

namespace BeatKeeper
{
    public class ComboSystem
    {
        public int ComboCount => _comboCount;
        private int _comboCount;
        
        private float _lastAttackTime = Time.time;

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
            //最後の攻撃から3病後にコンボリセっと
            if (Time.time < _lastAttackTime + 3)
            {
                ComboReset();
            }
        }
    }
}
