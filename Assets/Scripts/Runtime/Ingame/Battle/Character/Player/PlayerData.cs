using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = CHARACTER_DATA_DIRECTORY + "PlayerData")]
    public class PlayerData : CharacterData
    {
        [Space]
        
        [SerializeField] private float _firstAttackPower = 100;
        public float FirstAttackPower => _firstAttackPower;
        
        [SerializeField] private float _secondAttackPower = 100;
        public float SecondAttackPower => _secondAttackPower;
        
        [SerializeField] private float _thirdAttackPower = 100;
        public float ThirdAttackPower => _thirdAttackPower;

        [Space]
        
        [SerializeField, Range(0,1), Tooltip("リズム共鳴の範囲")]
        private float _resonanceRange = 0.5f;
        public float ResonanceRange => _resonanceRange;
        
        [SerializeField, Min(1), Tooltip("リズム共鳴時のダメージ倍率")] private float _resonanceCriticalDamage = 1f;
        public float ResonanceCriticalDamage => _resonanceCriticalDamage;
        
        [SerializeField, Min(1), Tooltip("フローゾーン突入の敷居")] private float _flowZoneThreshold = 10;
        public float FlowZoneThreshold => _flowZoneThreshold;
        
        [Space] 
        
        [SerializeField, Min(0), Tooltip("コンボ維持時間")] private float _comboRisetTime = 3;
        public float ComboResetTime => _comboRisetTime;
    }
}
