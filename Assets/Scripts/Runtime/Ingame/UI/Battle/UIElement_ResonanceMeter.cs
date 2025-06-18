using BeatKeeper.Runtime.Ingame.Character;
using DG.Tweening;
using JetBrains.Annotations;
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
        [SerializeField] private GameObject _meterPrefab;
        [SerializeField] private Color _defaultColor = Color.black;
        [SerializeField] private Color _resonanceColor = Color.yellow;
        [SerializeField] private Image[] _icons;
        [SerializeField] private CanvasGroup _overlayCanvasGroup; // フローゾーン突入時のオーバーレイ

        private PlayerManager _playerManager;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private async void Start()
        {
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();

            await SymphonyTask.WaitUntil(() => _playerManager.FlowZoneSystem != null);

            Initialize();
            GenerateMetar();
            AllReset();
        }

        private void Initialize()
        {
            _playerManager.FlowZoneSystem.ResonanceCount.Subscribe(IconColorChanged).AddTo(_disposable);
            _playerManager.FlowZoneSystem.IsFlowZone.Subscribe(value =>
            {
                _overlayCanvasGroup.DOFade(value ? 1 : 0, 0.15f);

                if (!value)
                {
                    AllReset();
                }
            }).AddTo(_disposable);
        }

        private void GenerateMetar()
        {
            if (_playerManager == null) return;
            if (_meterPrefab == null) return;

            _icons = new Image[FlowZoneSystem.MAX_COUNT];

            for (int i = 0; i < FlowZoneSystem.MAX_COUNT; i++)
            {
                GameObject go = Instantiate(_meterPrefab, transform);
                if (go.TryGetComponent(out Image image)) //生成したオブジェクトからイメージを取得
                {
                    _icons[i] = image;
                }
                else
                {
                    _icons[i] = go.AddComponent<Image>();
                }
            }
        }

        /// <summary>
        /// リズム共鳴回数に合わせてアイコンの色を変更する
        /// </summary>
        private void IconColorChanged(int count)
        {
            count--; // カウントが1オリジンで渡ってくるので、1減らす処理を挟む

            if (count < -1 || count >= _icons.Length) // 0-6の範囲に収めたい
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