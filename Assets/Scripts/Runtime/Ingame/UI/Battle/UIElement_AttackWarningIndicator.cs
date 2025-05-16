using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// 敵の攻撃警告UI
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIElement_AttackWarningIndicator : MonoBehaviour
    {
        [Header("基本設定")] 
        [SerializeField] private float _initialScale = 3.5f;
        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        [Header("色設定")]
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;
        
        private Image _image;
        private Image _childImage; // 子オブジェクト（Scaleを調整する方）のImage
        private CanvasGroup _canvasGroup;
        private MusicEngineHelper _musicEngineHelper; // タイミング調整用
        
        private Sequence _effectSequence; // 警告時の赤い明滅を行うシーケンス

        private bool _processEveryOtherBeat; // 2拍に1回のみ処理を行うための変数
        private int _beatCount; // 現在の拍数

        private const float DURATION = 0.57f; // BPM210 / 2の時間 
        
        private void Start()
        {
            _image = GetComponent<Image>();
            _childImage = transform.GetChild(0).GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
        }

        [ContextMenu("エフェクト")]
        public void EffectStart()
        {
            if (_musicEngineHelper == null)
            {
                Debug.LogError("MusicEngineHelperが初期化されていません");
                return;
            }
            
            _beatCount = 0;
            _processEveryOtherBeat = false;
            _musicEngineHelper.OnJustChangedBeat += ProcessBeat; // ビートイベントの購読を開始して演出を始める
        }

        /// <summary>
        /// ビート処理 - 2拍ごとに実行
        /// </summary>
        private void ProcessBeat()
        {
            // 2拍に1回処理を行う TODO: BPM次第で後で変更になるかも
            _processEveryOtherBeat = !_processEveryOtherBeat;
            if (!_processEveryOtherBeat) return;
            
            _beatCount++;

            switch (_beatCount)
            {
                case 1: 
                    ShowInitialWarning(); 
                    break;
                case 3:
                    ShrinkWarning();
                    break;
                case 5:
                    CompleteWarning();
                    break;
            }
        }

        /// <summary>
        /// 1拍目で表示
        /// </summary>
        private void ShowInitialWarning()
        {
            ResetVisuals();   
            
            _effectSequence?.Kill(); // 進行中のシーケンスがあればKillしておく
            
            _effectSequence = DOTween.Sequence();
            
            // 赤く点滅する
            _effectSequence.Append(_image.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Restart));
            _effectSequence.Join(_childImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Restart));
            
            // 元の色に戻す
            _effectSequence.Append(_image.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
            _effectSequence.Join(_childImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
        }

        /// <summary>
        /// 3拍目・4拍目で収縮
        /// </summary>
        private void ShrinkWarning()
        {
            _childImage.transform.DOScale(Vector3.one, DURATION * 2).SetEase(Ease.Linear);
        }

        /// <summary>
        /// 5拍目で収縮終了+フェードしながら消えていく(攻撃タイミング)
        /// </summary>
        private void CompleteWarning()
        {
            _effectSequence?.Kill();
            _musicEngineHelper.OnJustChangedBeat -= ProcessBeat; // 次のエフェクト表示タイミングまで処理を行いたくないので購読を解除する
            _childImage.transform.localScale = Vector3.one;
            Success();
            
        }

        /// <summary>
        /// 表示のリセット
        /// </summary>
        private void ResetVisuals()
        {
            _canvasGroup.alpha = 1;
            _childImage.transform.localScale = Vector3.one * _initialScale;
            _image.color = _defaultColor;
            _childImage.color = _defaultColor;
        }
        
        /// <summary>
        /// ジャスト回避に成功した時のエフェクト
        /// </summary>
        private void Success()
        {
            // パルスエフェクト追加
            _effectSequence.Append(_childImage.transform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f).SetLoops(3, LoopType.Restart));
            
            // 円を黄色に光らせる
            _effectSequence.Join(_childImage.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            _effectSequence.Join(_image.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            
            // フェードしながら消える
            _effectSequence.Join(_canvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));
        }

        private void OnDestroy()
        {
            _musicEngineHelper.OnJustChangedBeat -= ProcessBeat;
        }
    }
}
