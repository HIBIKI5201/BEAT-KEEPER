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
        float _speed = 1f;
        int _progress = 0;
        Tween _moveTween;
        public SplineMover(SplineContainer splineContainer,Transform transform)
        {
            _splineContainer = splineContainer;
            _transform = transform;
        }
        /// <summary>
        /// 次のSplineに移動します。
        /// /// </summary>
        public async UniTask MoveToNext()
        {
            //Tweenがアクティブな場合はKillして移動する
            if (_moveTween.IsActive())
            {
                Debug.Log("移動中です。");
                _progress++;
                _moveTween.Kill();
                await SkipToNext();
            }
            // Splineの数を超えた場合は何もしない
            if (_progress >= _splineContainer.Splines.Count)
            {
                Debug.Log("全てのSplineを移動しました。");
                return;
            }
            float progressOnSpline = 0f;
            _moveTween = DOTween.To(() => progressOnSpline, x => progressOnSpline = x, 1f, _speed)
                .OnUpdate(() =>
                {
                    _transform.position = _splineContainer.Splines[_progress].EvaluatePosition(progressOnSpline);
                }).OnComplete(() =>
                {
                    _progress++;
                });
        }
        /// <summary>
        /// 次のSplineに移動します。
        /// /// </summary>
        /// <param name="speed">移動速度</param>
        public async UniTask MoveToNext(float speed)
        {
            _speed = speed;
            //Tweenがアクティブな場合は現在のSplineの移動を無視して次のSplineに移動する
            if (_moveTween.IsActive())
            {
                Debug.Log("移動中です。");
                _progress++;
                _moveTween?.Kill();
                await SkipToNext();
            }
            // Splineの数を超えた場合は何もしない
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
                    _transform.position = _splineContainer.Splines[_progress].EvaluatePosition(progressOnSpline);
                }).OnComplete(() =>
                {
                    _progress++;
                });
        }
        async UniTask SkipToNext()
        {
            if(_progress >= _splineContainer.Splines.Count)return;
            _moveTween = _transform.DOMove(_splineContainer.Splines[_progress].EvaluatePosition(0),0.1f);
            await _moveTween.AsyncWaitForCompletion();
        }
    }
}
