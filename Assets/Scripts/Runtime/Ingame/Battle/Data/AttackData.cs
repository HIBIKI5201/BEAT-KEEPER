namespace BeatKeeper.Runtime.Ingame.Battle
{
    /// <summary>
    /// 　　攻撃時に渡されるデータ
    /// </summary>
    public struct AttackData
    {
        public AttackData(float damage, bool nockback = false)
        {
            _damage = damage;
            _isNockback = nockback;
        }

        public float Damage => _damage;
        public bool IsNockback => _isNockback;

        private float _damage;
        private bool _isNockback;
    }
}
