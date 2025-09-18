using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     Playerのアニメーション制御を請け負う
    /// </summary>
    public class PlayerAnimeManager : CharacterAnimeManagerB
    {
        public PlayerAnimeManager(Animator animator,
            int mainLayer, int idleLayer, string idleStateName) : base(animator)
        {
            _mainLayer = mainLayer;
            _idleLayer = idleLayer;
            _idleStateName = idleStateName;
        }

        public void Update()
        {
            SyncIdle();
        }

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

        public void FatalHit()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_fatalHit);
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

        public void ResetIdle()
        {
            _animator.SetTrigger(_idle);
        }


        public void PauseAnimator()
        {
            if (_animator == null) return;
            _animatorSpeed = _animator.speed;
            _animator.speed = 0f;
        }

        public void ResumeAnimator()
        {
            if (_animator == null) return;
            _animator.speed = _animatorSpeed;
        }

        private readonly int _moveX = Animator.StringToHash("MoveX");
        private readonly int _moveZ = Animator.StringToHash("MoveZ");

        private readonly int _avoid = Animator.StringToHash("Avoid");
        private readonly int _hit = Animator.StringToHash("Hit");
        private readonly int _fatalHit = Animator.StringToHash("FatalHit");

        private readonly int _shoot = Animator.StringToHash("Shoot");
        private readonly int _combo = Animator.StringToHash("Combo");
        private readonly int _chargeShoot = Animator.StringToHash("ChargeShoot");

        private readonly int _skill = Animator.StringToHash("Skill");

        private readonly int _idle = Animator.StringToHash("Idle");

        private readonly int _mainLayer;
        private readonly int _idleLayer;
        private readonly string _idleStateName;

        private float _animatorSpeed;
        private bool _isNotIdle;

        private void SyncIdle()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(_mainLayer);
            bool isNotIdle = stateInfo.IsName(_idleStateName);

            if (isNotIdle != _isNotIdle) //変化時
            {
                if (isNotIdle)
                {
                    _animator.SetLayerWeight(_idleLayer, 0);
                    Debug.Log("not idle");
                }
                else
                {
                    _animator.SetLayerWeight(_idleLayer, 1);
                    Debug.Log("idle");
                }
            }

            _isNotIdle = isNotIdle;
        }
    }
}