using UnityEngine;

namespace BeatKeeper
{
    [RequireComponent(typeof(Animator))]
    public class TrafficLightManager : MonoBehaviour
    {
        public void ToPhase1()
        {
            _animator.SetBool(_phaseHash, false);
        }

        public void ToPhase3()
        {
            _animator.SetBool(_phaseHash, true);
        }

        private readonly int _phaseHash = Animator.StringToHash("Phase3");
        
        private Animator _animator;
        private void Awake()
        {
            TryGetComponent(out _animator);
        }
    }
}
