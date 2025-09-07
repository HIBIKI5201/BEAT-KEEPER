using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクターのベースデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "NormalData", menuName = CHARACTER_DATA_DIRECTORY + "NormalData")]
    public class CharacterData : ScriptableObject
    {
        public string Name => _name;

        protected const string CHARACTER_DATA_DIRECTORY = "BeatKeeper/CharacterData/";

        [SerializeField, Tooltip("名前")] private string _name = "empty name";
    }
}