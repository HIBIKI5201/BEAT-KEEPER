using SymphonyFrameWork.Config;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクターのベースデータクラス
    /// </summary>
    [CreateAssetMenu(menuName = CHARACTER_DATA_DIRECTORY + "NormalData")]
    public class CharacterData : ScriptableObject
    {
        public const string CHARACTER_DATA_DIRECTORY = "BeatKeeper/CharacterData/";
        
        public string Name => _name;
        [SerializeField, Tooltip("名前")] private string _name;
        
        public float MaxHealth => _maxHealth;
        [SerializeField, Tooltip("最大体力値")] private float _maxHealth;
    }
}