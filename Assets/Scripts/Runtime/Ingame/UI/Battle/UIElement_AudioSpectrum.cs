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
                // フレーム間隔をミリ秒で計算
                int frameIntervalMs = (int)((MusicEngineHelper.DurationOfBeat / _spectrumsAtlas.spriteCount) * 1000);
                
                int currentFrame = 0;
                
                while (!token.IsCancellationRequested)
                {
                    // 現在のフレームの画像を設定
                    if (currentFrame < _spritesCache.Length && _spritesCache[currentFrame] != null)
                    {
                        targetImage.sprite = _spritesCache[currentFrame];
                    }
                    
                    // 次のフレームへ
                    currentFrame = (currentFrame + 1) % _spritesCache.Length;
                    
                    // フレーム間隔分待機
                    await UniTask.Delay(frameIntervalMs, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException ex)
            {
                // キャンセル時は正常終了として扱う
            }
        }
    }
}
