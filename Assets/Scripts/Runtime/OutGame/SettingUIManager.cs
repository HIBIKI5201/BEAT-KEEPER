using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BeatKeeper
{
    public class SettingUIManager : MonoBehaviour
    {
        /// <summary>
        /// 現在選択中のボタンのインデックス
        /// </summary>
        public int CurrentButtonIndex => _currentButtonIndex;

        /// <summary>
        /// デフォルトボタンを選択状態に
        /// </summary>
        public void Setup()
        {
            _currentButtonIndex = _defaultButtonIndex;
            
            // ボタン配列の長さに合わせて作成
            _buttonTexts = new Text[_buttons.Length][];
            
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttonTexts[i] = _buttons[i].gameObject.GetComponentsInChildren<Text>();
            }
            
            UpdateButtonSelection();
        }

        /// <summary>
        /// ボタン切り替え
        /// </summary>
        public void MoveSelection(int direction)
        {
            int newIndex = ((_currentButtonIndex + direction) % _buttons.Length + _buttons.Length) % _buttons.Length;
            _currentButtonIndex = newIndex;
            UpdateButtonSelection();
        }
        
        [SerializeField] private Button[] _buttons;
        [SerializeField] private int _defaultButtonIndex = 0;
        [SerializeField] private float _selectedScaleMultiplier = 1.05f;
        
        [Header("色設定")]
        [SerializeField] private Color _selectedTextColor = Color.gray;
        [SerializeField] private Color _defaultTextColor = Color.white;
        
        private int _currentButtonIndex = 0;
        private Text[][] _buttonTexts; // 各ボタンに対応する複数のTextコンポーネント

        /// <summary>
        /// ボタンの選択状態
        /// </summary>
        private void UpdateButtonSelection()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] == null) continue;
                
                var isSelected = (i == _currentButtonIndex);
                
                // Scale、色の変更
                var targetScale = isSelected ? _selectedScaleMultiplier : 1f;
                var targetColor = isSelected ? _selectedTextColor : _defaultTextColor;
                
                _buttons[i].transform.localScale = Vector3.one * targetScale;
                _buttons[i].image.color = isSelected ? _selectedTextColor : _defaultTextColor;
                
                if (_buttonTexts[i] != null)
                {
                    foreach (var text in _buttonTexts[i])
                    {
                        if (text != null)
                        {
                            text.color = targetColor;
                        }
                    }
                }
            }
        }
    }
}
