using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     エネミーのデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = CharacterData.CHARACTER_DATA_DIRECTORY + "EnemyData")]
    public class EnemyData : CharacterData
    {
        private void Awake()
        {
             if (_beat.Length != 32)
                 Debug.LogWarning("譜面データの長さが不適切です。");
        }

        public bool[] Beat => _beat;
        [SerializeField, Tooltip("ビートの拍子")]
        private bool[] _beat = new bool[32];
    }
}