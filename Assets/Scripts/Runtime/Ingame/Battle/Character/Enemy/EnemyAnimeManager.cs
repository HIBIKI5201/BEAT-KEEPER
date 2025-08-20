using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     敵のアニメーション管理システム
    /// </summary>
    public class EnemyAnimeManager : CharacterAnimeManagerB
    {
        public EnemyAnimeManager(Animator animator) : base(animator) { }

        public void KnockBack(int value = 0) => _animator?.SetTrigger(_knockBackHash);
        public void Attack() => _animator?.SetTrigger(_attackHash);
        public void PreAttack() => _animator?.SetTrigger(_preAttackHash);

        private readonly int _knockBackHash = Animator.StringToHash("KnockBack");
        private readonly int _attackHash = Animator.StringToHash("Attack");
        private readonly int _preAttackHash = Animator.StringToHash("PreAttack");
    }
}