using System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.Stage;
using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// 敵の攻撃警告UI
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIElement_AttackWarningIndicator : MonoBehaviour
    {
        [Header("基本設定")] 
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private float _initialScale = 3.5f;
        [Space]
        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField, Tooltip("リングの縮小時間の拍数")] private int _reductionTime = 3;

        [Header("色設定")]
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;
        
        private Image _image;
        private MusicEngineHelper _musicEngineHelper; // タイミング調整用

        private PlayerManager _playerManager;
        private StageEnemyAdmin _enemies;

        private ObjectPool<Image> _ringPool;

        private int _thickRefCount;
        
        private const float DURATION = 0.57f; // BPM210 / 2の時間 

        private void Awake()
        {
            _ringPool = new ObjectPool<Image>(
                createFunc: () =>
                {
                    var go = new GameObject("Ring");
                    go.transform.SetParent(transform);
                    var image = go.AddComponent<Image>();
                    image.sprite = _ringSprite;
                    image.rectTransform.sizeDelta = Vector2.one * 250;
                    image.enabled = false;
                    return image;
                },
                actionOnGet: image =>
                {
                    image.enabled = true;
                    image.rectTransform.localScale = Vector3.one * _initialScale;
                },
                actionOnRelease: image => image.enabled = false,
                defaultCapacity: 5,
                maxSize: 10);
        }

        private async void Start()
        {
            _image = GetComponent<Image>();
            _image.color = new Color(1,1,1,0);
            _playerManager = ServiceLocator.GetInstance<PlayerManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();

            SceneLoader.RegisterAfterSceneLoad(SceneListEnum.Battle.ToString(),
                () =>
                {
                    _enemies = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
                    _musicEngineHelper.OnJustChangedBeat += OnBeat;
                });
        }
        
        private void OnDestroy()
        {
            _musicEngineHelper.OnJustChangedBeat -= OnBeat;
        }

        private void OnBeat()
        {
            if (_enemies == null) return;
            
            var timing = _musicEngineHelper.GetCurrentTiming();
            foreach (var enemy in _enemies.Enemies)
            {
                if (enemy.Data.IsAttack(timing.Bar * 4 + timing.Beat + _reductionTime + 2))
                {
                    EffectStart(_ringPool.Get());
                }
            }
        }

        private async void EffectStart(Image ring)
        {
            ring.rectTransform.anchoredPosition = Vector2.zero;
            _thickRefCount++;
            
            var effectSequence = DOTween.Sequence();

            // 赤く点滅する
            effectSequence.Append(_image.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Restart));
            effectSequence.Join(ring.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Restart));

            // 元の色に戻す
            effectSequence.Append(_image.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
            effectSequence.Join(ring.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
            
            await Awaitable.WaitForSecondsAsync((float)_musicEngineHelper.DurationOfBeat * 2, destroyCancellationToken);
            
            ring.rectTransform.DOScale(Vector3.one, (float)_musicEngineHelper.DurationOfBeat * 2).SetEase(Ease.Linear); //段々小さく
            
            await Awaitable.WaitForSecondsAsync((float)_musicEngineHelper.DurationOfBeat * _reductionTime, destroyCancellationToken);
            
            // パルスエフェクト追加
            effectSequence.Append(ring.rectTransform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f)
                .SetLoops(3, LoopType.Restart));

            // 円を黄色に光らせる
            effectSequence.Join(ring.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            effectSequence.Join(_image.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));

            // フェードしながら消える
            effectSequence.Join(ring.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));

            _thickRefCount--;
            if (_thickRefCount <= 0)
            {
                effectSequence.Join(_image.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));
            }
        }
    }
}