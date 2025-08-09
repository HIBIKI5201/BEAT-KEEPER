using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     Playerのアニメーション制御を請け負う
    /// </summary>
    public class PlayerAnimeManager : CharacterAnimeManagerB
    {
        public PlayerAnimeManager(Animator animator) : base(animator) { }

        public void Avoid()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_avoid);
        }

        public void MoveVector(Vector2 direction)
        {
            if (_animator == null) return;
            _animator.SetFloat(_moveX, direction.x);
            _animator.SetFloat(_moveZ, direction.y);
        }

        public void SetAnimatorSpeed(float speed)
        {
            if (_animator == null) return;
            Debug.Log($"Player SetAnimatorSpeed: {speed}");
            _animator.speed = speed;
        }

        public void Hit()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_hit);
        }

        public void Shoot()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_shoot);
        }

        public void Combo(int count)
        {
            if (_animator == null) return;
            _animator.SetInteger(_combo, count);
        }

        public void ChargeShoot()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_chargeShoot);
        }

        public void Skill()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_skill);
        }

        private readonly int _moveX = Animator.StringToHash("MoveX");
        private readonly int _moveZ = Animator.StringToHash("MoveZ");

        private readonly int _avoid = Animator.StringToHash("Avoid");
        private readonly int _hit = Animator.StringToHash("Hit");

        private readonly int _shoot = Animator.StringToHash("Shoot");
        private readonly int _combo = Animator.StringToHash("Combo");
        private readonly int _chargeShoot = Animator.StringToHash("ChargeShoot");

        private readonly int _skill = Animator.StringToHash("Skill");
    }
}