using UnityEngine;

namespace BeatKeeper
{
    public abstract class CharacterAnimeManagerB
    {
        protected Animator _animator;
        
        public CharacterAnimeManagerB(Animator animator)
        {
            _animator = animator;
        }
    }
}
