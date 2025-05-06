using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using static SymphonyFrameWork.System.PauseManager;
using SymphonyFrameWork.Utility;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    /// <summary>
    /// SplineMoverは、SplineContainerを使用してTransformを移動させるクラスです。
    /// </summary>
    public class SplineMover : IPausable
    {
        SplineContainer _splineContainer;
        Transform _playerTransform;
        float _skipTime;
        int _progress = 0;
        Tween _moveTween;
        /// <summary>
        /// SplineMoverのコンストラクタ
        /// /// </summary>
        /// <param name="splineContainer">SplineContainer</param>
        /// <param name="transform">プレイヤーのTransform</param>
        /// <param name="skipTime">スキップする時間。MoveToNextのtimeより短くある必要があります。</param>
        public SplineMover(SplineContainer splineContainer,Transform transform,float skipTime = 0.1f)
        {
            _splineContainer = splineContainer;
            _playerTransform = transform;
            _skipTime = skipTime;
            IPausable.RegisterPauseManager(this);
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
            //Spline上の進捗を保持する変数の初期化
            float progressOnSpline = 0f;
            //Splineの進捗をTweenで移動させる
            _moveTween = DOTween.To(() => progressOnSpline, x => progressOnSpline = x, 1f, time)
                .OnUpdate(() =>
                {
                    //playerの前方をSplineの接線に合わせる
                    _playerTransform.forward = _splineContainer.Splines[_progress].EvaluateTangent(progressOnSpline);
                    //playerの位置をSpline上の進捗に合わせる
                    _playerTransform.position = _splineContainer.Splines[_progress].EvaluatePosition(progressOnSpline);
                }).OnComplete(() =>
                {
                    //完了したらSplineの進捗を1つ進める
                    _progress++;
                });
        }
        async UniTask SkipToNext(float time)
        {
            //Splianeの数を超えた場合は何もしない
            if(_progress >= _splineContainer.Splines.Count)return;
            //Tweenを使用して移動する
            _moveTween = _playerTransform.DOMove(_splineContainer.Splines[_progress].EvaluatePosition(0),time);
            //Tweenが完了するまで待機する
            await _moveTween.AsyncWaitForCompletion();
        }
        //Pauseインターフェイスの実装
        public void Pause()
        {
            if (_moveTween.IsActive())
            {
                _moveTween.Pause();
            }
        }

        public void Resume()
        {
            if (_moveTween.IsActive())
            {
                _moveTween.Play();
            }
        }
    }
}
