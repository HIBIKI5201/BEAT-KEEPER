using DG.Tweening;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// キャンバスコントローラの表示・非表示を操作するためのクラス
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasController : MonoBehaviour
    {
        [SerializeField] private MovePattern _movePattern;
        [SerializeField] private float _showDuration = 1f;
        [SerializeField] private float _hideDuration = 0.5f;
        [SerializeField] private float _slideDistance = 100f; // スライドする距離
        private CanvasGroup _canvasGroup;
        private Vector3 _defaultPosition;
        private Vector3 _hiddenPosition;
        

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _defaultPosition = transform.localPosition;
            _hiddenPosition = CalculateHiddenPosition(); // 非表示時の位置を計算
            SetHiddenState();
        }
        
        public void Show()
        {
            _canvasGroup.DOFade(1f, _showDuration);
            transform.DOLocalMove(_defaultPosition, _showDuration).SetEase(Ease.OutQuad);
        }

        public void Hide()
        {
            _canvasGroup.DOFade(0f, _hideDuration);
            transform.DOLocalMove(_hiddenPosition, _hideDuration).SetEase(Ease.InQuad);
        }
        
        /// <summary>
        /// 非表示時の位置を計算
        /// </summary>
        private Vector3 CalculateHiddenPosition()
        {
            Vector3 hiddenPos = _defaultPosition;
            
            switch (_movePattern)
            {
                case MovePattern.TopToBottom:
                    hiddenPos.y = _defaultPosition.y + _slideDistance;
                    break;
                case MovePattern.BottomToTop:
                    hiddenPos.y = _defaultPosition.y - _slideDistance;
                    break;
                case MovePattern.RightToLeft:
                    hiddenPos.x = _defaultPosition.x + _slideDistance;
                    break;
            }
            
            return hiddenPos;
        }
        
        /// <summary>
        /// 非表示状態に初期化
        /// </summary>
        private void SetHiddenState()
        {
            _canvasGroup.alpha = 0f;
            transform.localPosition = _hiddenPosition;
        }

        /// <summary>
        /// UIをスライドさせる向きの列挙型
        /// </summary>
        private enum MovePattern
        {
            TopToBottom, // 上から下
            BottomToTop, // 下から上
            RightToLeft, // 右から左
        }
    }
}
