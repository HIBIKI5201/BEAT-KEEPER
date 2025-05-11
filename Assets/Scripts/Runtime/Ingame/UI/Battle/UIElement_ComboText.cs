using BeatKeeper.Runtime.Ingame.Character;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// コンボのテキストを管理するクラス
    /// </summary>
    [RequireComponent(typeof(Text), typeof(CanvasGroup))]
    public class UIElement_ComboText : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager; // ComboSystem取得用
        private Text _text;
        private CanvasGroup _canvasGroup;
        private CompositeDisposable _disposables = new CompositeDisposable();
        
        private void Start()
        {
            _playerManager = ServiceLocator.GetInstance<PlayerManager>();
            
            _text = GetComponent<Text>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _playerManager.ComboSystem.ComboCount.Subscribe(UpdateText).AddTo(_disposables);
        }

        /// <summary>
        /// テキストを更新する
        /// </summary>
        private void UpdateText(int value)
        {
            if (value == 0)
            {
                Hide(); // コンボがゼロになったら非表示にする
            }
            else if (value != 0 && _canvasGroup.alpha == 0)
            {
                Show(); // コンボがゼロ以外で、かつ非表示状態だったら表示処理を行う
            }
            
            _text.text = $"{value} COMBO!";
        }
        
        private void Show() => _canvasGroup.alpha = 1;
        
        private void Hide() => _canvasGroup.alpha = 0;

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
