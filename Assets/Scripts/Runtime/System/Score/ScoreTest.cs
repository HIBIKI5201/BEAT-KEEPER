using UnityEngine;
using UnityEngine.UI;
using R3;

namespace BeatKeeper
{
    public class ScoreTest : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private BattleGradeEvaluator _battleGradeEvaluator;
        [SerializeField] private Text _scoreText;
        private CompositeDisposable _disposables = new CompositeDisposable();
        
        private void Start()
        {
            _scoreManager.ScoreProp.Subscribe(score => _scoreText.text = score.ToString()).AddTo(_disposables);
        }

        [ContextMenu("ScoreTest")]
        public void AddScore()
        {
            _scoreManager.AddScore(100);
        }

        [ContextMenu("Test_Save")]
        public void Save()
        {
            _scoreManager.SavePreBattleScore();
        }

        [ContextMenu("Test_Reset")]
        public void Reset()
        {
            _scoreManager.ResetScore();
        }

        [ContextMenu("Rank")]
        public void Rank()
        {
            Debug.Log(_battleGradeEvaluator.EvaluateRank().ToString());
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
