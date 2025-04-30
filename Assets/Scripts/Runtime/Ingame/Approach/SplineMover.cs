using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    public class SplineMover : MonoBehaviour
    {
        [SerializeField, Tooltip("SplineContainerを指定してください")] SplineContainer _splineContainer;
        [SerializeField, Tooltip("Spline間の移動時間")] float _speed = 1f;
        int _progress = 0;
        Tween _moveTween;
        void Awake()
        {
            if (_splineContainer == null)
            {
                Debug.LogError("SplineContainerが指定されていません。");
                return;
            }
            if (_splineContainer.Splines.Count == 0)
            {
                Debug.LogError("SplineContainerにSplineがありません。");
                return;
            }
        }
        [ContextMenu("MoveToNext")]
        /// <summary>
        /// 次のSplineに移動します。
        /// /// </summary>
        public void MoveToNext()
        {
            if (_progress >= _splineContainer.Splines.Count)
            {
                Debug.Log("全てのSplineを移動しました。");
                return;
            }
            float progressOnSpline = 0f;
            _moveTween?.Kill();
            _moveTween = DOTween.To(() => progressOnSpline, x => progressOnSpline = x, 1f, _speed)
                .OnUpdate(() =>
                {
                    transform.position = _splineContainer.Splines[_progress].EvaluatePosition(progressOnSpline);
                })
                .OnComplete(() =>
                {
                    _progress++;
                });
        }
    }
}
