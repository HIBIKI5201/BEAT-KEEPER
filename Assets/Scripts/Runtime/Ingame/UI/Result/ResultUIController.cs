using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;
using DG.Tweening;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.System;
using CriWare;

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

        /// <summary>
        /// ボイスが再生中なら止める
        /// </summary>
        public void StopVoice()
        {
            _playback.Stop();
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
        
        [Header("SEのCueNameの設定")]
        [SerializeField] private string _rankSSe = "Rank_S";
        [SerializeField] private string _rankASe = "Rank_A";
        [SerializeField] private string _rankBSe = "Rank_B";
        [SerializeField] private string _rankCSe = "Rank_C";
        [SerializeField] private string _counterSe = "ResultCounter";
        [SerializeField] private string _slideInSe = "FrameSlideIn";

        [Header("演出関連の設定")] 
        [SerializeField, Tooltip("スコアアニメーションにかける時間")] private float _scoreAnimationDuration = 2f; 
        [SerializeField, Tooltip("ランク発表までの待機時間")] private float _rankRevealDelay = 1f;
        [SerializeField, Tooltip("賞賛ボイスまでの待機時間")] private float _resultRevealDelay = 2f;
        
        
        private BattleGradeEnum _currentRank; // 今回のランク
        private Sequence _resultSequence;
        private CriAtomExPlayback _playback;

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
            ValidateReferences();

            if (_scoreText != null)
            {
                // 0を8桁埋めて表示を初期化
                _scoreText.text = "00000000";
            }

            if (_rankImage != null)
            {
                _rankImage.enabled = false;
            }
        }

        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _resultSequence?.Kill();
            
            // ボイスが再生中なら止める
            _playback.Stop();
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

            // 少し待機
            _resultSequence.AppendInterval(_rankRevealDelay);

            // ランク表示
            _resultSequence.Append(CreateRankTween());
            _resultSequence.Join(DOVirtual.DelayedCall(_resultRevealDelay, () => { }));
            
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
            if(_scoreText == null) return DOVirtual.DelayedCall(0f, () => { });
            
            // SE再生
            SoundEffectManager.PlaySoundEffect(_counterSe);
            
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
                return DOVirtual.DelayedCall(0f, () => { });
            
            // スコアを元にランクを算出
            var rank = _gradeEvaluator.EvaluateRank(_scoreManager.Score);

            // ランクに応じて再生するボイス名を取得
            var voice = GetRankVoiceCueName(rank, _rankVoice);

            // ランクのスプライトを取得
            var rankSprite = _rankSpriteAtlas.GetSprite($"{rank.ToString()}{_rankSpriteSuffix}");
            if (rankSprite == null)
            {
                return DOVirtual.DelayedCall(0f, () => { });
            }
            
            // 表示を整える
            _rankImage.transform.localScale = Vector3.zero;
            _rankImage.color = new Color(1f, 1f, 1f, 0f);
            _rankImage.sprite = rankSprite;
            _rankImage.enabled = true;
                    
            var sequence = DOTween.Sequence();
        
            // スケールアップと同時にフェードイン
            sequence.Append(_rankImage.transform.DOScale(1.4f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(_rankImage.DOFade(1f, 0.25f).SetEase(Ease.OutQuart));
            
            // 通常サイズに戻す
            sequence.Append(_rankImage.transform.DOScale(1f, 0.2f).SetEase(Ease.InOutQuart));
            sequence.Join(DOVirtual.DelayedCall(0f, () =>
            {
                // ランク読み上げボイス/SEを再生
                VoiceManager.PlayVoice(voice);
                SoundEffectManager.PlaySoundEffect(GetRankSeCueName(rank));
            }));
            
            return sequence;
        }

        /// <summary>
        /// コンボ数やPerfect数などの記録UIを設定する
        /// </summary>
        private Tween CreateRecordsTween()
        {
            var elements = new[] { _maxCombo, _perfectCount, _goodCount, _missCount };
    
            // 初期化
            foreach (var element in elements)
            {
                element.CanvasGroup.alpha = 0f;
                element.transform.localPosition += Vector3.left * 80f;
            }
    
            // スコア設定
            _maxCombo.SetAmount(_scoreManager.MaxCombo);
            _perfectCount.SetAmount(_scoreManager.AccuracyTracker.PerfectCount);
            _goodCount.SetAmount(_scoreManager.AccuracyTracker.GoodCount);
            _missCount.SetAmount(_scoreManager.AccuracyTracker.MissCount);
            
            // アニメーション
            var sequence = DOTween.Sequence();

            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var targetPos = element.transform.localPosition + Vector3.right * 80f;

                // 最初の要素だけ即座にSE再生
                if (i == 0)
                {
                    SoundEffectManager.PlaySoundEffect(_slideInSe);
                }

                sequence.Insert(i * 0.06f, element.CanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
                sequence.Insert(i * 0.06f, element.transform.DOLocalMove(targetPos, 0.4f).SetEase(Ease.OutCubic));
    
                // 2番目以降の要素は遅延してSE再生
                if (i > 0)
                {
                    sequence.InsertCallback(i * 0.06f, () => SoundEffectManager.PlaySoundEffect(_slideInSe));
                }
            }
    
            sequence.SetDelay(0.15f);

            return sequence;
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
            _playback = VoiceManager.PlayVoice(voice);
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
        /// 指定したランクのSEのCueNameを取得する
        /// </summary>
        private string GetRankSeCueName(BattleGradeEnum rank)
        {
            return rank switch
            {
                BattleGradeEnum.S => _rankSSe,
                BattleGradeEnum.A => _rankASe,
                BattleGradeEnum.B => _rankBSe,
                BattleGradeEnum.C => _rankCSe,
                _ => ""
            };
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
