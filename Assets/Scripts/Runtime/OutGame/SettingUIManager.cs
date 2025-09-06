using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BeatKeeper
{
    public class SettingUIManager : MonoBehaviour
    {
        [SerializeField] private Button[] _buttons;
        [SerializeField] private int _defaultButtonIndex = 0;
        
        private int _currentButtonIndex = 0;
        
        /// <summary>
        /// 現在選択中のボタンのインデックス
        /// </summary>
        public int CurrentButtonIndex => _currentButtonIndex;

        public void Setup()
        {
            _currentButtonIndex = _defaultButtonIndex;
            Select();
        }

        public void Next(int direction)
        {
            int newIndex = ((_currentButtonIndex + direction) % _buttons.Length + _buttons.Length) % _buttons.Length;
            _currentButtonIndex = newIndex;
            Select();
        }

        private void Select()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].transform.localScale = Vector3.one;
                
                if (i == _currentButtonIndex)
                {
                    _buttons[i].transform.localScale = Vector3.one * 1.05f;
                }
            }
        }
    }
}
