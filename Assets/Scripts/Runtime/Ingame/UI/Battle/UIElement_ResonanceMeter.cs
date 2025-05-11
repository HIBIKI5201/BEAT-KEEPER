using BeatKeeper.Runtime.Ingame.Character;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// リズム共鳴アイコンを管理するクラス
    /// </summary>
    public class UIElement_ResonanceMeter : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private Color _defaultColor = Color.black;
        [SerializeField] private Color _resonanceColor = Color.yellow;
        [SerializeField] private Image[] _icons;
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        private void Start()
        {
            _playerManager.FlowZoneSystem.ResonanceCount.Subscribe(IconColorChanged).AddTo(_disposable);
            _playerManager.FlowZoneSystem.IsFlowZone.Subscribe(value => { if(!value) AllReset(); }).AddTo(_disposable);
            AllReset();
        }

        /// <summary>
        /// リズム共鳴回数に合わせてアイコンの色を変更する
        /// </summary>
        private void IconColorChanged(int count)
        {
            _icons[count].color = _resonanceColor;
        }

        /// <summary>
        /// アイコンを全て暗い色に設定する
        /// </summary>
        private void AllReset()
        {
            foreach (var icon in _icons)
            {
                icon.color = _defaultColor;
            }
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}
