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
            [VoiceType.voice_start_01] = new DialogueData("voice_start_01", new[] { "やけに静かね......", "It's strangely quiet..." }),
            [VoiceType.voice_start_02] = new DialogueData("voice_start_02", new[] { "こちらリンハ。地上に出ました。", "This is Rinha. I've reached the surface." }),
            [VoiceType.voice_start_03] = new DialogueData("voice_start_03", new[] { "これより、救出作戦を開始しま......", "Now beginning the rescue operation..." }),
            [VoiceType.voice_start_04] = new DialogueData("voice_start_04", new[] { "っ！？", "Huh!?" }),
            [VoiceType.voice_start_05] = new DialogueData("voice_start_05", new[] { "あれが敵の新型？", "Is that the enemy's new model?" }),
            [VoiceType.voice_start_06] = new DialogueData("voice_start_06", new[] { "......早速お出ましってわけね。", "...So they're showing up already." }),
            [VoiceType.voice_start_07] = new DialogueData("voice_start_07", new[] { "見逃してはくれなそうね......", "Doesn't look like they'll let me pass..." }),
            [VoiceType.voice_start_08] = new DialogueData("voice_start_08", new[] { "こちらリンハ。これより敵性ユニットとの交戦を開始します！", "This is Rinha. Commencing engagement with hostile units!" }),
            [VoiceType.voice_start_09] = new DialogueData("voice_start_09", new[] { "さて......邪魔をするなら容赦はしないわよ！", "Well then... If you're going to get in my way, I won't hold back!" }),
            
            // === クリア関連 ===
            [VoiceType.voice_clear_01] = new DialogueData("voice_clear_01", new[] { "戦闘終了。", "Combat complete." }),
            [VoiceType.voice_clear_02] = new DialogueData("voice_clear_02", new[] { "敵性ユニットの沈黙を確認！", "Hostile unit silenced, confirmed!" }),
            [VoiceType.voice_result] = new DialogueData("voice_result", new[] { "今回のバトルレポートを確認するわ。", "Let me check this battle report." }),
            
            // === ランク関連 ===
            [VoiceType.voice_rank_c] = new DialogueData("voice_rank_c", new[] { "ランクC！", "Rank C!" }),
            [VoiceType.voice_result_c] = new DialogueData("voice_result_c", new[] { "手ごわい相手だったわ...でも次は、きっと楽勝ね！", "That was a tough opponent... But next time will be a breeze!" }),
            [VoiceType.voice_rank_b] = new DialogueData("voice_rank_b", new[] { "ランクB！", "Rank B!" }),
            [VoiceType.voice_result_b] = new DialogueData("voice_result_b", new[] { "ふふっ、私にかかればこんなものよ！", "Hehe, this is nothing for me!" }),
            [VoiceType.voice_rank_a] = new DialogueData("voice_rank_a", new[] { "ランクA！", "Rank A!" }),
            [VoiceType.voice_result_a] = new DialogueData("voice_result_a", new[] { "完璧！さすが私ね！", "Perfect! That's me alright!" }),
            [VoiceType.voice_rank_s] = new DialogueData("voice_rank_s", new[] { "ランクS！", "Rank S!" }),
            [VoiceType.voice_result_s] = new DialogueData("voice_result_s", new[] { "すごい！こんな感覚初めて！今ならどんな敵にも負ける気がしないわ！", "Amazing! I've never felt like this before! I feel like I could beat any enemy right now!" }),
            [VoiceType.voice_ending] = new DialogueData("voice_ending", new[] { "遊んでくれてありがとう！また、あなたと戦える日を楽しみにしているわ。", "Thank you for playing! I look forward to fighting alongside you again." }),
            
            // === チュートリアル - 基本 ===
            [VoiceType.voice_tutorial_start] = new DialogueData("voice_tutorial_start", new[] { "まずはこの力のおさらいね…！", "First, let's review these powers...!" }),
            [VoiceType.voice_tutorial_common_sign] = new DialogueData("voice_tutorial_common_sign", new[] { "今よ！", "Now!" }),
            [VoiceType.voice_tutorial_common_encourage] = new DialogueData("voice_tutorial_common_encourage", new[] { "ちょっとタイミングが合わなかったわね。もう一回やってみましょ！", "The timing was a bit off. Let's try again!" }),
            
            // === チュートリアル - 通常攻撃 ===
            [VoiceType.voice_tutorial_normal_attack_01] = new DialogueData("voice_tutorial_normal_attack_01", new[] { "最初は通常攻撃から！", "Let's start with normal attacks!" }),
            [VoiceType.voice_tutorial_normal_attack_02] = new DialogueData("voice_tutorial_normal_attack_02", new[] { "リングが重なるタイミングを狙って！", "Aim for when the rings overlap!" }),
            [VoiceType.voice_tutorial_normal_attack_encourage_01] = new DialogueData("voice_tutorial_normal_attack_encourage_01", new[] { "そう！いい感じ！", "Yes! That's the way!" }),
            [VoiceType.voice_tutorial_normal_attack_03] = new DialogueData("voice_tutorial_normal_attack_03", new[] { "次は連続で攻撃してみましょう！行くわよ！", "Now let's try consecutive attacks! Here we go!" }),
            [VoiceType.voice_tutorial_normal_attack_encourage_02] = new DialogueData("voice_tutorial_normal_attack_encourage_02", new[] { "いいわね、その調子！", "Nice, keep it up!" }),
            
            // === チュートリアル - スキル ===
            [VoiceType.voice_tutorial_skill] = new DialogueData("voice_tutorial_skill", new[] { "今度はスキルよ！さっきみたいにタイミングを合わせて！", "Now it's skills! Match the timing like before!" }),
            
            // === チュートリアル - 回避 ===
            [VoiceType.voice_tutorial_avoid_01] = new DialogueData("voice_tutorial_avoid_01", new[] { "危ない！敵が攻撃してくる！", "Danger! The enemy is attacking!" }),
            [VoiceType.voice_tutorial_avoid_02] = new DialogueData("voice_tutorial_avoid_02", new[] { "タイミングを合わせて回避よ！", "Match the timing and dodge!" }),
            [VoiceType.voice_tutorial_avoid_03] = new DialogueData("voice_tutorial_avoid_03", new[] { "今度はボタンが違うからね！", "This time the button is different!" }),
            [VoiceType.voice_tutorial_avoid_04] = new DialogueData("voice_tutorial_avoid_04", new[] { "うまく避けられたわね！", "You dodged that well!" }),
            
            // === チュートリアル - フローゾーン ===
            [VoiceType.voice_tutorial_flow_zone_01] = new DialogueData("voice_tutorial_flow_zone_01", new[] { "見て！ゲージが溜まったことに気付いた？", "Look! Did you notice the gauge filled up?" }),
            [VoiceType.voice_tutorial_flow_zone_02] = new DialogueData("voice_tutorial_flow_zone_02", new[] { "これを全て溜めると、武器の全ての力を引き出せるフローゾーンに入れるの！", "When this fills completely, you can enter the Flow Zone and unleash your weapon's full power!" }),
            [VoiceType.voice_tutorial_flow_zone_03] = new DialogueData("voice_tutorial_flow_zone_03", new[] { "リズムに合わせて、敵の攻撃はしっかり回避していきましょう！", "Keep to the rhythm and dodge the enemy attacks properly!" }),
            
            // === チュートリアル - チャージアタック ===
            [VoiceType.voice_tutorial_charge_attack_01] = new DialogueData("voice_tutorial_charge_attack_01", new[] { "もっと危険そうな攻撃の準備をしてるみたいね......！", "Looks like they're preparing a more dangerous attack...!" }),
            [VoiceType.voice_tutorial_charge_attack_02] = new DialogueData("voice_tutorial_charge_attack_02", new[] { "チャージショットで相手の動きを封じてしまいましょう！", "Let's seal their movements with a charge shot!" }),
            [VoiceType.voice_tutorial_charge_attack_03] = new DialogueData("voice_tutorial_charge_attack_03", new[] { "ボタンを長押ししてチャージよ！", "Hold the button to charge!" }),
            [VoiceType.voice_tutorial_charge_attack_04] = new DialogueData("voice_tutorial_charge_attack_04", new[] { "ボタンを押し続けて！", "Keep holding the button!" }),
            [VoiceType.voice_tutorial_charge_attack_05] = new DialogueData("voice_tutorial_charge_attack_05", new[] { "チャージ完了！ボタンを放して！", "Charge complete! Release the button!" }),
            
            // === チュートリアル - 終了 ===
            [VoiceType.voice_tutorial_end_01] = new DialogueData("voice_tutorial_end_01", new[] { "決まったわ！", "Got it!" }),
            [VoiceType.voice_tutorial_end_02] = new DialogueData("voice_tutorial_end_02", new[] { "さあっ、本番といきましょう！", "Alright, let's go for the real thing!" }),
            [VoiceType.voice_tutorial_end_03] = new DialogueData("voice_tutorial_end_03", new[] { "いくわよ！", "Here I go!" }),
            [VoiceType.voice_tutorial_end_04] = new DialogueData("voice_tutorial_end_04", new[] { "ビートシンカー。オンステージ！", "Beat Syncer. On stage!" }),
            
            // === 敵ボイス ===
            [VoiceType.enemyvoice02] = new DialogueData("enemyvoice02", new[] { "キケンキケン ミダレタリズム ハイジョ シマス", "DANGER DANGER. CHAOTIC RHYTHM. ELIMINATING." })
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