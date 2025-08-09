using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// チャージ攻撃のインジケーターUI
    /// </summary>
    public class ChargeIndicator : RingIndicatorBase
    {
        public override int EffectLength => 5;

        /// <summary>
        /// エフェクトを再生
        /// </summary>
        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                case 1:
                    InitializeComponents();
                    StartContractionEffect();
                    break;
            }
        }
        
        public override void End()
        {
            // イベントの購読解除
            _player.OnStartChargeAttack -= OnPlayerCharge;
            _player.OnFullChargeAttack -= OnPlayerAttackSuccess;
            _player.OnNonFullChargeAttack -= PlayFailEffect;
            
            // Tweens配列をクリア
            if (_tweens != null)
            {
                for (int i = 0; i < _tweens.Length; i++)
                {
                    _tweens[i]?.Kill();
                }
            }
            
            ResetAllComponents();
            
            base.End();
        }
        
        private const float CONTRACTION_SPEED = 4; // 縮小を始めてからJustタイミングは4拍後
        private const float CHARGE_TIME = 2; // チャージにかかる拍
        private const float RECEPTION_TIME = 0.45f; // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        
        // テキストコンポーネントで使用する文字列
        private const string DEFAULT_TEXT = "CHARGE";
        private const string DEFAULT_KEY_TEXT = "PRESS T KEY";
        private const string COMPLEAT_TEXT = "FIRE!!!";
        private const string COMPLEAT_KEY_TEXT = "RELEASE";
        
        [Header("コンポーネントの参照")] 
        [SerializeField] private Image _blurImage; // 収縮を行う枠の発光演出用のリング

        [SerializeField] private Image _maskImage;
        [SerializeField] private Image _gaugeImage;
        [SerializeField] private Text[] _centerTexts;

        [Header("追加の色設定")] 
        [SerializeField] private Color _chargeColor = Color.cyan;
        [SerializeField] private Color _criticalColor = Color.red;

        private void Start()
        {
            ResetAllComponents();
        }
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            ResetAllComponents();

            _tweens = new Tween[5];

            _player.OnStartChargeAttack += OnPlayerCharge; // チャージ開始
            _player.OnFullChargeAttack += OnPlayerAttackSuccess; // チャージ完了したあとに攻撃
            _player.OnNonFullChargeAttack += PlayFailEffect; // チャージ完了前に攻撃（=チャージ攻撃失敗）
        }

        #region ベースとなる演出
        
        /// <summary>
        /// リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 表示
            SetAllAlpha(1f);
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_blurImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_blurImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[2] = blurPulseSequence;

            var sequence = DOTween.Sequence()
                
                // パンチアニメーション
                .Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.2f, beatDuration * 0.2f, 3, 0.8f))
                .Join(CreateTextPunch(beatDuration * 0.2f))
                
                // 縮小開始（完全には収縮しきらないようにする）
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale * 1.5f, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_gaugeImage.rectTransform.DOScale(_centerRingsScale * 1.5f, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_blurImage.rectTransform.DOScale(_centerRingsScale * 1.5f, beatDuration * CONTRACTION_SPEED * 0.7f).SetEase(Ease.Linear))
                
                // Just判定後も縮小を続ける
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
        }

        #endregion

        /// <summary>
        /// チャージ中の演出
        /// </summary>
        private void OnPlayerCharge()
        {
            // 進行中の縮小以外の演出を停止。最終値に到達させた状態にする
            _tweens[0]?.Kill();
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            var totalDuration = beatDuration * CHARGE_TIME;
    
            // マスクのスケールを外側リングの大きさに合わせる
            _maskImage.rectTransform.localScale = _ringImage.rectTransform.localScale;

            var sequence = DOTween.Sequence()
                
                // ゲージを確実に表示させる
                .Append(_gaugeImage.DOFade(1f, totalDuration * 0.1f).SetEase(Ease.OutQuad))
    
                // 即座にゲージの色をチャージ中の色に変更（全体の10%）
                .Join(_gaugeImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutQuad).SetDelay(totalDuration * 0.2f))
                
                // 色変更
                .Join(_ringImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash))
                .Join(_blurImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash))
                .Join(_selfImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash))
                
                // メインのマスクアニメーション（全体の60%で最初のスケール）
                .Append(_maskImage.rectTransform.DOScale(_centerRingsScale, totalDuration * 0.9f).SetEase(Ease.OutQuart))
        
                .OnComplete(OnChargeComplete);
            
            _tweens[1] = sequence;
        }

        /// <summary>
        /// チャージ完了時の処理
        /// </summary>
        private void OnChargeComplete()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 色とテキストを変更
            ResetRingsColor(_successColor, _successColor);
            ChangeText(COMPLEAT_TEXT, COMPLEAT_KEY_TEXT);
        }
        
        /// <summary>
        /// 当たりエフェクト（プレイヤーが攻撃に成功したときに再生）
        /// </summary>
        private void OnPlayerAttackSuccess()
        {
            if (MusicEngineHelper.GetBeatNearerSinceStart() != _timing)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }   
            
            // 他のすべてのTweenをキル
            for (int i = 0; i < _tweens.Length; i++)
            {
                _tweens[i]?.Kill();
            }
            
            // 色とテキストが変更されていない場合、念のためここで変えておく
            ResetRingsColor(_successColor, _successColor);
            ChangeText(COMPLEAT_TEXT, COMPLEAT_KEY_TEXT);
           
            var sequence = DOTween.Sequence()
                
                // 拡大
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale * 1.5f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.8f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale * 2f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(CreateSuccessTextExplosion(_blinkDuration * 0.4f))
                
                // フェードアウト
                .Join(CreateFadeSequence(_fadeDuration))
                
                .OnComplete(End);

            _tweens[3] = sequence;
        }

        /// <summary>
        /// 失敗演出（チャージ完了前に終了）
        /// </summary>
        private void PlayFailEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                _tweens[i]?.Kill();
            }
            
            var sequence = DOTween.Sequence()
                
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale * 0.6f, _fadeDuration * 0.3f).SetEase(Ease.InQuad))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 0.7f, _fadeDuration * 0.3f).SetEase(Ease.InQuad))
                
                // フェードアウト
                .Append(CreateFadeSequence(_fadeDuration * 0.7f))
                .OnComplete(End);
            
            _tweens[0] = sequence;
        }

        #region Reset

        /// <summary>
        /// 全コンポーネントの完全初期化
        /// </summary>
        private void ResetAllComponents()
        {
            // スケールをリセット
            ResetRingsScale();
            
            // 色をデフォルトに設定
            ResetRingsColor(_defaultColor, _translucentDefaultColor);

            // テキストオブジェクトのTransformをリセットする
            ResetText();
            
            // テキストをデフォルトに設定
            ChangeText(DEFAULT_TEXT, DEFAULT_KEY_TEXT);
            
            SetAllAlpha(0f);
        }
        
        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        private void ResetRingsScale()
        {
            // 収縮する一番外側のリング
            if(_ringImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            if(_blurImage != null) _blurImage.rectTransform.localScale = Vector3.one * _initialScale;
            
            // チャージのゲージを管理しているもの
            if(_maskImage != null) _maskImage.rectTransform.localScale = Vector3.one * _initialScale;
            if(_gaugeImage != null) _gaugeImage.rectTransform.localScale = Vector3.one * _initialScale;
            
            // 中央のUI
            if (_selfImage != null) _selfImage.rectTransform.localScale = _centerRingsScale;
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color, Color translucentColor)
        {
            // NOTE: マスクの画像は色を変える必要がないのでここには書いていない
            if(_ringImage != null) _ringImage.color = color;
            if(_gaugeImage != null) _gaugeImage.color = translucentColor;
            if(_selfImage != null) _selfImage.color = color;
            if(_blurImage != null) _blurImage.color = translucentColor;
            if(_centerTexts[0] != null) _centerTexts[0].color = color;
            if(_centerTexts[1] != null) _centerTexts[1].color = color;
        }

        /// <summary>
        /// 全要素のアルファ値設定
        /// </summary>
        private void SetAllAlpha(float alpha)
        {
            if(_ringImage != null) _ringImage.color = new Color(_ringImage.color.r, _ringImage.color.g, _ringImage.color.b, alpha);
            if(_gaugeImage != null) _gaugeImage.color = new Color(_gaugeImage.color.r, _gaugeImage.color.g, _gaugeImage.color.b, alpha);
            if(_selfImage != null) _selfImage.color = new Color(_selfImage.color.r, _selfImage.color.g, _selfImage.color.b, alpha);
            if(_blurImage != null) _blurImage.color = new Color(_blurImage.color.r, _blurImage.color.g, _blurImage.color.b, alpha);
            if(_centerTexts != null)
            {
                foreach (var text in _centerTexts)
                {
                    if (text != null) text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                }
            }
        }
        
        /// <summary>
        /// 表示する文字列を変更する
        /// </summary>
        private void ChangeText(string text, string inputText)
        {
            if (_centerTexts != null && _centerTexts.Length >= 2)
            {
                if(_centerTexts[0] != null) _centerTexts[0].text = text;
                if(_centerTexts[1] != null) _centerTexts[1].text = inputText;
            }
        }

        /// <summary>
        /// テキストオブジェクトのTransformをリセットする
        /// </summary>
        private void ResetText()
        {
            foreach (var text in _centerTexts)
            {
                text.rectTransform.localScale = _centerRingsScale;
                text.rectTransform.localScale = _centerRingsScale;
                text.rectTransform.localRotation = Quaternion.identity;
            }
        }
        
        #endregion

        #region Create Tween

        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_gaugeImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_selfImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_blurImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_centerTexts[0].DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_centerTexts[1].DOFade(0f, duration).SetEase(Ease.Linear));
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor, float duration)
        {
            var colorSequence = DOTween.Sequence();
            
            colorSequence.Join(_ringImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_gaugeImage.DOColor(translucentColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_selfImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_blurImage.DOColor(translucentColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_centerTexts[0].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_centerTexts[1].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
        
        /// <summary>
        /// テキストパンチ演出
        /// </summary>
        private Tween CreateTextPunch(float duration)
        {
            var sequence = DOTween.Sequence();
            
            if (_centerTexts != null)
            {
                foreach (var text in _centerTexts)
                {
                    if (text != null)
                    {
                        sequence.Join(text.rectTransform.DOPunchScale(Vector3.one * 0.3f, duration, 2, 0.5f));
                        sequence.Join(text.DOColor(_defaultColor, duration * 0.5f).SetEase(Ease.OutFlash));
                    }
                }
            }
            
            return sequence;
        }
        
        // <summary>
        /// 成功時のテキスト爆発演出
        /// </summary>
        private Tween CreateSuccessTextExplosion(float duration)
        {
            var sequence = DOTween.Sequence();
            
            if (_centerTexts != null)
            {
                foreach (var text in _centerTexts)
                {
                    if (text != null)
                    {
                        sequence.Join(text.rectTransform.DOScale(Vector3.one * 1.8f, duration * 0.3f).SetEase(Ease.OutBack));
                        sequence.Join(text.rectTransform.DOScale(Vector3.one * 1.2f, duration * 0.7f).SetEase(Ease.InBack).SetDelay(duration * 0.3f));
                        sequence.Join(text.rectTransform.DOShakeRotation(duration, 15f, 10, 90f, true));
                    }
                }
            }
            
            return sequence;
        }
        
        #endregion
    }
}
