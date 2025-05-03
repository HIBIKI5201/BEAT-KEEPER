using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクターのアニメーション制御を請け負う
    /// </summary>
    public abstract class CharacterAnimeManagerB
    {
        protected Animator _animator;
        
        public CharacterAnimeManagerB(Animator animator)
        {
            _animator = animator;
        }
    }
}
