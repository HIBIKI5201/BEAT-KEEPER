using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    public class AutoMove : MonoBehaviour
    {
        [SerializeField, Tooltip("SplineContainerを指定してください")] SplineContainer _splineContainer;
        [SerializeField, Tooltip("Spline間の移動時間")] float _speed = 1f;
        float _progress = 0f;

        void Awake()
        {
        }
        /// <summary>
        /// 次のSplineに移動します
        /// </summary>
        [ContextMenu("MoveToNext")]
        public void MoveToNext()
        {
        }
    }
}
