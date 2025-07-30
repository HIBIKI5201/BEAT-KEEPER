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
        }

        public void OnGet(Action onEndAction, Vector2 rectPos, int timing)
        {
            _selfImage.rectTransform.position = rectPos
                + new Vector2(Screen.width / 2, Screen.height / 2);
            _centerImage.rectTransform.sizeDelta = _defaultCenterImageSize;
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
            if (_tweens != null) //実行中のTweenを停止
            {
                foreach (var teen in _tweens)
                    teen?.Kill();
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

        [Header("色設定")]
        [SerializeField] protected Color _successColor = Color.yellow;
        [SerializeField] protected Color _translucentSuccessColor = Color.yellow; // 半透明の成功色
        [SerializeField] protected Color _defaultColor = Color.white;
        [SerializeField] protected Color _translucentDefaultColor = Color.white; // 半透明のデフォルト色

        [SerializeField] protected float _blinkDuration = 0.2f;
        [SerializeField] protected float _fadeDuration = 0.3f;

		[Header("中央の操作方法/判定UIの設定")]
		[SerializeField] protected Image _centerImage; // 操作方法・評価を表示するImage
		[SerializeField] protected HitResultSpriteSO _hitResult;

        [Header("SE")]
        [SerializeField] protected string _apperanceSoundCueName;

        protected PlayerManager _player;
        protected UIElement_ChartRingManager _chartRingManager;
        protected Action _onEndAction;

        protected Image _selfImage;
        protected Image _ringImage;

        protected int _timing;
        protected int _count;
        protected Tween[] _tweens;
        
        private Vector2 _defaultCenterImageSize; // 中央の画像素材のデフォルトのWidth/Height

        private void Awake()
        {
            _selfImage = GetComponent<Image>();
            _ringImage = transform.GetChild(0).GetComponent<Image>();
			_defaultCenterImageSize = _centerImage.rectTransform.sizeDelta;
        }
    }
}