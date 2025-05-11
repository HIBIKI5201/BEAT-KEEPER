using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// 必殺技ゲージクラス
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIElement_SpecialMoveIcon : MonoBehaviour
    {
        private Image _image;

        public void Initialize()
        {
            _image = GetComponent<Image>();
        }

        /// <summary>
        /// ゲージを更新する
        /// </summary>
        public void FillUpdate(float value)
        {
            _image.fillAmount = value;
        }
    }
}
