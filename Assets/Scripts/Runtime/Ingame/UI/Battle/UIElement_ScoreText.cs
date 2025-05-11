using UnityEngine;
using UnityEngine.UI;
using R3;

namespace BeatKeeper
{
    /// <summary>
    /// スコアテキストを更新するためのクラス
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class UIElement_ScoreText : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        private Text _text;
        private CompositeDisposable _disposable = new CompositeDisposable();

        private void Start()
        {
            _text = GetComponent<Text>();
            _scoreManager.ScoreProp.Subscribe(UpdateScore).AddTo(_disposable);
        }

        /// <summary>
        /// スコアのテキストを書き換える
        /// </summary>
        private void UpdateScore(int score)
        {
            _text.text = score.ToString("00000000");
        }

        /// <summary>
        /// バトル開始時点のスコアを記録する
        /// </summary>
        public void SavePreBattleScore() => _scoreManager.SavePreBattleScore();

        private void OnDestroy() => _disposable.Dispose();
    }
}
