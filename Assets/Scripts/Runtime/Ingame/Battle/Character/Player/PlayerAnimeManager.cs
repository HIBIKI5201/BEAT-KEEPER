using BeatKeeper.Runtime.Ingame.Character;
using TMPro;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class PlayerAnimeManager : CharacterAnimeManagerB
    {
        private readonly int _moveX = Animator.StringToHash("MoveX");
        private readonly int _moveZ = Animator.StringToHash("MoveZ");
        
        private readonly int _avoid = Animator.StringToHash("Avoid");
        
        public PlayerAnimeManager(Animator animator) : base(animator) { }
        
        public void Avoid() => _animator.SetTrigger(_avoid);

        public void MoveVector(Vector2 direction)
        {
            _animator.SetFloat(_moveX, direction.x);
            _animator.SetFloat(_moveZ, direction.y);
        }
    }
}