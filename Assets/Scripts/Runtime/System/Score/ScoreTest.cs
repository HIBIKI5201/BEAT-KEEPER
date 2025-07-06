using BeatKeeper.Runtime.Ingame.System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// スコアシステムテスト用コンポーネント
    /// </summary>
    public class ScoreTest : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private BattleGradeEvaluator _battleGradeEvaluator;
        [SerializeField] private Text _scoreText;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            _scoreManager.ScoreProp
                .Subscribe(score => _scoreText.text = score.ToString())
                .AddTo(_disposables);
        }

        /// <summary>
        /// スコアを加算するテスト（100ポイント追加）
        /// </summary>
        [ContextMenu("Test_AddScore")]
        public void AddScore()
        {
            _scoreManager.AddScore(100);
        }

        /// <summary>
        /// バトル前スコアの保存テスト
        /// </summary>
        [ContextMenu("Test_SavePreBattleScore")]
        public void Save()
        {
            _scoreManager.SavePreBattleScore();
        }

        /// <summary>
        /// スコアリセットテスト
        /// </summary>
        [ContextMenu("Test_ResetScore")]
        public void Reset()
        {
            _scoreManager.ResetScore();
        }

        /// <summary>
        /// バトルグレード評価テスト
        /// </summary>
        [ContextMenu("Test_EvaluateRank")]
        public void Rank()
        {
            Debug.Log($"バトルグレード: {_battleGradeEvaluator.EvaluateRank()}");
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
