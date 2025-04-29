using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = CharacterData.CharacterDataDirectory + "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public float Beat => _beat;
        [SerializeField, Tooltip("ビートの拍子")]
        private float _beat;
    }
}