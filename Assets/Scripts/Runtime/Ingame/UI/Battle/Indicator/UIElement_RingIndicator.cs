using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// 敵の攻撃警告UI
    /// </summary>
    [RequireComponent(typeof(Image))]
    public abstract class UIElement_RingIndicator : MonoBehaviour
    {
        public abstract int EffectLength { get; }

        [Header("基本設定")]
        [SerializeField] protected Sprite _ringSprite;
        [SerializeField] protected float _initialScale = 3.5f;

        protected Action _onEndAction;

        protected Image _selfImage;
        protected Image _ringImage;

        protected int _count;

        private void Awake()
        {
            _selfImage = GetComponent<Image>();
            _ringImage = transform.GetChild(0).GetComponent<Image>();
        }

        public void OnGet(Action onEndAction, Vector2 rectPos)
        {
            _selfImage.rectTransform.position = rectPos 
                + new Vector2(Screen.width / 2, Screen.height / 2);
            _onEndAction = onEndAction;

            _count = 0;
        }

        public void AddCount()
        {
            _count++;

            Effect(_count);
        }

        public abstract void Effect(int count);
    }
}