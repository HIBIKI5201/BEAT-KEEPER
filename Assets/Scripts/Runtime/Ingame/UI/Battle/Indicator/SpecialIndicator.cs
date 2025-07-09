using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     スキルのインジケーターUI
    /// </summary>
    public class SpecialIndicator : RingIndicatorBase
    {
        public override int EffectLength => 3;

        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                // 1拍目　縮小エフェクトを開始する
                case 1:
                    InitializeComponents();
                    StartContractionEffect();
                    break;
                
                // 3拍目　スキル発動の演出を行う
                case 2:
                    _player.OnSkill += PlaySkillEffect;
                    break;
            }
        }
        
        public override void End()
        {
            _player.OnSkill -= PlaySkillEffect;
            
            // 全てのTweenを停止
            _successSequence?.Kill();
            _failSequence?.Kill();

            if (_tweens != null && _tweens.Length > 0)
            {
                _tweens[1]?.Kill();
            }
            
            // NOTE: InitializeComponents()より先に表示されてしまうのでここでも初期化を行う
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
            
            base.End();
        }

        // Justタイミングは2拍後
        private const float CONTRACTION_SPEED = 2;
        // Justタイミングのあとの判定受付時間
        private const float RECEPTION_TIME = 0.45f;

        [SerializeField] Text _centerText;
        
        [SerializeField] private Image[] _ringImages = new Image[2];
        [SerializeField] private Vector3 _centerRingsScale = Vector3.one;

        [Header("色設定")]
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _translucentSuccessColor = Color.yellow; // 半透明の成功色
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _translucentDefaultColor = Color.white; // 半透明のデフォルト色

        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private DG.Tweening.Sequence _successSequence;
        private DG.Tweening.Sequence _failSequence;

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            _timing = MusicEngineHelper.GetBeatSinceStart();

            // 2種類のTweenを使用するため、配列も2つ分確保する
            _tweens = new Tween[2];
            
            // 初期化
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);

            _centerText.DOFade(1, 0.1f);
        }
        
        /// <summary>
        /// リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;

            var sequence = DOTween.Sequence()
                
                // Just判定まで縮小を行う
                .Append(_ringImages[0].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_ringImages[1].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定を過ぎたら縮小は続行しつつ段々フェードアウトする
                .Append(_ringImages[0].rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(_ringImages[1].rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(CreateFadeSequence(beatDuration * RECEPTION_TIME))
                
                // シーケンスが中断されなかった場合はミス。失敗演出を行う
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
        }

        /// <summary>
        /// スキル発動エフェクト
        /// </summary>
        private void PlaySkillEffect()
        {
            if (_timing > MusicEngineHelper.GetBeatSinceStart())
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }
            
            // 成功した場合はリングの縮小演出は不要になるのでキル
            _tweens[0].Kill();
           
            // 念のため既存の成功エフェクトをキャンセル
            _successSequence?.Kill();
            _successSequence = DOTween.Sequence();

            // パンチスケール
            _successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f));
            
            // 色変更とフェードアウト
            _successSequence.Join(CreateColorChangeSequence(_successColor, _translucentSuccessColor, _fadeDuration));
            _successSequence.Join(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            _successSequence.OnComplete(End);

            _tweens[1] = _successSequence;
        }

        /// <summary>
        /// 失敗演出
        /// </summary>
        private void PlayFailEffect()
        {
            float fadeDuration = 0.2f;
            
            _failSequence?.Kill();
            _failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            _failSequence.Append(CreateColorChangeSequence(Color.darkGray, Color.darkGray, fadeDuration));
            _failSequence.Join(CreateFadeSequence(fadeDuration));
            
            _failSequence.OnComplete(End);
        }

        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        private void ResetRingsScale()
        {
            if(_ringImages[0] != null) _ringImages[0].rectTransform.localScale = Vector3.one * _initialScale;
            if(_ringImages[1] != null) _ringImages[1].rectTransform.localScale = Vector3.one * _initialScale;
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color, Color translucentColor)
        {
            if(_ringImages[0] != null) _ringImages[0].color = color;
            if(_ringImages[1] != null) _ringImages[1].color = translucentColor;
        }
        
        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.Linear));
            
            if (_ringImages[0] != null)
                fadeSequence.Join(_ringImages[0].DOFade(0f, duration).SetEase(Ease.Linear));
            
            if (_ringImages[1] != null)
                fadeSequence.Join(_ringImages[1].DOFade(0f, duration).SetEase(Ease.Linear));
            
            if(_centerText != null)
                fadeSequence.Join(_centerText.DOFade(0f, duration).SetEase(Ease.Linear));
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor, float duration)
        {
            var colorSequence = DOTween.Sequence();
            
            if (_ringImages[0] != null)
                colorSequence.Join(_ringImages[0].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            if (_ringImages[1] != null)
                colorSequence.Join(_ringImages[1].DOColor(translucentColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
    }
}