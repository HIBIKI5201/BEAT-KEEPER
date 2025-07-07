using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     プレイヤー攻撃のインジケーターUI
    /// </summary>
    public class PlayerIndicator : RingIndicatorBase
    {
        public override int EffectLength => 3;

        /// <summary>
        /// エフェクトを再生
        /// </summary>
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
                
                // 2拍目　念のため判定が始まる2拍目のタイミングでアクション登録を行う
                case 2:
                    _player.OnShootComboAttack += OnPlayerAttackSuccess;
                    break;
            }
        }

        // Justタイミングは2拍後
        private const float CONTRACTION_SPEED = 2;
        // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        private const float RECEPTION_TIME = 0.45f;

        [Header("色設定")]
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;

        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private DG.Tweening.Sequence _successSequence;
        private DG.Tweening.Sequence _failSequence;
        
        private Image[] _ringImages = new Image[2];

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // 2種類のTweenを使用するため、配列も2つ分確保する
            _tweens = new Tween[2];
            
            // 子オブジェクトからImageコンポーネントを取得
            // NOTE: 通常攻撃リングは自身のImageコンポーネントではなく子オブジェクトのリングを操作しているため、取得する必要がある
            if (transform.childCount > 2)
            {
                // Child(0) は収縮するリングのオブジェクト
                _ringImages[0] = transform.GetChild(1).GetComponent<Image>();
                _ringImages[1] = transform.GetChild(2).GetComponent<Image>();
            }
            else
            {
                Debug.LogError($"PlayerIndicator: 必要な子オブジェクトが不足しています。{gameObject.name}");
                return;
            }
            
            // 初期化
            _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            ResetRingsColor(_defaultColor);
        }
        
        /// <summary>
        /// リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            var sequence = DOTween.Sequence()
                
                // Just判定まで縮小を行う
                .Append(_ringImage.rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定を過ぎたら縮小は続行しつつ段々フェードアウトする
                .Append(_ringImage.rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(CreateFadeSequence(beatDuration * RECEPTION_TIME))
                
                // シーケンスが中断されなかった場合はミス。失敗演出を行う
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
        }

        /// <summary>
        /// 当たりエフェクト（プレイヤーが攻撃に成功したときに再生）
        /// </summary>
        private void OnPlayerAttackSuccess()
        {
            if (_timing < MusicEngineHelper.GetBeatSinceStart())
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
            _successSequence.Join(CreateColorChangeSequence(_successColor, _fadeDuration));
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
            _failSequence.Append(CreateColorChangeSequence(Color.darkGray, fadeDuration));
            _failSequence.Join(CreateFadeSequence(fadeDuration));
            
            _failSequence.OnComplete(End);
        }

        public override void End()
        {
            // NOTE: InitializeComponents()より先に表示されてしまうのでここでも初期化を行う
            // 拡大状態から始めたいので、リングの拡大率をベースクラスで設定した拡大率に変更する
            _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            ResetRingsColor(_defaultColor);
            
            _player.OnShootComboAttack -= OnPlayerAttackSuccess;
            
            // 全てのTweenを停止
            _successSequence?.Kill();
            _failSequence?.Kill();

            if (_tweens != null && _tweens.Length > 0)
            {
                _tweens[1]?.Kill();
            }
            
            base.End();
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color)
        {
            if(_ringImage != null) _ringImage.color = color;
            if(_ringImages[0] != null) _ringImages[0].color = color;
            if(_ringImages[1] != null) _ringImages[1].color = color;
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
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, float duration)
        {
            var colorSequence = DOTween.Sequence();
            
            colorSequence.Join(_ringImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            if (_ringImages[0] != null)
                colorSequence.Join(_ringImages[0].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            if (_ringImages[1] != null)
                colorSequence.Join(_ringImages[1].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
    }
}
