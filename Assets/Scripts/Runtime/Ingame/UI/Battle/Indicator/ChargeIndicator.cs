using BeatKeeper.Runtime.Ingame.System;
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
                    StartEntryEffect();
                    break;
                case 3:
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
                    _tweens[i] = null;
                }
            }
            
            ResetAllComponents();
            
            // 全要素のアルファ値をリセット（透明状態から正常状態へ）
            SetAllAlpha(0f);

            _isComplete = false;
            
            base.End();
        }
        
        private const float CONTRACTION_SPEED = 2; // 縮小を始めてからJustタイミングは2拍後
        private const float CHARGE_TIME = 2; // チャージにかかる拍
        private const float RECEPTION_TIME = 0.45f; // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        
        // テキストコンポーネントで使用する文字列
        private const string DEFAULT_TEXT = "CHARGE";
        private const string DEFAULT_KEY_TEXT = "PRESS T KEY";
        private const string COMPLEAT_TEXT = "FIRE!!!";
        private const string COMPLEAT_KEY_TEXT = "RELEASE";
        
        [SerializeField] private Vector3 _centerRingsScale = Vector3.one; // 中央のリングの拡大率

        [Header("コンポーネントの参照")] 
        [SerializeField] private Image _blurImage; // 収縮を行う枠の発光演出用のリング

        [SerializeField] private Image _maskImage;
        [SerializeField] private Image _gaugeImage;
        [SerializeField] private Text[] _centerTexts;

        [Header("色設定")] 
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _translucentSuccessColor = Color.yellow; // 半透明の成功色
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _translucentDefaultColor = Color.white; // 半透明のデフォルト色
        [SerializeField] private Color _chargeColor = Color.cyan;
        [SerializeField] private Color _criticalColor = Color.red;

        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private bool _isComplete = false; // チャージ完了 

        private void Start()
        {
            ResetAllComponents();
            SetAllAlpha(0f);
        }
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            _timing = MusicEngineHelper.GetBeatSinceStart();

            _tweens = new Tween[5];

            _player.OnStartChargeAttack += OnPlayerCharge; // チャージ開始
            _player.OnFullChargeAttack += OnPlayerAttackSuccess; // チャージ完了したあとに攻撃
            _player.OnNonFullChargeAttack += PlayFailEffect; // チャージ完了前に攻撃（=チャージ攻撃失敗）
            
            ResetAllComponents();
            
            // 全要素のアルファ値をリセット（透明状態から正常状態へ）
            SetAllAlpha(0f);
        }

        #region ベースとなる演出

        /// <summary>
        /// 登場演出
        /// </summary>
        private void StartEntryEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 全要素を一旦透明・縮小状態に
            SetAllAlpha(0f);
            SetAllScale(Vector3.zero);
            
            var sequence = DOTween.Sequence()
                
                // 外側リング
                .Append(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.3f, beatDuration * 0.1f).SetEase(Ease.OutBack))
                .Join(_ringImage.DOFade(1f, beatDuration * 0.1f).SetEase(Ease.OutQuad))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.3f, beatDuration * 0.15f).SetEase(Ease.OutBack))
                .Join(_blurImage.DOFade(0.6f, beatDuration * 0.15f).SetEase(Ease.OutQuad))
                
                // 少し遅れて中央UI（中央の円・テキスト・ゲージを登場させる）
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale * 1.1f, beatDuration * 0.1f).SetEase(Ease.OutBack))
                .Join(_selfImage.DOFade(1f, beatDuration * 0.1f).SetEase(Ease.OutQuad))
                .Join(_gaugeImage.rectTransform.DOScale(Vector3.one * _initialScale, beatDuration * 0.1f).SetEase(Ease.OutBack))
                .Join(_gaugeImage.DOFade(1f, beatDuration * 0.1f).SetEase(Ease.OutQuad))
                .Join(FadeInTexts(beatDuration * 0.1f))
                
                // 最終的なサイズに調整
                .Append(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale, beatDuration * 0.1f).SetEase(Ease.InBack))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale, beatDuration * 0.1f).SetEase(Ease.InBack))
                .Join(_selfImage.rectTransform.DOScale(_centerRingsScale, beatDuration * 0.1f).SetEase(Ease.InBack))
                .Join(_blurImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.1f))
                
                // 待機中のパルス開始
                .AppendCallback(StartIdlePulse);
            
            _tweens[0] = sequence;
        }
        
        /// <summary>
        /// 待機中のパルス演出
        /// </summary>
        private void StartIdlePulse()
        {
            // 念のため最終値にジャンプさせて終了するように保証
            _tweens[0]?.Kill(true);
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // メインリングの心拍パルス
            var pulseSequence = DOTween.Sequence()
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale * 1.1f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_blurImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_blurImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[1] = pulseSequence;
            _tweens[2] = blurPulseSequence;
        }
        
        /// <summary>
        /// リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // パルス演出を停止
            _tweens[1]?.Kill(true);
            _tweens[2]?.Kill(true);

            var sequence = DOTween.Sequence()
                // 一瞬静止
                .Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.2f, beatDuration * 0.2f, 3, 0.8f))
                .Join(CreateTextPunch(beatDuration * 0.2f));
                
            // チャージが完了していない場合のみ色変更
            if (!_isComplete)
            {
                sequence.Join(_ringImage.DOColor(_chargeColor, beatDuration * 0.2f).SetEase(Ease.OutFlash))
                    .Join(_blurImage.DOColor(_chargeColor, beatDuration * 0.2f).SetEase(Ease.OutFlash))
                    .Join(_selfImage.DOColor(_chargeColor, beatDuration * 0.2f).SetEase(Ease.OutFlash));
            }
                
            sequence
                // 縮小開始
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, beatDuration * CONTRACTION_SPEED).SetEase(Ease.InQuart))
                .Join(_gaugeImage.rectTransform.DOScale(_centerRingsScale, beatDuration * CONTRACTION_SPEED).SetEase(Ease.InQuart))
                .Join(_blurImage.rectTransform.DOScale(_centerRingsScale * 1.1f, beatDuration * CONTRACTION_SPEED * 0.7f).SetEase(Ease.OutQuart))
                .Join(_blurImage.DOFade(0.9f, beatDuration * CONTRACTION_SPEED * 0.3f).SetEase(Ease.OutQuart))
                .Join(_blurImage.DOFade(0.2f, beatDuration * CONTRACTION_SPEED * 0.7f).SetEase(Ease.InQuart).SetDelay(beatDuration * CONTRACTION_SPEED * 0.3f))
                
                // Just判定後のクリティカル演出
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(CreateCriticalFlash(beatDuration * RECEPTION_TIME))
                
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
            _tweens[1]?.Kill(true);
            _tweens[2]?.Kill(true);
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // マスクのスケールを外側リングの大きさに合わせる
            _maskImage.rectTransform.localScale = _ringImage.rectTransform.localScale;
            
            var sequence = DOTween.Sequence()
                
                // 一瞬マスクを中央の狭い範囲のみにする
                .Append(_maskImage.rectTransform.DOPunchScale(Vector3.one * 0.2f, beatDuration * 0.2f, 2, 0.5f))
                
                // ゲージを確実に表示させる（Entryアニメーション中だと表示されていない可能性がある）
                .Join(_gaugeImage.DOFade(1f, beatDuration * 0.1f).SetEase(Ease.OutQuad))
                
                // メインのマスクアニメーション
                .Append(_maskImage.rectTransform.DOScale(_centerRingsScale * 0.7f, beatDuration * CHARGE_TIME * 0.6f).SetEase(Ease.OutQuart))
                .Join(_maskImage.rectTransform.DOScale(_centerRingsScale, beatDuration * CHARGE_TIME).SetEase(Ease.InBack).SetDelay(beatDuration * CHARGE_TIME * 0.6f))
                
                // 即座にゲージの色をチャージ中の色に変更
                .Join(_gaugeImage.DOColor(_chargeColor, beatDuration * 0.1f).SetEase(Ease.OutQuad))
                
                .OnComplete(OnChargeComplete);
            
            _tweens[1] = sequence;
        }

        /// <summary>
        /// チャージ完了時の処理
        /// </summary>
        private void OnChargeComplete()
        {
            _isComplete = true;
            
            ResetRingsColor(_successColor, _translucentSuccessColor);
            ChangeText(COMPLEAT_TEXT, COMPLEAT_KEY_TEXT);
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            var sequence = DOTween.Sequence()
                
                // 全体が光る
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale * 1.4f, beatDuration * 0.1f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.2f, beatDuration * 0.1f).SetEase(Ease.OutBack))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.5f, beatDuration * 0.1f).SetEase(Ease.OutBack))
                .Join(_blurImage.DOFade(1f, beatDuration * 0.1f).SetEase(Ease.OutQuad))
                
                // 元のサイズに戻りつつ点滅
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale, beatDuration * 0.1f).SetEase(Ease.InBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale, beatDuration * 0.1f).SetEase(Ease.InBack))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale, beatDuration * 0.1f).SetEase(Ease.InBack))
                .Join(_blurImage.DOFade(0.4f, beatDuration * 0.1f).SetEase(Ease.InQuad))
                
                // 連続点滅（3回）
                .Append(_selfImage.DOFade(0.3f, 0.1f).SetLoops(6, LoopType.Yoyo))
                .Join(_ringImage.DOFade(0.3f, 0.1f).SetLoops(6, LoopType.Yoyo))
                .Join(CreateTextFlash(0.6f));
            
            _tweens[2] = sequence;
        }
        
        /// <summary>
        /// 当たりエフェクト（プレイヤーが攻撃に成功したときに再生）
        /// </summary>
        private void OnPlayerAttackSuccess()
        {
            if (_timing + CHARGE_TIME >= MusicEngineHelper.GetBeatSinceStart())
            {
                // チャージ完了タイミングより前なら処理はスキップ
                return;
            }
            
            // 他のすべてのTweenをキル
            for (int i = 0; i < _tweens.Length; i++)
            {
                _tweens[i]?.Kill();
            }
           
            var sequence = DOTween.Sequence()
                
                .Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.8f, _blinkDuration * 0.3f, 1, 0.8f))
                .Join(_ringImage.rectTransform.DOPunchScale(Vector3.one * 0.6f, _blinkDuration * 0.3f, 1, 0.8f))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale * 2.5f, _blinkDuration * 0.15f).SetEase(Ease.OutQuart))
                .Join(_blurImage.DOFade(1f, _blinkDuration * 0.1f).SetEase(Ease.OutQuart))
                .Join(CreateColorChangeSequence(_successColor, _translucentSuccessColor, _blinkDuration * 0.2f))
                
                // 拡大
                .Append(_selfImage.rectTransform.DOScale(_centerRingsScale * 1.5f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 2f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale * 2.5f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
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
        /// 各リングの拡大率を変更する
        /// </summary>
        private void ResetRingsScale()
        {
            // 収縮する一番外側のリング
            if(_ringImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            if(_blurImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            
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
        /// 回転の初期化
        /// </summary>
        private void ResetRotation()
        {
            if (_ringImage != null) _ringImage.rectTransform.localRotation = Quaternion.identity; // メインリングの初期化
            if (_selfImage != null) _selfImage.rectTransform.localRotation = Quaternion.identity; // 中央UIの初期化
            if (_blurImage != null) _blurImage.rectTransform.localRotation = Quaternion.identity; // ブラーエフェクトの初期化
            if (_maskImage != null) _maskImage.rectTransform.localRotation = Quaternion.identity; // マスクの初期化
            if (_gaugeImage != null) _gaugeImage.rectTransform.localRotation = Quaternion.identity; // ゲージの初期化
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
        /// テキストを変更する
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
        /// 全要素のスケール設定
        /// </summary>
        private void SetAllScale(Vector3 scale)
        {
            if(_ringImage != null) _ringImage.rectTransform.localScale = scale;
            if(_gaugeImage != null) _gaugeImage.rectTransform.localScale = scale;
            if(_selfImage != null) _selfImage.rectTransform.localScale = scale;
            if(_blurImage != null) _blurImage.rectTransform.localScale = scale;
        }
        
        /// <summary>
        /// 全コンポーネントの完全初期化
        /// </summary>
        private void ResetAllComponents()
        {
            Debug.Log("Reset All Components");
            
            // スケールをリセット
            ResetRingsScale();
            
            // 各UI要素の回転を初期化
            ResetRotation();
            
            // 色をデフォルトに設定
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
            
            // テキストをデフォルトに設定
            ChangeText(DEFAULT_TEXT, DEFAULT_KEY_TEXT);
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
                        
                        // チャージが完了していない場合のみ色変更
                        if (!_isComplete)
                        {
                            sequence.Join(text.DOColor(_chargeColor, duration * 0.5f).SetEase(Ease.OutFlash));
                        }
                    }
                }
            }
            
            return sequence;
        }
        
        /// <summary>
        /// クリティカル点滅演出
        /// </summary>
        private Tween CreateCriticalFlash(float duration)
        {
            var sequence = DOTween.Sequence();
            
            // 全体を危険色に
            sequence.Join(_ringImage.DOColor(_criticalColor, duration * 0.2f).SetEase(Ease.OutFlash));
            sequence.Join(_selfImage.DOColor(_criticalColor, duration * 0.2f).SetEase(Ease.OutFlash));
            
            // 激しい点滅
            var flashCount = Mathf.FloorToInt(duration / 0.1f);
            sequence.Join(_ringImage.DOFade(0.2f, 0.05f).SetLoops(flashCount, LoopType.Yoyo));
            sequence.Join(_selfImage.DOFade(0.3f, 0.05f).SetLoops(flashCount, LoopType.Yoyo));
            
            // ブラーで危険演出
            sequence.Join(_blurImage.DOColor(_criticalColor, duration * 0.3f).SetEase(Ease.OutFlash));
            sequence.Join(_blurImage.rectTransform.DOShakeScale(duration, 0.1f, 10, 90f, true));
            
            return sequence;
        }
        
        /// <summary>
        /// テキスト点滅演出
        /// </summary>
        private Tween CreateTextFlash(float duration)
        {
            var sequence = DOTween.Sequence();
            
            if (_centerTexts != null)
            {
                foreach (var text in _centerTexts)
                {
                    if (text != null)
                    {
                        sequence.Join(text.DOFade(0.2f, 0.1f).SetLoops(Mathf.FloorToInt(duration / 0.1f), LoopType.Yoyo));
                        sequence.Join(text.rectTransform.DOShakeScale(duration, 0.05f, 10, 90f, true));
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
        
        /// <summary>
        /// テキストのフェードイン
        /// </summary>
        private Tween FadeInTexts(float duration)
        {
            var sequence = DOTween.Sequence();
            
            if (_centerTexts != null)
            {
                foreach (var text in _centerTexts)
                {
                    if (text != null)
                    {
                        sequence.Join(text.DOFade(1f, duration).SetEase(Ease.OutQuad));
                        sequence.Join(text.rectTransform.DOScale(Vector3.one * 1.2f, duration * 0.5f).SetEase(Ease.OutBack));
                        sequence.Join(text.rectTransform.DOScale(Vector3.one, duration * 0.5f).SetEase(Ease.InBack).SetDelay(duration * 0.5f));
                    }
                }
            }
            
            return sequence;
        }
        
        #endregion
    }
}
