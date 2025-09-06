using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BeatKeeper
{
    public class SettingUIManager : MonoBehaviour
    {
        [SerializeField] private Button[] _buttons;
        [SerializeField] private int _defaultButtonIndex = 0;
        [SerializeField] private float _selectedScaleMultiplier = 1.05f;
        
        private int _currentButtonIndex = 0;
        
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

        /// <summary>
        /// ボタンの選択状態
        /// </summary>
        private void UpdateButtonSelection()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] == null) continue;
                
                var isSelected = (i == _currentButtonIndex);
                var targetScale = isSelected ? _selectedScaleMultiplier : 1f;
                _buttons[i].transform.localScale = Vector3.one * targetScale;
            }
        }
    }
}
