using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// バトルリザルトを表示するクラス
    /// </summary>
    public class BattleResultController : MonoBehaviour
    {
        [SerializeField] private BattleGradeEvaluator _battleGradeEvaluator;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _scoreText;
        
        /// <summary>
        /// リザルトを表示する
        /// </summary>
        public void Show()
        {
            DataSet();
            _canvasGroup.alpha = 1;
        }

        /// <summary>
        /// テキストを書き換える
        /// </summary>
        private void DataSet()
        {
            int score = _battleGradeEvaluator.CalculateBattleScore(); // 今回のバトルのスコア
            _rankText.text = _battleGradeEvaluator.EvaluateRank().ToString();
            _scoreText.text = $"score {score} !!!";
        }
        
        /// <summary>
        /// 非表示にする
        /// </summary>
        public void Hide()
        {
            _canvasGroup.alpha = 0;
        }
    }
}
