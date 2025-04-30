using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     エネミーのデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = CharacterData.CHARACTER_DATA_DIRECTORY + "EnemyData")]
    public class EnemyData : CharacterData
    {
        public bool[] Beat => _beat;
        [SerializeField, Tooltip("ビートの拍子")]
        private bool[] _beat = new bool[32];
    }
}