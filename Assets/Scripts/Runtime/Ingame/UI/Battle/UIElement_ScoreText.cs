using UnityEngine;
using UnityEngine.UI;
using R3;
using DG.Tweening; // DOTweenの名前空間を追加

namespace BeatKeeper
{
    /// <summary>
    /// スコアテキストを更新するためのクラス
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class UIElement_ScoreText : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private float _animationDuration = 0.5f;
        
        private Text _text;
        private int _currentDisplayScore = 0; // 現在表示されているスコア
        private Tweener _scoreTween;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private void Start()
        {
            _text = GetComponent<Text>();
            _scoreManager.ScoreProp.Subscribe(UpdateScore).AddTo(_disposable);
            _text.text = "00000000"; // 初期スコア表示
        }

        /// <summary>
        /// スコアのテキストをアニメーション付きで書き換える
        /// </summary>
        private void UpdateScore(int targetScore)
        {
            // 進行中のアニメーションがあれば中断
            if (_scoreTween != null && _scoreTween.IsActive())
            {
                _scoreTween.Kill();
            }
            
            // DOTweenを使用して現在の表示スコアから目標スコアまでアニメーション
            _scoreTween = DOTween.To(
                () => _currentDisplayScore,
                value => {
                    _currentDisplayScore = value;
                    _text.text = value.ToString("00000000");
                },
                targetScore, _animationDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => _currentDisplayScore = targetScore);  // アニメーション完了時に現在のスコアを確実に目標値に設定
        }

        /// <summary>
        /// バトル開始時点のスコアを記録する
        /// </summary>
        public void SavePreBattleScore() => _scoreManager.SavePreBattleScore();

        private void OnDestroy()
        {
            if (_scoreTween != null && _scoreTween.IsActive())
            {
                _scoreTween.Kill();
            }
            
            _disposable.Dispose();
        }
    }
}