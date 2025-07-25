using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     プレイヤーのデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = CHARACTER_DATA_DIRECTORY + "PlayerData")]
    public class PlayerData : CharacterData
    {
        #region 攻撃パラメータ
        public float ComboAttackPower => _comboAttackPower;
        public float PerfectCriticalDamage => _perfectCriticalDamage;
        public float[] ComboScoreScale => _comboScoreScale;
        public float ComboResetTime => _comboRisetTime;
        public float ChargeAttackPower => _chargeAttackPower;
        public float ChargeAttackTime => _chargeAttackTime;

        public float PerfectRange => _perfectRange;
        public float GoodRange => _goodRange;
        #endregion

        #region リズムパラメータ
        public float FlowZoneThreshold => _flowZoneThreshold;
        public int FlowZoneDuration => _flowZoneDuration;
        #endregion

        #region 回避パラメータ
        public float PerfectAvoidRnage => _perfectAvoidRange;
        public float GoodAvoidRange => _goodAvoidRange;
        public float AvoidInvincibilityTime => _avoidInvincibilityTime;

        public float HitStunTime => _hitStunTime;
        public float ChargeHitStunTime => _chargeHitStunTime;

        public int AvoidScore => _avoidScore;
        public float AvoidPerfectScoreScale => _avoidPerfectScoreScale;
        #endregion

        #region スキルパラメータ
        public float PerfectSkillRange => _perfectSkillRange;
        public float GoodSkillRange => _goodSkillRange;

        public float SkillDuration => _skillDuration;
        public float SkillStrangth => _skillStrangth;
        #endregion

        [Header("攻撃 パラメータ")]

        [Space(5), DisplayText("コンボ攻撃")]

        [SerializeField, Tooltip("コンボの攻撃力")]
        private float _comboAttackPower = 100f;

        [Space(10)]
        [SerializeField, Min(1), Tooltip("パーフェクト時のダメージ倍率")]
        private float _perfectCriticalDamage = 1f;

        [Space(10)]
        [SerializeField, Tooltip("コンボごとのスコア取得量")]
        private float[] _comboScoreScale = { 100, 100, 100 };

        [SerializeField, Min(0), Tooltip("コンボ維持時間")]
        private float _comboRisetTime = 3;

        [Space(5), DisplayText("チャージ攻撃")]
        [SerializeField, Tooltip("チャージの攻撃力")]
        private float _chargeAttackPower = 500;
        [SerializeField, Tooltip("最大チャージになるまでの拍数")]
        private float _chargeAttackTime = 3;

        [SerializeField, Range(0, 1), Tooltip("パーフェクトヒットの範囲")]
        private float _perfectRange = 0.1f;
        [SerializeField, Range(0, 1), Tooltip("パーフェクトヒットの範囲")]
        private float _goodRange = 0.5f;

        [Header("リズム パラメータ")]

        [SerializeField, Min(1), Tooltip("フローゾーン突入の敷居")]
        private float _flowZoneThreshold = 10;

        [SerializeField, Tooltip("フローゾーンの持続時間")]
        private int _flowZoneDuration = 16;

        [Header("回避 パラメータ")]

        [SerializeField, Range(0, 1), Tooltip("回避の範囲")]
        private float _perfectAvoidRange = 0.3f;

        [SerializeField, Range(0, 1), Tooltip("回避の範囲")]
        private float _goodAvoidRange = 0.5f;

        [SerializeField, Tooltip("無敵時間の拍数")]
        private float _avoidInvincibilityTime = 2;

        [SerializeField, Tooltip("ヒット時のスタン時間")]
        private float _hitStunTime = 1f;
        [SerializeField, Tooltip("チャージヒット時のスタン時間")]
        private float _chargeHitStunTime = 2f;

        [SerializeField, Tooltip("回避時のスコア")]
        private int _avoidScore = 100;
        [SerializeField, Range(1, 2), Tooltip("パーフェクト回避スコア倍率")]
        private float _avoidPerfectScoreScale;

        [Header("スキル パラメータ")]
        [SerializeField, Range(0, 1)]
        private float _perfectSkillRange = 0.5f;
        [SerializeField, Range(0, 1)]
        private float _goodSkillRange = 0.7f;

        [SerializeField, Tooltip("スキルの効果時間")]
        private float _skillDuration = 5f;

        [SerializeField, Tooltip("スキルの効果量"), Min(1)]
        private float _skillStrangth = 1.5f;
    }
}
