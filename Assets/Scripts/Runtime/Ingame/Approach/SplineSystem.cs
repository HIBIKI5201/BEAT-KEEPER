using BeatKeeper.Runtime.Ingame.Approach;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Splines;

namespace BeatKeeper
{
    public class SplineSystem : MonoBehaviour
    {
        [SerializeField]SplineContainer _splineContainer;
        [SerializeField]float _speed = 1f;
        [SerializeField]float _skipTime = 0.5f;
        SplineMover _splineMover;

        void Awake()
        {
            _splineMover = new SplineMover(_splineContainer, transform, _skipTime);
        }
        [ContextMenu("MoveToNext")]
        public async void MoveToNext()
        {
            await _splineMover.MoveToNext(_speed);
        }
    }
}
