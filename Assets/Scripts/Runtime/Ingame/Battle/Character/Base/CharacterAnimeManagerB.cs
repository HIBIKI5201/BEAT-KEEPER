using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクターのアニメーション制御を請け負う
    /// </summary>
    public abstract class CharacterAnimeManagerB
    {
        public CharacterAnimeManagerB(Animator animator)
        {
            _animator = animator;
        }
        
        protected Animator _animator;
    }
}
