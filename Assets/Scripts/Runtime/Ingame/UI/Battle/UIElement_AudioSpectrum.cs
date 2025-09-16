using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using BeatKeeper.Runtime.Ingame.System;

namespace BeatKeeper
{
    /// <summary>
    /// オーディオスペクトラム管理クラス
    /// </summary>
    public class UIElement_AudioSpectrum : MonoBehaviour
    {
        /// <summary>
        /// 演出開始
        /// </summary>
        public async UniTask PlayAudioSpectrum()
        {
            // 念のため演出をキャンセル
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            // キャンバスを表示
            _canvasGroup.DOFade(1, _fadeDuration);
            
            var leftTask = AnimateSpectrumAsync(_leftImage, _cts.Token);
            var rightTask = AnimateSpectrumAsync(_rightImage, _cts.Token);
            
            // 両方のアニメーションを並行実行
            await UniTask.WhenAll(leftTask, rightTask);
        }

        public void StopAudioSpectrum()
        {
            // キャンバスを非表示にしたあと、アニメーションをキャンセル
            _canvasGroup.DOFade(0, _fadeDuration).OnComplete(() => _cts?.Cancel());
        }
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _leftImage;
        [SerializeField] private Image _rightImage;
        [SerializeField] private SpriteAtlas _spectrumsAtlas;
        
        [Header("アニメーションの設定")]
        [SerializeField] private float _fadeDuration = 0.15f;
        
        private Sprite[] _spritesCache; // SpriteAtlasのデータのキャッシュ
        private CancellationTokenSource _cts;
        
        private void Start()
        {
            // 初期状態は非表示にしておく
            _canvasGroup.alpha = 0;
            
            // NOTE: Imageの透明度は全てキャンバスグループで操作するものとし、
            // Image自体のアルファ値は変えずに処理を行う

            // アトラスに登録されている枚数分配列を確保したあと画像を全取得
            _spritesCache = new Sprite[_spectrumsAtlas.spriteCount];
            _spectrumsAtlas.GetSprites(_spritesCache);
            
            // スプライトを名前順でソート（連番画像の正しい順序のため）
            System.Array.Sort(_spritesCache, (a, b) => a.name.CompareTo(b.name));
            
            // 初期画像を設定
            if (_spritesCache.Length > 0)
            {
                _leftImage.sprite = _spritesCache[0];
                _rightImage.sprite = _spritesCache[0];
            }
        }
        
        /// <summary>
        /// アニメーションの処理
        /// </summary>
        private async UniTask AnimateSpectrumAsync(Image targetImage, CancellationToken token)
        {
            if (_spritesCache == null || _spritesCache.Length == 0)
            {
                return;
            }
            
            try
            {
                // アニメーション開始時の時間を記録
                // NOTE: while文を繰り返すと段々遅くなっていくためその対策 
                float animationStartTime = Time.time;
                
                // フレーム間隔を計算（BPM / 画像枚数）
                float frameDuration = (float)(MusicEngineHelper.DurationOfBeat / _spectrumsAtlas.spriteCount);
                
                while (!token.IsCancellationRequested)
                {
                    // 現在の経過時間から正しいフレーム番号を計算
                    float elapsedTime = Time.time - animationStartTime;
                    int targetFrame = Mathf.FloorToInt(elapsedTime / frameDuration) % _spritesCache.Length;
                    
                    // フレームが変わった時のみ更新する
                    if (targetFrame < _spritesCache.Length && _spritesCache[targetFrame] != null)
                    {
                        targetImage.sprite = _spritesCache[targetFrame];
                    }
            
                    // 次のフレーム時間まで待機
                    float nextFrameTime = (targetFrame + 1) * frameDuration;
                    float waitTime = nextFrameTime - elapsedTime;
            
                    if (waitTime > 0)
                    {
                        await UniTask.Delay(Mathf.Max(1, Mathf.RoundToInt(waitTime * 1000)), cancellationToken: token);
                    }
                    else
                    {
                        // フレームドロップ対応：1フレーム待機
                        await UniTask.NextFrame(token);
                    }
                }
            }
            catch (System.OperationCanceledException ex)
            {
                // キャンセル時は正常終了として扱う
            }
        }
    }
}
