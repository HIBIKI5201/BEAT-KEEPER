using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;
using DG.Tweening;
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
        /// リザルト演出を開始する
        /// </summary>
        public void SetResult()
        {
            // 参照に不備がある場合return
            if (!IsValidState()) return;
            
            // ランクを計算
            _currentRank = _gradeEvaluator.EvaluateRank(_scoreManager.Score);
            
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
        [SerializeField, Tooltip("スコアアニメーションにかける時間")] private float _scoreAnimationDuration = 2f; 
        [SerializeField, Tooltip("ランク発表までの待機時間")] private float _rankRevealDelay = 3f;
        
        private BattleGradeEnum _currentRank; // 今回のランク
        private Sequence _resultSequence;

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
            ValidateReferences();
        }

        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _resultSequence?.Kill();
        }

        #endregion
        
        /// <summary>
        /// 演出を開始する
        /// </summary>
        private void StartPerformance()
        {
            // 既存のシーケンスがあれば停止する
            _resultSequence?.Kill();
            _resultSequence = DOTween.Sequence();
            
            // 初期表示とボイス再生「今回のバトルレポートを確認するわ」
            _resultSequence.AppendCallback(ShowCanvas);
            
            // ボイス再生中にスコアのアニメーションと最大コンボ数などの枠のスライドインアニメーションを再生
            _resultSequence.Append(CreateScoreTween());
            _resultSequence.Join(CreateRecordsTween());
            _resultSequence.Join(DOVirtual.DelayedCall(_rankRevealDelay, () => { }));

            _resultSequence.Append(CreateRankTween());
            _resultSequence.Join(DOVirtual.DelayedCall(_rankRevealDelay, () => { }));
            
            // ランク読み上げを待ってから賞賛ボイスを再生
            _resultSequence.AppendCallback(PlayPraiseVoice);
        }

        /// <summary>
        /// Canvasが有効になったときの処理
        /// </summary>
        private void ShowCanvas()
        {
            // NOTE: CanvasGroupの不透明度は現状ResultManagerから変更しているのでその処理は書いてない
            VoiceManager.PlayVoice(_resultCueName);
        }
        
        /// <summary>
        /// スコアの値を設定する
        /// </summary>
        private Tween CreateScoreTween()
        {
            if(_scoreText == null) 
                return DOVirtual.DelayedCall(_scoreAnimationDuration, () => { });

            // 0を8桁埋めて表示を初期化
            _scoreText.text = "00000000";
            
            // スコアを先に取得しておく
            var targetScore = _scoreManager.Score;
            
            // 目標スコアまでアニメーション
            return DOTween.To(
                    () => 0,
                    SetScore, // キャッシュしたデリゲートを使用
                    targetScore, // 目標値 
                    _scoreAnimationDuration) // 時間
                .SetEase(Ease.OutQuad)
                .OnComplete(() => 
                {
                    SetScore(targetScore); // アニメーション完了時に確実に目標値を表示
                });
        }

        /// <summary>
        /// Textコンポーネントを更新
        /// </summary>
        private void SetScore(int amount)
        {
            _scoreText.text = amount.ToString("D8");
        }

        /// <summary>
        /// ランクの画像を設定する
        /// </summary>
        private Tween CreateRankTween()
        {
            if(_rankImage == null) 
                return DOVirtual.DelayedCall(_scoreAnimationDuration, () => { });
            
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

            return DOVirtual.DelayedCall(_scoreAnimationDuration, () => { });
        }

        /// <summary>
        /// コンボ数やPerfect数などの記録UIを設定する
        /// </summary>
        private Tween CreateRecordsTween()
        {
            // TODO: 演出をつける
            _maxCombo.SetAmount(_scoreManager.MaxCombo);
            _perfectCount.SetAmount(_scoreManager.AccuracyTracker.PerfectCount);
            _goodCount.SetAmount(_scoreManager.AccuracyTracker.GoodCount);
            _missCount.SetAmount(_scoreManager.AccuracyTracker.MissCount);
            
            return DOVirtual.DelayedCall(_scoreAnimationDuration, () => { });
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
        
        /// <summary>
        /// 必要な参照の検証を行う
        /// </summary>
        private void ValidateReferences()
        {
            if (_scoreManager == null)
            {
                Debug.LogWarning($"{this}: スコアマネージャーがアサインされていません");
            }

            if (_gradeEvaluator == null)
            {
                Debug.LogWarning($"{this}: BattleGradeEvaluatorがアサインされていません");
            }
            
            if (_rankSpriteAtlas == null)
            {
                Debug.LogWarning($"{this}: ランクスプライトアトラスがアサインされていません");
            }
        }
        
        /// <summary>
        /// 有効な状態かどうかを確認する
        /// </summary>
        private bool IsValidState()
        {
            return _scoreManager != null && _gradeEvaluator != null;
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
