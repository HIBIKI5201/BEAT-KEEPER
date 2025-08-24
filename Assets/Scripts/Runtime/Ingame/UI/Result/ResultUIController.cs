using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using BeatKeeper.Runtime.Ingame.System;

namespace BeatKeeper
{
    /// <summary>
    /// リザルト画面のUIを管理するクラス
    /// </summary>
    public class ResultUIController : MonoBehaviour
    {
        /// <summary>
        /// リザルトの数字を設定する
        /// </summary>
        /// <returns></returns>
        public void SetResult()
        {
            SetScore();
            SetRank();
            SetRecords();
        }
        
        [SerializeField] private ScoreManager _scoreManager; // スコアマネージャー
        [SerializeField] private BattleGradeEvaluator _gradeEvaluator; // ランクを判定するクラス
        
        [Header("UIの各要素の参照")]
        [SerializeField] private Text _scoreText; // スコア
        [SerializeField] private Image _rankImage; // ランク
        [SerializeField] private UIContents_Result _maxCombo; // 最大コンボ数
        [SerializeField] private UIContents_Result _perfectCount; // Perfect判定の回数
        [SerializeField] private UIContents_Result _goodCount; // Good判定の回数
        [SerializeField] private UIContents_Result _missCount; // Miss判定の回数

        [Header("ランク画像")] 
        [SerializeField] private SpriteAtlas _rankSpriteAtlas; // ランクの画像のスプライトアトラス
        
        [SerializeField] private string _rankSpriteSuffix = "_rank"; // ランク画像のアセット名のサフィックス

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
            // nullチェックをして警告を出しておく
            if (_scoreManager == null)
            {
                Debug.LogError($"{typeof(ResultUIController)}: スコアマネージャーがアサインされていません");
            }

            if (_gradeEvaluator == null)
            {
                Debug.LogError($"{typeof(ResultUIController)}: {typeof(BattleGradeEvaluator)}がアサインされていません");
            }
        }

        /// <summary>
        /// スコアの値を設定する
        /// </summary>
        private void SetScore()
        {
            if(_scoreManager == null || _scoreText == null) return;
            
            // TODO: アニメーションをつける
            _scoreText.text = _scoreManager.Score.ToString();
        }

        /// <summary>
        /// ランクの画像を設定する
        /// </summary>
        private void SetRank()
        {
            if(_scoreManager == null || _gradeEvaluator == null || _rankImage == null) return;
            
            // スコアを元にランクを算出
            var rank = _gradeEvaluator.EvaluateRank(_scoreManager.Score);
            
            // ランクの文字列とサフィックスを連結して、スプライトをロードしてくる
            var rankSprite = _rankSpriteAtlas.GetSprite($"{rank.ToString()}{_rankSpriteSuffix}");

            if (rankSprite != null)
            {
                _rankImage.sprite = rankSprite;
            }
        }

        /// <summary>
        /// コンボ数やPerfect数などの記録UIを設定する
        /// </summary>
        private void SetRecords()
        {
            _maxCombo.SetAmount(_scoreManager.MaxCombo);
            _perfectCount.SetAmount(_scoreManager.AccuracyTracker.PerfectCount);
            _goodCount.SetAmount(_scoreManager.AccuracyTracker.GoodCount);
            _missCount.SetAmount(_scoreManager.AccuracyTracker.MissCount);
        }
    }
}
