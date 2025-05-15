using BeatKeeper.Runtime.Ingame.Character;
using DG.Tweening;
using R3;
using SymphonyFrameWork.System;
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
        [SerializeField] private float _animationDuration = 0.5f; // アニメーションの長さ（秒）
        
        private Image _image;
        private Tweener _currentTween;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private void Start()
        {
            _playerManager = ServiceLocator.GetInstance<PlayerManager>();
            
            _image = GetComponent<Image>();
            _playerManager.SpecialSystem.SpecialEnergy.Subscribe(FillUpdate).AddTo(_disposable);
        }

        /// <summary>
        /// ゲージを徐々に更新する
        /// </summary>
        private void FillUpdate(float value)
        {
            _currentTween?.Kill();
            _currentTween = _image.DOFillAmount(value, _animationDuration).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _disposable.Dispose();
        }
    }
}