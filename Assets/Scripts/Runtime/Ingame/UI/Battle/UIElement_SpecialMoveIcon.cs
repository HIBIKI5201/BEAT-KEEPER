using BeatKeeper.Runtime.Ingame.Character;
using R3;
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
        [SerializeField] private PlayerManager _playerManager; // SpecialSystem取得用
        private Image _image;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private void Start()
        {
            _image = GetComponent<Image>();
            _playerManager.SpecialSystem.SpecialEnergy.Subscribe(FillUpdate).AddTo(_disposable);
        }

        /// <summary>
        /// ゲージを更新する
        /// </summary>
        public void FillUpdate(float value)
        {
            _image.fillAmount = value;
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}
