using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using R3;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    public class SplineMover
    {
        SplineContainer _splineContainer;
        Transform _transform;
        float _skipTime;
        int _progress = 0;
        Tween _moveTween;
        /// <summary>
        /// SplineMoverのコンストラクタ
        /// /// </summary>
        /// <param name="splineContainer">SplineContainer</param>
        /// <param name="transform">Transform</param>
        /// <param name="skipTime">スキップする時間。MoveToNextのtimeより短くある必要があります。</param>
        public SplineMover(SplineContainer splineContainer,Transform transform,float skipTime = 0.1f)
        {
            _splineContainer = splineContainer;
            _transform = transform;
            _skipTime = skipTime;
        }
        /// <summary>
        /// 次のSplineに移動します。
        /// /// </summary>
        /// <param name="time">時間</param>
        public async UniTask MoveToNext(float time = 1f)
        {
            //Tweenがアクティブな場合は現在のSplineの移動を無視して次のSplineに移動する
            if (_moveTween.IsActive())
            {
                Debug.Log("移動中です。");
                _progress++;
                _moveTween?.Kill();
                time -= _skipTime;
                await SkipToNext(_skipTime);
            }
            // Splineの数を超えた場合は何もしない
            if (_progress >= _splineContainer.Splines.Count)
            {
                Debug.Log("全てのSplineを移動しました。");
                return;
            }
            float progressOnSpline = 0f;
            _moveTween?.Kill();
            _moveTween = DOTween.To(() => progressOnSpline, x => progressOnSpline = x, 1f, time)
                .OnUpdate(() =>
                {
                    _transform.position = _splineContainer.Splines[_progress].EvaluatePosition(progressOnSpline);
                }).OnComplete(() =>
                {
                    _progress++;
                });
        }
        async UniTask SkipToNext(float time)
        {
            if(_progress >= _splineContainer.Splines.Count)return;
            _moveTween = _transform.DOMove(_splineContainer.Splines[_progress].EvaluatePosition(0),time);
            await _moveTween.AsyncWaitForCompletion();
        }
    }
}
