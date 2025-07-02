using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class PlayerAnimeManager : CharacterAnimeManagerB
    {
        public PlayerAnimeManager(Animator animator) : base(animator) { }

        public void Avoid() => _animator?.SetTrigger(_avoid);

        public void MoveVector(Vector2 direction)
        {
            _animator?.SetFloat(_moveX, direction.x);
            _animator?.SetFloat(_moveZ, direction.y);
        }

        public void Hit() => _animator?.SetTrigger(_hit);

        public void Shoot() => _animator?.SetTrigger(_shoot);
        public void Combo(int count) => _animator?.SetInteger(_combo, count);

        public void Skill() => _animator?.SetTrigger(_skill);

        private readonly int _moveX = Animator.StringToHash("MoveX");
        private readonly int _moveZ = Animator.StringToHash("MoveZ");

        private readonly int _avoid = Animator.StringToHash("Avoid");
        private readonly int _hit = Animator.StringToHash("Hit");

        private readonly int _shoot = Animator.StringToHash("Shoot");
        private readonly int _combo = Animator.StringToHash("Combo");

        private readonly int _skill = Animator.StringToHash("Skill");
    }
}