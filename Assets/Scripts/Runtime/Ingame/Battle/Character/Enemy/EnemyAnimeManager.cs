using UnityEngine;

namespace BeatKeeper
{
    public class EnemyAnimeManager : CharacterAnimeManagerB
    {
        private readonly int _knockBackHash = Animator.StringToHash("KnockBack");

        public EnemyAnimeManager(Animator animator) : base(animator) { }

        public void KnockBack() => _animator.SetTrigger(_knockBackHash);
    }
}