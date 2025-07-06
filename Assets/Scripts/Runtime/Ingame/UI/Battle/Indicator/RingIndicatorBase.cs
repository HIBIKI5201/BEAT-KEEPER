using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
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

        public void OnGet(Action onEndAction, Vector2 rectPos)
        {
            _selfImage.rectTransform.position = rectPos
                + new Vector2(Screen.width / 2, Screen.height / 2);
            _onEndAction = onEndAction;

            _count = 0;

            CheckRemainTime();
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

        protected PlayerManager _player;
        protected UIElement_ChartRingManager _chartRingManager;
        protected Action _onEndAction;

        protected Image _selfImage;
        protected Image _ringImage;

        protected int _count;
        protected Tween[] _tweens;

        private void Awake()
        {
            _selfImage = GetComponent<Image>();
            _ringImage = transform.GetChild(0).GetComponent<Image>();
        }
    }
}