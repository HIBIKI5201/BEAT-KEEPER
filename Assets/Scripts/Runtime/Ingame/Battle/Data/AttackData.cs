using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    public struct AttackData
    {
        public AttackData(float damage, bool nockback = false)
        {
            _damage = damage;
            _isNockback = nockback;
        }

        private float _damage;
        public float Damage => _damage;


        private bool _isNockback;
        public bool IsNockback => _isNockback;
    }
}
