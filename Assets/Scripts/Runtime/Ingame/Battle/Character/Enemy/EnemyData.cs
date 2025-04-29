using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     エネミーのデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = CharacterData.CHARACTER_DATA_DIRECTORY + "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public float Beat => _beat;
        [SerializeField, Tooltip("ビートの拍子")]
        private float _beat;
    }
}