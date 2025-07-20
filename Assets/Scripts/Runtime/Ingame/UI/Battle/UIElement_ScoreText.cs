using DG.Tweening;
using DG.Tweening.Core;
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
        /// <summary>
        /// バトル開始時点のスコアを記録する
        /// </summary>
        public void SavePreBattleScore() => _scoreManager.SavePreBattleScore();
        
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private float _animationDuration = 0.5f;

        [SerializeField] private Sprite[] _numberSprites = new Sprite[10]; // 数字の画像素材
        [SerializeField] private Image[] _scoreImages = new Image[8]; // 8桁分の表示用Image
        
        private int _currentDisplayScore = 0; // 現在表示されているスコア
        
        // DOTweenのGC Allocを避けるためのキャッシュ
        private readonly DOSetter<int> _displayScoreSetter;
        
        private Tweener _scoreTween;
        
        private readonly int[] _currentDigits = new int[8]; // 現在表示中の各桁（差分更新用キャッシュ）
        private static readonly int[] _digitDivisors = { 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1 }; // 桁数分解計算の最適化用。事前計算済み配列
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UIElement_ScoreText()
        {
            // デリゲートをキャッシュ
            _displayScoreSetter = SetScoreForTween;
        }

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
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
            
            // 初期スコア表示（全桁強制更新）
            DisplayScore(0, true);
        }
        
        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _scoreTween?.Kill();
            _disposable?.Dispose();
        }

        #endregion

        /// <summary>
        /// DOTweenのアニメーション中に呼び出すためのセッター
        /// </summary>
        private void SetScoreForTween(int score)
        {
            // アニメーション中は桁が変わった部分のみ更新する
            DisplayScore(score, false);
        }
        
        /// <summary>
        /// スコアのテキストをアニメーション付きで書き換える
        /// </summary>
        private void UpdateScore(int targetScore)
        {
            // 進行中のアニメーションがあれば中断
            _scoreTween?.Kill();

            // DOTweenを使用して現在の表示スコアから目標スコアまでアニメーション
            _scoreTween = DOTween.To(
                    () => _currentDisplayScore, // 現在の値を取得
                    _displayScoreSetter, // キャッシュしたデリゲートを使用
                    targetScore, // 目標値 
                    _animationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => 
                {
                    _currentDisplayScore = targetScore;
                    DisplayScore(targetScore, true); // アニメーション完了時に確実に目標値を表示
                });
        }

        /// <summary>
        /// スコアを画像で表示する
        /// </summary>
        private void DisplayScore(int score, bool forceUpdate = false)
        {
            _currentDisplayScore = score;
            
            // 数値を直接計算で各桁に分解（string変換を回避）
            int tempScore = Mathf.Min(score, 99999999); // 8桁制限
            
            // NOTE: score.ToString()は文字列アロケーションが発生するため使用しない
            // アニメーション中だけfor文を8回処理する必要があるが...
            
            for (int i = 0; i < 8; i++)
            {
                // 桁の計算: (数値 / 桁の重み) % 10
                int digit = (tempScore / _digitDivisors[i]) % 10;
                
                // 桁が変わった場合、または強制更新の場合のみスプライトを更新
                // NOTE: UI更新はコストが高いため、必要最小限に抑制
                if (forceUpdate || _currentDigits[i] != digit)
                {
                    _currentDigits[i] = digit; // キャッシュ更新
                    _scoreImages[i].sprite = _numberSprites[digit]; // UI更新
                }
            }
        }
    }
}