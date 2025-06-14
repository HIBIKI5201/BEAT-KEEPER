using Unity.VisualScripting;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     敵のアニメーション管理システム
    /// </summary>
    public class EnemyAnimeManager : CharacterAnimeManagerB
    {
        private readonly int _knockBackHash = Animator.StringToHash("KnockBack");
        
        public EnemyAnimeManager(Animator animator) : base(animator) { }

        public void KnockBack(int value = 0) => _animator?.SetTrigger(_knockBackHash);
    }
}