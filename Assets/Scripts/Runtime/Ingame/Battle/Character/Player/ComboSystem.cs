using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///    プレイヤーのコンボシステムを管理するクラス
    /// </summary>
    public class ComboSystem
    {
        public ComboSystem(PlayerData data) => _data = data;

        public ReadOnlyReactiveProperty<int> ComboCount => _comboCount;

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

        private readonly PlayerData _data;
        private ReactiveProperty<int> _comboCount = new();
        private float _lastAttackTime = Time.time;
    }
}
