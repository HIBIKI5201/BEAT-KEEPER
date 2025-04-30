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
        
        public float ComboResetTime;
        [SerializeField, Tooltip("コンボ維持時間")] private float _comboRisetTime = 3;
    }
}
