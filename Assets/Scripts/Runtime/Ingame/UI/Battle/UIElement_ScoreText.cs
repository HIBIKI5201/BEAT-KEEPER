using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// スコアテキストを更新するためのクラス
    /// </summary>
    public class UIElement_ScoreText : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private string _scoreFormat = "00000000";

        [SerializeField] private Sprite[] _numberSprites = new Sprite[10]; // 数字の画像素材
        [SerializeField] private Image[] _scoreImages = new Image[8];
        
        private int _currentDisplayScore = 0; // 現在表示されているスコア
        private Tweener _scoreTween;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private void Start()
        {
            if (_scoreManager == null)
            {
                Debug.LogError("ScoreManager がアサインされていません");
                return;
            }
            
            // スコア増加のリアクティブプロパティを購読して値の変動時にスコアテキスト更新処理が実行されるようにする
            _scoreManager.ScoreProp.Subscribe(UpdateScore).AddTo(_disposable);
            
            for (int index = 0; index < _scoreImages.Length; index++)
            {
                if (_scoreImages[index] == null)
                {
                    // アサインされていなかった場合は、子オブジェクトから自動で取得する
                    _scoreImages[index] = transform.GetChild(index).GetComponent<Image>();
                }
            }
            
            // 初期スコア表示
            DisplayScore(0);
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
                    value =>
                    {
                        _currentDisplayScore = value;
                        DisplayScore(value);
                    },
                    targetScore, _animationDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => 
                {
                    _currentDisplayScore = targetScore;
                    DisplayScore(targetScore); // アニメーション完了時に確実に目標値を表示
                });
        }

        /// <summary>
        /// バトル開始時点のスコアを記録する
        /// </summary>
        public void SavePreBattleScore() => _scoreManager.SavePreBattleScore();

        /// <summary>
        /// スコアを画像で表示する
        /// </summary>
        private void DisplayScore(int score)
        {
            string scoreString = score.ToString(_scoreFormat);
            
            for (int i = 0; i < _scoreImages.Length; i++)
            {
                if (i < scoreString.Length)
                {
                    int digit = int.Parse(scoreString[i].ToString());
                    _scoreImages[i].sprite = _numberSprites[digit];
                }
            }
        }
        
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