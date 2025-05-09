using System;
using SymphonyFrameWork.Config;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクターのベースデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "NormalData", menuName = CHARACTER_DATA_DIRECTORY + "NormalData")]
    public class CharacterData : ScriptableObject
    {
        public const string CHARACTER_DATA_DIRECTORY = "BeatKeeper/CharacterData/";
        
        public string Name => _name;
        [SerializeField, Tooltip("名前")] private string _name = "empty name";
        
        # region 未使用パラメータ
        [Obsolete("未使用パラメータ")]
        public float AttackPower => _attackPower;
        [Tooltip("攻撃力")] private float _attackPower = 10;
        
        [Obsolete("未使用パラメータ")]
        public float MaxHealth => _maxHealth;
        [Tooltip("最大体力値")] private float _maxHealth = 100;
        #endregion
    }
}