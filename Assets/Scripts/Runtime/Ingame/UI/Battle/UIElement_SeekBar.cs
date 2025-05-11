using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// 画面下部のシークバーを管理するクラス
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIElement_SeekBar : MonoBehaviour
    {
        private float _playTime; // 曲全体の時間
        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
        }

        /// <summary>
        /// シークバーの初期化
        /// </summary>
        public void Initialize()
        {
            _image.fillAmount = 1;
            _playTime = (float)Music.CurrentTempo * 131; // TODO: 仮置き。曲全体の時間をうまく自動計算できるようにしたい
            
            DOTween.Kill(this);
            DOTween.To(() => _image.fillAmount, x => _image.fillAmount = x, 0, _playTime);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}
