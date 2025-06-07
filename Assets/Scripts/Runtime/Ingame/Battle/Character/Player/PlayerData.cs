using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = CHARACTER_DATA_DIRECTORY + "PlayerData")]
    public class PlayerData : CharacterData
    {
        #region 攻撃パラメータ
        [Header("攻撃 パラメータ")]

        [DisplayText("コンボ攻撃")]

        [SerializeField, Tooltip("コンボの攻撃力")] private float _comboAttackPower = 100f;
        public float ComboAttackPower => _comboAttackPower;

        [Space(10)]
        [SerializeField, Min(1), Tooltip("パーフェクト時のダメージ倍率")] private float _perfectCriticalDamage = 1f;
        public float PerfectCriticalDamage => _perfectCriticalDamage;

        [Space(10)]
        [SerializeField] private float[] _comboScoreScale = { 100, 100, 100 };
        public float[] ComboScoreScale => _comboScoreScale;

        [SerializeField, Min(0), Tooltip("コンボ維持時間")] private float _comboRisetTime = 3;
        public float ComboResetTime => _comboRisetTime;

        [DisplayText("チャージ攻撃")]
        [SerializeField, Tooltip("最大チャージになるまでの拍数")]
        private float _chargeAttackTime = 3;
        public float ChargeAttackTime => _chargeAttackTime;
        #endregion

        #region リズムパラメータ
        [Header("リズム パラメータ")]
        
        [SerializeField, Range(0,1), Tooltip("パーフェクトヒットの範囲")]
        private float _perfectRange = 0.1f;
        public float PerfectRange => _perfectRange;

        [SerializeField, Range(0, 1), Tooltip("パーフェクトヒットの範囲")]
        private float _goodRange = 0.5f;
        public float GoodRange => _goodRange;
        
        [SerializeField, Min(1), Tooltip("フローゾーン突入の敷居")] private float _flowZoneThreshold = 10;
        public float FlowZoneThreshold => _flowZoneThreshold;
        #endregion


        [SerializeField,Tooltip("無敵時間の拍数")]
        private float _avoidInvincibilityTime = 2;
        public float AvoidInvincibilityTime => _avoidInvincibilityTime;
    }
}
