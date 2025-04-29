using SymphonyFrameWork.Config;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class CharacterData : ScriptableObject
    {
        public string Name => _name;
        [SerializeField, Tooltip("名前")] string _name;
        
        public float MaxHealth => _maxHealth;
        [SerializeField, Tooltip("最大体力値")] private float _maxHealth;
        
        
    }
}