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
        
        private readonly int _knockBackHash = Animator.StringToHash("KnockBack");

    }
}