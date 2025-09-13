using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using BeatKeeper.Runtime.Ingame.Battle;
using System;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_CountDown : MonoBehaviour
    {
        /// <summary>
        ///　ブレイクムービー終了通知イベント
        /// </summary>
        public event Action OnMovieFinished;
        
        /// <summary>
        /// カウントダウン演出を開始する
        /// </summary>
        public void Play()
        {
            Debug.LogError("Play");
            if (_isPerforming) return;

            _isPerforming = true;
            Reset();
            StartPerform();
        }
        
        /// <summary>
        /// 演出を強制停止する
        /// </summary>
        public void Stop()
        {
            if (!_isPerforming) return;

            _currentSequence?.Kill();
            EndPerform();
            Reset();
        }
        
        [SerializeField] private Image _selfImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("画像設定")] 
        [SerializeField] private Sprite _number1;
        [SerializeField] private Sprite _number2;
        [SerializeField] private Sprite _number3;

        [Header("アニメーションの設定")] 
        [SerializeField] private float _scaleAnimationDuration = 0.3f;

        [SerializeField] private float _fadeAnimationDuration = 0.2f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Vector3 _maxScale = new Vector3(1.5f, 1.5f, 1f);
        [SerializeField] private Vector3 _normalScale = Vector3.one;

        [Header("Effects")] 
        [SerializeField] private Color _normalColor = Color.white;

        private BGMManager _bgmManager;
        private int _counter;
        private bool _isPerforming = false;
        private DG.Tweening.Sequence _currentSequence;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;

        #region Life cycle
        
        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            InitializeComponents();
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();

            ServiceLocator.SetInstance(this, ServiceLocator.LocateType.Locator);
        }
        
        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            // メモリリーク防止
            _currentSequence?.Kill();
            if (_bgmManager != null)
            {
                _bgmManager.OnJustChangedBeat -= OnBeatTrigger;
            }
        }
        
        #endregion

        private void InitializeComponents()
        {
            if (_selfImage == null)
            {
                _selfImage = GetComponent<Image>();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            // キャッシュを取得
            _rectTransform = transform as RectTransform;
            _originalPosition = _rectTransform.anchoredPosition;

            // 初期状態を設定
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 演出開始
        /// </summary>
        private void StartPerform()
        {
            OnMovieFinished?.Invoke();
            
            //次の敵をアクティブ化する
            var enemyAdmin = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
            if (enemyAdmin)
            {
                enemyAdmin.NextEnemyActive();
            }
            
            
            // ビートの更新イベントを購読
            _bgmManager.OnJustChangedBeat += OnBeatTrigger;
        }

        /// <summary>
        /// 拍更新時の処理
        /// </summary>
        private void OnBeatTrigger()
        {
            if (!_isPerforming) return;

            int currentBeat = MusicEngineHelper.GetBeatSinceStart();
            
            if (_counter == 0 && currentBeat % 4 == 0)
            {
                // 4で割り切れる拍から始める
                ChangeImage();
            }
            else if (_counter > 0 && _counter < 3)
            {
                // 既に演出が始まっている場合
                ChangeImage();
            }
        }

        /// <summary>
        /// スプライトを変更する
        /// </summary>
        private void ChangeImage()
        {
            _counter++;

            // 現在のアニメーションを停止
            _currentSequence?.Kill();

            // 現在のカウントのスプライトを入手
            Sprite targetSprite = GetSpriteByCounter(_counter);
            if (targetSprite != null)
            {
                _selfImage.sprite = targetSprite;
                PlayCountDownAnimation();
            }

            // 最後の数字の後は終了処理
            if (_counter >= 3)
            {
                DOVirtual.DelayedCall(_scaleAnimationDuration + 0.1f, EndPerform);
            }
        }

        /// <summary>
        /// カウントダウンアニメーション
        /// </summary>
        /// <returns></returns>
        private void PlayCountDownAnimation()
        {
            transform.localScale = _maxScale;
            
            _currentSequence = DOTween.Sequence();

            // フェードイン
            _currentSequence.Append(
                DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, _fadeAnimationDuration)
            );

            // スケールダウン
            _currentSequence.Join(
                transform.DOScale(_normalScale, _scaleAnimationDuration * 0.4f)
                    .SetEase(Ease.OutBounce)
            );

            // 最後の数字以外はフェードアウト
            if (_counter < 3)
            {
                _currentSequence.Append(
                    DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0f, _fadeAnimationDuration)
                        .SetDelay(0.1f)
                );
            }
            else
            {
                // 最後の数字は少し長めに表示してからフェードアウト
                _currentSequence.AppendInterval(0.3f);
                _currentSequence.Append(
                    DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0f, _fadeAnimationDuration * 2f)
                );
            }
        }

        /// <summary>
        /// 演出終了時の処理
        /// </summary>
        private void EndPerform()
        {
            _bgmManager.OnJustChangedBeat -= OnBeatTrigger;
            _isPerforming = false;

            // ポジションリセット
            _rectTransform.anchoredPosition = _originalPosition;
            
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.TransitionTo(PhaseEnum.Battle);
            }
        }

        /// <summary>
        /// リセット
        /// </summary>
        private void Reset()
        {
            _counter = 0;
            _selfImage.sprite = _number3;
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            _selfImage.color = _normalColor;
            _rectTransform.anchoredPosition = _originalPosition;
        }
        
        /// <summary>
        /// 引数に合わせてスプライトを取得
        /// </summary>
        private Sprite GetSpriteByCounter(int counter)
        {
            return counter switch
            {
                1 => _number3,
                2 => _number2,
                3 => _number1,
                _ => null
            };
        }

#if UNITY_EDITOR
        [ContextMenu("Test Play")]
        private void TestPlay()
        {
            Play();
        }
#endif
    }
}