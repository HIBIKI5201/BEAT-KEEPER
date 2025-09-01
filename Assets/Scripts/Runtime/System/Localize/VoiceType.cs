using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// ボイスの種類（CueNameを元に作成）
    /// </summary>
   public enum VoiceType
   {
       // === 攻撃関連 ===
       voice_attack_normal1,
       voice_attack_normal2,
       voice_attack_normal3,
       
       // === チャージアタック関連 ===
       voice_charge_attack_charge,
       voice_charge_attack_fire,
       voice_charge_attack_damaged,
       
       // === 回避関連 ===
       voice_avoid_success,
       voice_avoid_damaged,
       
       // === スキル・アクション関連 ===
       voice_skill,
       voice_finisher,
       voice_recover,
       
       // === コンボ関連 ===
       voice_combo_complete,
       voice_combo_10,
       voice_combo_30,
       voice_combo_50,
       voice_combo_100,
       voice_combo_high,
       
       // === フローゾーン関連 ===
       voice_flow_zone_enter,
       voice_perfect_sync,
       
       // === ゲーム開始 ===
       voice_start_01,
       voice_start_02,
       voice_start_03,
       voice_start_04,
       voice_start_05,
       voice_start_06,
       voice_start_07,
       voice_start_08,
       voice_start_09,
       
       // === クリア関連 ===
       voice_clear_01,
       voice_clear_02,
       voice_result,
       voice_ending,
       
       // === ランク関連 ===
       voice_rank_c,
       voice_result_c,
       voice_rank_b,
       voice_result_b,
       voice_rank_a,
       voice_result_a,
       voice_rank_s,
       voice_result_s,
       
       // === チュートリアル - 基本 ===
       voice_tutorial_start,
       voice_tutorial_common_sign,
       voice_tutorial_common_encourage,
       
       // === チュートリアル - 通常攻撃 ===
       voice_tutorial_normal_attack_01,
       voice_tutorial_normal_attack_02,
       voice_tutorial_normal_attack_encourage_01,
       voice_tutorial_normal_attack_03,
       voice_tutorial_normal_attack_encourage_02,
       
       // === チュートリアル - スキル ===
       voice_tutorial_skill,
       
       // === チュートリアル - 回避 ===
       voice_tutorial_avoid_01,
       voice_tutorial_avoid_02,
       voice_tutorial_avoid_03,
       voice_tutorial_avoid_04,
       
       // === チュートリアル - フローゾーン ===
       voice_tutorial_flow_zone_01,
       voice_tutorial_flow_zone_02,
       voice_tutorial_flow_zone_03,
       
       // === チュートリアル - チャージアタック ===
       voice_tutorial_charge_attack_01,
       voice_tutorial_charge_attack_02,
       voice_tutorial_charge_attack_03,
       voice_tutorial_charge_attack_04,
       voice_tutorial_charge_attack_05,
       
       // === チュートリアル - 終了 ===
       voice_tutorial_end_01,
       voice_tutorial_end_02,
       voice_tutorial_end_03,
       voice_tutorial_end_04,
       
       // === 敵ボイス ===
       enemyvoice02,
   }
}