using System.Threading.Tasks;
using BeatKeeper.Runtime.Ingame.Character;
using DG.Tweening;
using R3;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
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
        [SerializeField] private CanvasGroup _overlayCanvasGroup; // フローゾーン突入時のオーバーレイ
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        private void Start()
        {
            _playerManager = ServiceLocator.GetInstance<PlayerManager>();

            Task.Run(async () =>
            {
                await SymphonyTask.WaitUntil(() => _playerManager.FlowZoneSystem != null);
                
                _playerManager.FlowZoneSystem.ResonanceCount.Subscribe(IconColorChanged).AddTo(_disposable);
                _playerManager.FlowZoneSystem.IsFlowZone.Subscribe(value =>
                {
                    _overlayCanvasGroup.DOFade(value ? 1 : 0, 0.15f);
                
                    if (!value)
                    {
                        AllReset();
                    }
                }).AddTo(_disposable);
            });
            
            AllReset();
        }

        /// <summary>
        /// リズム共鳴回数に合わせてアイコンの色を変更する
        /// </summary>
        private void IconColorChanged(int count)
        {
            if (count < 0 || count >= _icons.Length)
            {
                Debug.LogError("[リズム共鳴メーター] 共鳴回数の範囲外です");
                return;
            }
            
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
