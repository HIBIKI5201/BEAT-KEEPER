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

        [Header("基本設定")]
        [SerializeField] protected float _initialScale = 3.5f;
        [SerializeField] protected Vector3 _centerRingsScale = Vector3.one;
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


        private void Awake()
        {
            _selfImage = GetComponent<Image>();

            if (_ringImage == null)
            {
                // スクリプタブルオブジェクトで割り当てられていない場合のみ、子オブジェクトを取得
                _ringImage = transform.GetChild(0).GetComponent<Image>();
            }

            _defaultCenterImageSize = _centerImage.rectTransform.sizeDelta;
        }

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
        /// Perfect/Goodの色変更用にSpriteを白色のものに変更する
        /// </summary>
        protected virtual void ChangeRingsImage()
        {
            // 色変更用にSpriteを白色のものに変更する
            _ringImage.sprite = _commonSprite.Ring;
            _decorationImage.sprite = _commonSprite.Decoration;
            _hitImage.sprite = _commonSprite.HitLine;
        }
    }
}