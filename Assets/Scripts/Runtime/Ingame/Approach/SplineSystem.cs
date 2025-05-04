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
        SplineMover _splineMover;

        void Awake()
        {
            _splineMover = new SplineMover(_splineContainer, transform);
        }
        [ContextMenu("MoveToNext")]
        public async void MoveToNext()
        {
            await _splineMover.MoveToNext(_speed);
        }
    }
}
