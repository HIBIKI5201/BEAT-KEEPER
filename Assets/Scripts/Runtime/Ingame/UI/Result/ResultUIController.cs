using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.System;

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
            // 演出開始
            StartPerformance();
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
        
        [Header("ボイスのCueNameの設定")]
        [SerializeField] private string _resultCueName;
        [SerializeField] private RankVoice[] _rankVoice = new RankVoice[4];
        [SerializeField] private RankVoice[] _resultVoice = new RankVoice[4];

        [Header("演出関連の設定")] 
        [SerializeField, Tooltip("ランク発表までの待機時間")] private float _rankDuration = 3f;
        
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
        /// 演出を開始する
        /// </summary>
        private void StartPerformance()
        {
            ShowCanvas();
            
            // ボイス再生中にスコアのアニメーションと記録のアニメーションを再生
            SetScore();
            SetRecords();
            
            // ボイス再生とアニメーションが終わるまで待つ（_rankDuration）
            SetRank();
            
            // ランク読み上げを待ってから賞賛ボイスを再生
            PlayPraiseVoice();
        }

        /// <summary>
        /// Canvasが有効になったときの処理
        /// </summary>
        private void ShowCanvas()
        {
            // NOTE: CanvasGroupの不透明度は現状ResultManagerから変更しているのでその処理は書いてない
            
            // ボイスを再生「今回のバトルレポートを確認するわ」
            VoiceManager.PlayVoice(_resultCueName);
        }
        
        /// <summary>
        /// スコアの値を設定する
        /// </summary>
        private void SetScore()
        {
            if(_scoreManager == null || _scoreText == null) return;
            
            // TODO: アニメーションをつける
            _scoreText.text = _scoreManager.Score.ToString("D8");
        }

        /// <summary>
        /// ランクの画像を設定する
        /// </summary>
        private void SetRank()
        {
            if(_scoreManager == null || _gradeEvaluator == null || _rankImage == null) return;
            
            // スコアを元にランクを算出
            var rank = _gradeEvaluator.EvaluateRank(_scoreManager.Score);

            // ランクに応じてボイス再生
            var voice = GetRankVoiceCueName(rank, _rankVoice);
            VoiceManager.PlayVoice(voice);
            
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
            // TODO: 演出をつける
            _maxCombo.SetAmount(_scoreManager.MaxCombo);
            _perfectCount.SetAmount(_scoreManager.AccuracyTracker.PerfectCount);
            _goodCount.SetAmount(_scoreManager.AccuracyTracker.GoodCount);
            _missCount.SetAmount(_scoreManager.AccuracyTracker.MissCount);
        }

        /// <summary>
        /// 賞賛ボイスを再生
        /// </summary>
        private void PlayPraiseVoice()
        {
            // スコアを元にランクを算出
            var rank = _gradeEvaluator.EvaluateRank(_scoreManager.Score);

            // ランクに応じてボイス再生
            var voice = GetRankVoiceCueName(rank, _resultVoice);
            VoiceManager.PlayVoice(voice);
        }
        
        /// <summary>
        /// ランクボイスの配列から指定されたランクに対応するCueNameを取得する
        /// </summary>
        private string GetRankVoiceCueName(BattleGradeEnum rank, RankVoice[] voiceNames)
        {
            foreach (var rankVoice in voiceNames)
            {
                if (rankVoice.Rank == rank)
                {
                    return rankVoice.CueName;
                }
            }
    
            Debug.LogWarning($"{typeof(ResultUIController)}: ランク {rank} に対応するボイスが見つかりません");
            return string.Empty;
        }

        [Serializable]
        private struct RankVoice
        {
            [SerializeField] private BattleGradeEnum _rank;
            [SerializeField] private string _cueName;
            
            public BattleGradeEnum Rank => _rank;
            public string CueName => _cueName;
        }
    }
}
