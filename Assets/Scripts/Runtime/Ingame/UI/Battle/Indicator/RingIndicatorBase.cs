using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     インジケーターのベースクラス
    /// </summary>
    [RequireComponent(typeof(Image))]
    public abstract class RingIndicatorBase : MonoBehaviour
    {
        public abstract int EffectLength { get; }

        public void OnInit(PlayerManager player, UIElement_ChartRingManager ringManager)
        {
            _player = player;
            _chartRingManager = ringManager;

            // 中央のリングの画像を操作方法のものに差し替える
            _centerImage.sprite = _guide.Sprite;
            _centerImage.rectTransform.sizeDelta = _guide.SizeDelta;
        }

        public void OnGet(Action onEndAction, Vector2 rectPos, int timing)
        {
            // 終了フラグをリセット
            _isEnded = false;

            _selfImage.rectTransform.position = rectPos
                + new Vector2(Screen.width / 2, Screen.height / 2);

            // 初期化
            InitializeComponents();
            UIInitialized();

            _onEndAction = onEndAction;
            _timing = timing;

            _count = 0;

            CheckRemainTime();

            if (!string.IsNullOrEmpty(_apperanceSoundCueName))
            { SoundEffectManager.PlaySoundEffect(_apperanceSoundCueName); }
        }
        
        // 長押しノーツ用のオーバーロード
        public virtual void OnGet(Action onEndAction, Vector2 startPosition, Vector2 endPosition, int timing)
        {
            // 終了フラグをリセット
            _isEnded = false;

            // 初期化
            InitializeComponents();
            UIInitialized();

            _onEndAction = onEndAction;
            _timing = timing;

            _count = 0;

            CheckRemainTime();

            if (!string.IsNullOrEmpty(_apperanceSoundCueName))
            { SoundEffectManager.PlaySoundEffect(_apperanceSoundCueName); }
        }

        /// <summary>
        ///     リングの実行を終了する
        /// </summary>
        public virtual void End()
        {
            if (_isEnded) return; // 既に終了済みなら何もしない
            _isEnded = true;

            if (_tweens != null) //実行中のTweenを停止
            {
                foreach (var teen in _tweens)
                    teen?.Kill(true);
            }

            _onEndAction?.Invoke();
            
            // UIのリセット
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

        public void AddCount()
        {
            _count++;

            Effect(_count);
        }

        public virtual void Effect(int count)
        {
            //残り時間がないなら終了する
            if (!CheckRemainTime()) return;

			switch (count)
            {
                // 1拍目　縮小エフェクトを開始する
                case 1:
                    StartContractionEffect();
                    break;
                
                // 2拍目　念のため判定が始まる2拍目のタイミングでPlayerManagerのイベント購読を行う
                case 2:
                    Subscribe();
                    break;
            }
        }

        /// <summary>
        ///     残り時間があるか確認する
        /// </summary>
        /// <returns>残っていたらtrue、そうでないならfalse</returns>
        public bool CheckRemainTime()
        {
            //残り時間を計算
            var remainTime = (EffectLength - _count) * MusicEngineHelper.DurationOfBeat;

            //もしリングがアクティブな時にプレイヤーがスタン中なら収縮を辞める
            if (_player.IsStunning((float)remainTime + Time.time))
            {
                //もし既に非アクティブになっていたら終了
                if (!_chartRingManager.ActiveRingIndicators.Contains(this))
                {
                    return false;
                }

                End();
                return false;
            }

            return true;
        }
        
        #region ノーツの演出（チュートリアル用publicメソッド）
        
        /// <summary>
        /// 失敗演出
        /// NOTE: チャージのみ使用しているコンポーネントが多いためオーバーライドして実装が必要
        /// </summary>
        public virtual void PlayFailEffect()
        {
			Unsubscribe();

            // Tweens配列をクリア
            if (_tweens != null)
            {
                for (int i = 0; i < _tweens.Length; i++)
                {
                    _tweens[i]?.Kill();
                }
            }
            
            // 中央のImageのスプライトとサイズをMissのものに変える
            SetMissImage();
            
            var failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            failSequence.Append(CreateColorChangeSequence(Color.darkGray, Color.darkGray, _fadeDuration));
            failSequence.Join(CreateFadeSequence(_fadeDuration));
            
            failSequence.OnComplete(End);
            
            _tweens[0] = failSequence;
        }
        
        #endregion
        
        public virtual void Pause()
        {
            if (_tweens == null) return;
            foreach (var tween in _tweens)
            {
                tween?.Pause();
            }
        }

        public virtual void Resume()
        {
            if (_tweens == null) return;
            foreach (var tween in _tweens)
            {
                tween?.Play();
            }
        }
        
        #region 変数宣言

        [Header("基本設定")]
        [SerializeField] protected float _initialScale = 3.5f;
        [SerializeField] protected Vector3 _centerRingsScale = Vector3.one;
		[SerializeField] protected Vector3 _contractionScale = new Vector3(0.95f, 0.95f, 0.95f);
        [SerializeField] protected IndicatorSpriteDataSO _commonSprite; // Perfect/Good判定で色を変更するための白色リング

        [Header("リングのImageコンポーネントの設定")]
        [SerializeField] protected Image _hitImage; // 中央の太めのリング
        [SerializeField] protected Image _decorationImage; // デコレーションパーツ
        [SerializeField] protected Image _ringImage; // 収縮するリング

        [Header("色設定")]
        [SerializeField] protected RingIndicatorColorSO _colorSettings;

        [SerializeField] protected float _blinkDuration = 0.2f;
        [SerializeField] protected float _fadeDuration = 0.3f;

        [Header("中央の操作方法/判定UIの設定")]
        [SerializeField] protected Image _centerImage; // 操作方法・評価を表示するImage

        [SerializeField] protected HitResultData _guide; // 操作方法の画像の設定
        [SerializeField] protected HitResultSpriteSO _hitResult;

        [Header("SE")]
        [SerializeField] protected string _apperanceSoundCueName;

        protected PlayerManager _player;
        protected UIElement_ChartRingManager _chartRingManager;
        protected Action _onEndAction;

        protected Image _selfImage; // 自身のImageコンポーネント

        // 画像データ
        private Sprite _hitLine;
        private Sprite _decoration;
        private Sprite _ring;

        // 判定に合わせて適用する色を変えるための変数
        protected Color _newColor;
        protected Color _newTranslucentColor;
        protected bool _isEnded = false;
        protected int _timing;
        protected int _count;
        protected Tween[] _tweens;

        private Vector2 _defaultCenterImageSize; // 中央の画像素材のデフォルトのWidth/Height

        protected Color _defaultColor => _colorSettings.DefaultColor;
        protected Color _translucentDefaultColor => _colorSettings.TranslucentDefaultColor;

        // Justタイミングは2拍後
        private const float CONTRACTION_SPEED = 2;
        // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        private const float RECEPTION_TIME = 0.45f;
        
        #endregion
        
        private void Awake()
        {
            _selfImage = GetComponent<Image>();

            if (_ringImage == null)
            {
                // スクリプタブルオブジェクトで割り当てられていない場合のみ、子オブジェクトを取得
                _ringImage = transform.GetChild(0).GetComponent<Image>();
            }

            _defaultCenterImageSize = _centerImage.rectTransform.sizeDelta;
            
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

        /// <summary>
        /// Perfect/Goodの色変更用にSpriteを白色のものに変更する
        /// </summary>
        protected virtual void ChangeRingsImage()
        {
            // 色変更用にSpriteを白色のものに変更する
            _ringImage.sprite = _commonSprite.Ring;
            _decorationImage.sprite = _commonSprite.Decoration;
            _hitImage.sprite = _commonSprite.HitLine;
        }

		#region イベント購読・解除（継承先で実装する）

		/// <summary>
        /// イベント購読
        /// </summary>
		protected abstract void Subscribe();

		/// <summary>
        /// イベント購読解除
        /// </summary>
		protected abstract void Unsubscribe();

		#endregion
        
        #region 中央画像の操作（操作アイコン・判定の画像）
        
        /// <summary>
        /// 中央のイメージを操作する
        /// </summary>
        protected void HandleCenterImage(bool isPerfect)
        {
            var hitResult = isPerfect ? _hitResult.Perfect : _hitResult.Good;

            // 中央のImageのスプライト変更とサイズ変更
            _centerImage.sprite = hitResult.Sprite;
            _centerImage.rectTransform.sizeDelta = hitResult.SizeDelta;

            if (isPerfect)
            {
                _newColor = _colorSettings.PerfectColor;
                _newTranslucentColor = _colorSettings.TranslucentPerfectColor;
            }
            else
            {
                _newColor = _colorSettings.GoodColor;
                _newTranslucentColor = _colorSettings.TranslucentGoodColor;
            }
        }

        /// <summary>
        /// 中央のイメージをMiss判定のものに差し替える
        /// </summary>
        protected void SetMissImage()
        {
            _centerImage.sprite = _hitResult.Miss.Sprite;
            _centerImage.rectTransform.sizeDelta = _hitResult.Miss.SizeDelta;
        }
        
        #endregion
        
        #region ノーツの演出（protectedメソッド）

        /// <summary>
        /// 縮小演出
        /// </summary>
        protected virtual void StartContractionEffect()
        {
            // 一拍が何秒か、アニメーションのために値をキャッシュしておく
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;

            var contractionSequence = DOTween.Sequence()

                // Just判定まで縮小を行う
                .Append(_ringImage.rectTransform.DOScale(_contractionScale, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定を過ぎたら縮小は続行しつつ段々フェードアウトする
                .Append(_ringImage.rectTransform.DOScale(_contractionScale * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(CreateFadeSequence(beatDuration * RECEPTION_TIME))
                
                // シーケンスが中断されなかった場合はミス。失敗演出を行う
                .OnComplete(() => PlayFailEffect());
            
            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 1)
            {
                _tweens[0] = contractionSequence;
            }

            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_decorationImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_decorationImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 1)
            {
                _tweens[1] = blurPulseSequence;
            }
        }
        
        /// <summary>
        /// 成功エフェクト
        /// NOTE: スキルノーツ、チャージノーツはオーバーライドして実装
        /// </summary>
        protected virtual void OnPlayerSuccess(bool isPerfect)
        {
            if (MusicEngineHelper.GetBeatNearerSinceStart() != _timing)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }

			// 連続押し対策としてイベント購読を解除
			Unsubscribe();

            OnPlayerSuccessForced(isPerfect);
        }

        /// <summary>
        /// タイミングチェックなしで強制的に成功エフェクトを再生
        /// </summary>
        protected virtual void OnPlayerSuccessForced(bool isPerfect)
        {
            // 成功した場合はリングの縮小演出は不要になるのでキル
            if(_tweens != null)
            {
                _tweens[0]?.Kill();
            }
            
            if (isPerfect)
            {
                // パーフェクト判定の場合は収縮するリングのScaleを補正
                _ringImage.rectTransform.localScale = _contractionScale;
            }

            // 中央の画像を判定用の画像に変更
            HandleCenterImage(isPerfect);

            // 白色のSpriteに変更
            ChangeRingsImage();
            
            var successSequence = DOTween.Sequence();

            // パンチスケールと色変更
            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.65f, _blinkDuration, 2, 0.5f));
            successSequence.Join(CreateColorChangeSequence(_newColor, _newTranslucentColor, _fadeDuration));
            
            // フェードアウト
            successSequence.Append(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            successSequence.OnComplete(End);

            _tweens[0] = successSequence;
        }
        
        #endregion
        
        #region リングのコンポーネント全てのScale、色の初期化・リセット
        
        /// <summary>
        /// コンポーネントの初期化
        /// NOTE: スキル、チャージノーツはオーバーライドして実装
        /// </summary>
        protected virtual void InitializeComponents()
        {
            // 2種類のTweenを使用するため、配列も2つ分確保する
            _tweens = new Tween[2];
            
            // スケールと色を初期化
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }
        
        /// <summary>
        /// UIの初期化処理
        /// オブジェクトプールのOnGet()処理の中で呼び出される
        /// </summary>
        protected virtual void UIInitialized()
        {
            if (_hitLine == null && _decoration == null && _ring == null)
            {
                // 初期画像を取得する
                _hitLine = _ringImage.sprite;
                _decoration = _decorationImage.sprite;
                _ring = _hitImage.sprite;
            }

            // 中央のリングの画像を操作方法のものに差し替える
            _centerImage.sprite = _guide.Sprite;
            _centerImage.rectTransform.sizeDelta = _guide.SizeDelta;

            // デフォルトのスプライトを設定する
            _ringImage.sprite = _hitLine;
            _decorationImage.sprite = _decoration;
            _hitImage.sprite = _ring;
        }
        
        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        protected virtual void ResetRingsScale()
        {
            if(_ringImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            if(_decorationImage != null) _decorationImage.rectTransform.localScale = _centerRingsScale;
            if(_hitImage != null) _hitImage.rectTransform.localScale = _centerRingsScale;
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        protected virtual void ResetRingsColor(Color color, Color translucentColor)
        {
            if(_ringImage != null) _ringImage.color = color;
            if(_decorationImage != null) _decorationImage.color = color;
            if(_hitImage != null) _hitImage.color = color;
        }
        
        #endregion
        
        #region PlayerManagerのイベント登録用
        
        /// <summary>
        /// Perfect判定時の処理
        /// </summary>
        protected virtual void HandlePerfect(){ }
        
        /// <summary>
        /// Good判定時の処理
        /// </summary>
        protected virtual void HandleGood(){ }
        
        #endregion
        
        #region シーケンス作成メソッド（通常攻撃/回避で使用。スキル/チャージはオーバーライドしてそれぞれ独自処理を実装する）
        
        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        protected virtual DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
			
            fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_hitImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_decorationImage.DOFade(0f, duration).SetEase(Ease.Linear));
            // fadeSequence.Join(_centerImage.DOFade(0f, duration).SetEase(Ease.Linear));

            return fadeSequence;
        }

        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        protected virtual DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor, float duration)
        {
            var colorSequence = DOTween.Sequence();

            colorSequence.Join(_ringImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_hitImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_decorationImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
			
            return colorSequence;
        }
        
        #endregion
    }
}