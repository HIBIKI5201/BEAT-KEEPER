using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public class PlayerData : CharacterData
    {
        [Space]
        
        [SerializeField] private float _firstAttackPower;
        public float FirstAttackPower => _firstAttackPower;
        
        [SerializeField] private float _secondAttackPower;
        public float SecondAttackPower => _secondAttackPower;
        
        [SerializeField] private float _thirdAttackPower;
        public float ThirdAttackPower => _thirdAttackPower;
    }
}
