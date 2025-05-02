using BeatKeeper.Runtime.Ingame.Character;
using R3;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// スコアを管理するシステム
    /// </summary>
    public class ScoreManager : MonoBehaviour, IScoreManager
    {
        [SerializeField] private PlayerManager _playerManager; // コンボ数を取得するためプレイヤーデータを参照
        [SerializeField] private ComboBonusSettingsSO _comboBonusData;
        
        public ReadOnlyReactiveProperty<int> ScoreProp => _scoreProp;
        private readonly ReactiveProperty<int> _scoreProp = new ReactiveProperty<int>(0);
        public int Score => _scoreProp.Value;

        private int _preBattleScore = 0; // バトルが始まる直前のスコア（バトルグレードの判定用）

        private void Awake()
        {
            if (_playerManager == null)
            {
                Debug.LogError("[ScoreManager] PlayerManager が設定されていません");
            }
            
            if (_comboBonusData == null)
            {
                Debug.LogError("[ScoreManager] ComboBonusData が設定されていません。デフォルト値を使用します");
            }
        }
        
        /// <summary>
        /// スコアを更新する
        /// </summary>
        public void AddScore(int score)
        {
            int addedScore = score + Score;
            
            // 正の数ならコンボボーナスも含めて計算する。負の数なら受け取ったスコアのまま
            if (score > 0)
            {
                addedScore = (int)Mathf.Floor(score * EvaluateComboBonus()) + Score; // 小数点以下は切り捨ててint型に変換
            }
            _scoreProp.Value = Mathf.Max(addedScore, 0); // スコアはゼロ以下にはしない
        }

        /// <summary>
        /// コンボ倍率を設定する
        /// </summary>
        private float EvaluateComboBonus()
        {
            if (_playerManager == null || _comboBonusData == null)
            {
                return 1.0f;
            }

            int currentCombo = _playerManager.ComboSystem.ComboCount;
            return _comboBonusData.GetBonusMultiplier(currentCombo);
        }
        
        /// <summary>
        /// バトル開始時にスコアを保存する
        /// TODO: バトルフェーズ開始タイミングで呼び出したい
        /// </summary>
        public void SavePreBattleScore()
        {
            _preBattleScore = Score;
            Debug.Log($"[ScoreManager] スコアを保存しました 保存: {_preBattleScore}"); 
        }

        /// <summary>
        /// バトル中に獲得したスコアを計算する
        /// TODO: バトルフェーズ フィニッシャー終了・クリアフェーズに移行したタイミングで呼び出したい
        /// </summary>
        public int CalculateBattleScore()
        {
            return Score - _preBattleScore;
        }

        /// <summary>
        /// スコアをリセットする
        /// TODO: インゲーム終了後に呼び出す（シーンの切り替えがあるなら不要）
        /// </summary>
        public void ResetScore()
        {
            _scoreProp.Value = 0;
            Debug.Log($"[ScoreManager] スコアをリセットしました 現在: {Score}");
        }
    }
}
