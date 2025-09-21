using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using System;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_CountDown : MonoBehaviour
    {
        /// <summary>
        /// 敵のモデル変更なしで演出を行う
        /// </summary>
        public void StartGame()
        {
            if (_isPerforming) return;

            _isPerforming = true;
            Reset();
            StartPerform();
        }
        
        /// <summary>
        /// カウントダウン演出を開始する
        /// </summary>
        public void Play()
        {
            if (_isPerforming) return;

            _isPerforming = true;
            Reset();
            ModelsActive();
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
		[SerializeField] private Sprite _go;

		[SerializeField] private Vector2 _numberSpriteSize;
		[SerializeField] private Vector2 _goSpriteSize;

        [Header("アニメーションの設定")] 
        [SerializeField] private float _scaleAnimationDuration = 0.3f;

        [SerializeField] private float _fadeAnimationDuration = 0.2f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private Vector3 _maxScale = new Vector3(1.5f, 1.5f, 1f);
        [SerializeField] private Vector3 _normalScale = Vector3.one;

        private BGMManager _bgmManager;
        private PlayerManager _playerManager;
        private int _counter;
        private bool _isPerforming = false;
        private DG.Tweening.Sequence _currentSequence;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;

        private int _bgmStartTiming = 1;

        #region Life cycle
        
        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            InitializeComponents();
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            
            // BGMが切り替わったときのイベントを購読
            _bgmManager.OnBGMChanged += OnBGMChanged;
        }

        private void OnBGMChanged(string phaseName)
        {
            // カウントをリセット
            _bgmStartTiming = 1;

            _bgmManager.OnJustChangedBeat += OnCount;
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
                _bgmManager.OnJustChangedBeat -= OnCount;
                _bgmManager.OnBGMChanged -= OnBGMChanged;
            }
        }
        
        #endregion

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
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

			// Imageの比率を数字のものに設定
			_selfImage.rectTransform.sizeDelta = _numberSpriteSize;
        }

        /// <summary>
        /// プレイヤーと敵のモデルを表示する
        /// NOTE: フェーズ1は既にチュートリアルで表示済みなのでこのメソッドを呼ぶ必要はない
        /// </summary>
        private void ModelsActive()
        {
            // プレイヤーのモデルを表示
            _playerManager.ModelActive();
            
            //次の敵をアクティブ化する
            var enemyAdmin = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
            if (enemyAdmin)
            {
                enemyAdmin.NextEnemyActive();
            }
        }

        /// <summary>
        /// BGMが始まって何拍目かカウント
        /// </summary>
        private void OnCount()
        {
            _bgmStartTiming = Music.Just.Bar * 4 + Music.Just.Beat - 2;
            //Debug.LogWarning(_bgmStartTiming);
        }
        
        /// <summary>
        /// 演出開始
        /// </summary>
        private void StartPerform()
        {
            // ビートの更新イベントを購読
            _bgmManager.OnJustChangedBeat += OnBeatTrigger;
        }

        /// <summary>
        /// 拍更新時の処理
        /// </summary>
        private void OnBeatTrigger()
        {
            if (!_isPerforming) return;
            
            // 16拍区切りの13拍目から開始（3, 2, 1で3拍使用し、16拍目で終了）
            int currentBeat = _bgmStartTiming % 16;
    
			if (_counter == 0 && currentBeat == 10)
            {
                ChangeImage(); // 3を表示
            }
            else if (_counter == 1 && currentBeat == 11)
            {
                ChangeImage(); // 2を表示
            }
            else if (_counter == 2 && currentBeat == 12)
            {
                ChangeImage(); // 3を表示  
            }
            else if (_counter == 3 && currentBeat == 13)
            {
                ChangeImage(); // GOを表示
            }
            else if (_counter == 4 && currentBeat == 14)
            {
                EndPerform();
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
			if(_counter >= 4)
			{
				// Counterが4以上のときはGoの表示で、画像の比率を変える必要がある
				_selfImage.rectTransform.sizeDelta = _goSpriteSize;
			}

            if (targetSprite != null)
            {
                _selfImage.sprite = targetSprite;
                PlayCountDownAnimation();
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
            if (_counter < 4)
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
            _bgmManager.OnJustChangedBeat -= OnCount;
            
            _isPerforming = false;
            
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
            _rectTransform.anchoredPosition = _originalPosition;
        	_selfImage.rectTransform.sizeDelta = _numberSpriteSize;	
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
				4 => _go,
                _ => null
            };
        }
    }
}