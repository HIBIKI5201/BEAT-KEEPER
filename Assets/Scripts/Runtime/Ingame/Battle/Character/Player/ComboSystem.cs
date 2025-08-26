using System.Collections.Generic;
using R3;
using UnityEngine;
using BeatKeeper.Runtime.System;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///    プレイヤーのコンボシステムを管理するクラス
    /// </summary>
    public class ComboSystem
    {
        public ComboSystem(PlayerData data) => _data = data;

        public ReadOnlyReactiveProperty<int> ComboCount => _comboCount;

        /// <summary>
        ///     攻撃時にコンボカウントを増やす
        /// </summary>
        public void Attack()
        {
            _comboCount.Value++;
            CheckAndPlayComboVoice(_comboCount.Value);
        }

        /// <summary>
        ///     コンボをリセットする
        /// </summary>
        public void ComboReset() 
        {
            if (_comboCount.Value == 0) return;

            _comboCount.Value = 0;
            Debug.Log("Combo Reset");
        }

        // NOTE: 使用しなくなったが、互換性のためにメソッドは残してある
        public void Update() { }

        // 各コンボボイスのcueName
        private static readonly Dictionary<int, string> _comboVoiceMap = new()
        {
            { 10, "voice_combo_10"},
            { 30, "voice_combo_30"},
            { 50, "voice_combo_50"},
            { 100, "voice_combo_100"}
        };
        private readonly string _highComboVoices = "voice_combo_high";
        
        private readonly PlayerData _data;
        private ReactiveProperty<int> _comboCount = new();

        /// <summary>
        /// コンボ数に応じてボイスを再生する
        /// </summary>
        private void CheckAndPlayComboVoice(int combo)
        {
            // 特定のコンボ数のボイスをチェック
            if (_comboVoiceMap.TryGetValue(combo, out string voice))
            {
                VoiceManager.PlayVoice(voice);
                return;
            }

            // 100コンボ以降で50の倍数の場合
            if (combo > 100 && combo % 50 == 0)
            {
                VoiceManager.PlayVoice(_highComboVoices);
            }
        }
    }
}
