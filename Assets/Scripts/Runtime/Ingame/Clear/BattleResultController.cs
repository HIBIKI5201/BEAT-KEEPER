using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// バトルリザルトを表示するクラス
    /// </summary>
    public class BattleResultController : MonoBehaviour
    {
        /// <summary>
        /// リザルトを表示する
        /// </summary>
        public void Show()
        {
            DataSet();
            _canvasGroup.DOFade(1, 1f);
            _canvasGroup.transform.DOLocalMoveX(-100, 1f);
        }

        /// <summary>
        /// 非表示にする
        /// </summary>
        public void Hide()
        {
            _canvasGroup.DOFade(0, 0.5f);
            _canvasGroup.transform.DOLocalMoveX(0, 0.5f);
        }

        [SerializeField] private BattleGradeEvaluator _battleGradeEvaluator;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _scoreText;

        /// <summary>
        /// テキストを書き換える
        /// </summary>
        private void DataSet()
        {
            int score = _battleGradeEvaluator.CalculateBattleScore(); // 今回のバトルのスコア
            _rankText.text = _battleGradeEvaluator.EvaluateRank().ToString();
            DOTween.To(() => 0,
                x => _scoreText.text = $"score {Mathf.FloorToInt(x).ToString()} !!!",
                score, 2f).SetEase(Ease.OutQuad);
        }
    }
}
