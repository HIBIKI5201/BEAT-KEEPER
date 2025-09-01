using UnityEngine;
using System.Collections.Generic;

namespace BeatKeeper
{
    /// <summary>
    /// ボイスと字幕のマスタデータ
    /// </summary>
    public static class MasterDialogueData
    {
        private static Dictionary<VoiceType, DialogueData> _dialogData = new Dictionary<VoiceType, DialogueData>
       {
           // === ゲーム開始 ===
           [VoiceType.voice_start_01] = new DialogueData("voice_start_01", new[] { "やけに静かね......" }),
           [VoiceType.voice_start_02] = new DialogueData("voice_start_02", new[] { "こちらリンハ。地上に出ました。" }),
           [VoiceType.voice_start_03] = new DialogueData("voice_start_03", new[] { "これより、救出作戦を開始しまsっ！？" }),
           [VoiceType.voice_start_04] = new DialogueData("voice_start_04", new[] { "あれが敵の新型？......" }),
           [VoiceType.voice_start_05] = new DialogueData("voice_start_05", new[] { "早速お出ましってわけね。" }),
           [VoiceType.voice_start_06] = new DialogueData("voice_start_06", new[] { "見逃してはくれなそうね......" }),
           [VoiceType.voice_start_07] = new DialogueData("voice_start_07", new[] { "こちらリンハ。これより敵性ユニットとの交戦を開始します！" }),
           [VoiceType.voice_start_08] = new DialogueData("voice_start_08", new[] { "さて......邪魔をするなら容赦はしないわよ！" }),
           [VoiceType.voice_start_09] = new DialogueData("voice_start_09", new[] { "パーフェクトシンク！！" }),
           
           // === フローゾーン関連 ===
           [VoiceType.voice_perfect_sync] = new DialogueData("voice_perfect_sync", new[] { "パーフェクトシンク！！" }),
           
           // === クリア関連 ===
           [VoiceType.voice_clear_01] = new DialogueData("voice_clear_01", new[] { "戦闘終了。敵性ユニットの沈黙を確認！" }),
           [VoiceType.voice_clear_02] = new DialogueData("voice_clear_02", new[] { "今回のバトルレポートを確認するわ。" }),
           [VoiceType.voice_result] = new DialogueData("voice_result", new[] { "今回のバトルレポートを確認するわ。" }),
           
           // === ランク関連 ===
           [VoiceType.voice_rank_c] = new DialogueData("voice_rank_c", new[] { "ランクC！" }),
           [VoiceType.voice_result_c] = new DialogueData("voice_result_c", new[] { "手ごわい相手だったわ...でも次は、きっと楽勝ね！" }),
           [VoiceType.voice_rank_b] = new DialogueData("voice_rank_b", new[] { "ランクB！" }),
           [VoiceType.voice_result_b] = new DialogueData("voice_result_b", new[] { "ふふっ、私にかかればこんなものよ！" }),
           [VoiceType.voice_rank_a] = new DialogueData("voice_rank_a", new[] { "ランクA！" }),
           [VoiceType.voice_result_a] = new DialogueData("voice_result_a", new[] { "完璧！さすが私ね！" }),
           [VoiceType.voice_rank_s] = new DialogueData("voice_rank_s", new[] { "ランクS！" }),
           [VoiceType.voice_result_s] = new DialogueData("voice_result_s", new[] { "すごい！こんな感覚初めて！今ならどんな敵にも負ける気がしないわ！" }),
           [VoiceType.voice_ending] = new DialogueData("voice_ending", new[] { "遊んでくれてありがとう！また、あなたと戦える日を楽しみにしているわ。" }),
           
           // === チュートリアル - 基本 ===
           [VoiceType.voice_tutorial_start] = new DialogueData("voice_tutorial_start", new[] { "まずはこの力のおさらいね…！" }),
           [VoiceType.voice_tutorial_common_sign] = new DialogueData("voice_tutorial_common_sign", new[] { "今よ！" }),
           [VoiceType.voice_tutorial_common_encourage] = new DialogueData("voice_tutorial_common_encourage", new[] { "ちょっとタイミングが合わなかったわね。もう一回やってみましょ！" }),
           
           // === チュートリアル - 通常攻撃 ===
           [VoiceType.voice_tutorial_normal_attack_01] = new DialogueData("voice_tutorial_normal_attack_01", new[] { "最初は通常攻撃から！" }),
           [VoiceType.voice_tutorial_normal_attack_02] = new DialogueData("voice_tutorial_normal_attack_02", new[] { "リングが重なるタイミングを狙って！" }),
           [VoiceType.voice_tutorial_normal_attack_encourage_01] = new DialogueData("voice_tutorial_normal_attack_encourage_01", new[] { "そう！いい感じ！" }),
           [VoiceType.voice_tutorial_normal_attack_03] = new DialogueData("voice_tutorial_normal_attack_03", new[] { "次は連続で攻撃してみましょう！行くわよ！" }),
           [VoiceType.voice_tutorial_normal_attack_encourage_02] = new DialogueData("voice_tutorial_normal_attack_encourage_02", new[] { "いいわね、その調子！" }),
           
           // === チュートリアル - スキル ===
           [VoiceType.voice_tutorial_skill] = new DialogueData("voice_tutorial_skill", new[] { "今度はスキルよ！さっきみたいにタイミングを合わせて！" }),
           
           // === チュートリアル - 回避 ===
           [VoiceType.voice_tutorial_avoid_01] = new DialogueData("voice_tutorial_avoid_01", new[] { "危ない！敵が攻撃してくる！" }),
           [VoiceType.voice_tutorial_avoid_02] = new DialogueData("voice_tutorial_avoid_02", new[] { "タイミングを合わせて回避よ！" }),
           [VoiceType.voice_tutorial_avoid_03] = new DialogueData("voice_tutorial_avoid_03", new[] { "今度はボタンが違うからね！" }),
           [VoiceType.voice_tutorial_avoid_04] = new DialogueData("voice_tutorial_avoid_04", new[] { "うまく避けられたわね！" }),
           
           // === チュートリアル - フローゾーン ===
           [VoiceType.voice_tutorial_flow_zone_01] = new DialogueData("voice_tutorial_flow_zone_01", new[] { "見て！ゲージが溜まったことに気付いた？" }),
           [VoiceType.voice_tutorial_flow_zone_02] = new DialogueData("voice_tutorial_flow_zone_02", new[] { "これを全て溜めると、武器の全ての力を引き出せるフローゾーンに入れるの！" }),
           [VoiceType.voice_tutorial_flow_zone_03] = new DialogueData("voice_tutorial_flow_zone_03", new[] { "リズムに合わせて、敵の攻撃はしっかり回避していきましょう！" }),
           
           // === チュートリアル - チャージアタック ===
           [VoiceType.voice_tutorial_charge_attack_01] = new DialogueData("voice_tutorial_charge_attack_01", new[] { "もっと危険そうな攻撃の準備をしてるみたいね...！" }),
           [VoiceType.voice_tutorial_charge_attack_02] = new DialogueData("voice_tutorial_charge_attack_02", new[] { "チャージショットで相手の動きを封じてしまいましょう！" }),
           [VoiceType.voice_tutorial_charge_attack_03] = new DialogueData("voice_tutorial_charge_attack_03", new[] { "ボタンを長押ししてチャージよ！", "ボタンを押し続けて！" }),
           [VoiceType.voice_tutorial_charge_attack_04] = new DialogueData("voice_tutorial_charge_attack_04", new[] { "チャージ完了！ボタンを放して！" }),
           [VoiceType.voice_tutorial_charge_attack_05] = new DialogueData("voice_tutorial_charge_attack_05", new[] { "決まったわ！" }),
           
           // === チュートリアル - 終了 ===
           [VoiceType.voice_tutorial_end_01] = new DialogueData("voice_tutorial_end_01", new[] { "さあっ、本番といきましょう！" }),
           [VoiceType.voice_tutorial_end_02] = new DialogueData("voice_tutorial_end_02", new[] { "いくわよ！" }),
           [VoiceType.voice_tutorial_end_03] = new DialogueData("voice_tutorial_end_03", new[] { "ビートシンカー。オンステージ！" }),
           [VoiceType.voice_tutorial_end_04] = new DialogueData("voice_tutorial_end_04", new[] { "" }), // 空の台詞
           
           // === 敵ボイス ===
           [VoiceType.enemyvoice02] = new DialogueData("enemyvoice02", new[] { "キケンキケン", "ミダレタリズム", "ハイジョシマス" })
       };

        /// <summary>
        /// 指定されたボイスタイプの台詞データを取得
        /// </summary>
        public static string GetSubtitleText(VoiceType voiceType)
        {
            if (!_dialogData.TryGetValue(voiceType, out var data))
            {
                Debug.LogWarning($"{voiceType} は登録されていません");
                return null;
            }
            
            // TODO: 取得方法修正
            return data.Subtitles[(int)LanguageType.Japanese];
        }
        
        /// <summary>
        /// 指定されたボイスタイプが存在するかチェック
        /// </summary>
        public static bool HasDialogueData(VoiceType voiceType)
        {
            return _dialogData.ContainsKey(voiceType);
        }
    }
}
