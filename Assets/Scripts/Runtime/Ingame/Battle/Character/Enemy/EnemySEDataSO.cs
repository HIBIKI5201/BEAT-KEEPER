using UnityEngine;
using System;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    /// メカのSEのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySEData", menuName = "BeatKeeper/CharacterData/EnemySEData")]
    public class EnemySEDataSO : ScriptableObject
    {
        [SerializeField] private string _punch = "EnemyPunch";
        [SerializeField] private string _beamCharge = "EnemyBeamCharge";
        [SerializeField] private string _beamAttack = "EnemyBeamAttack";
        [SerializeField] private string _smallDamage = "EnemySmallDamage";
        [SerializeField] private string _middleDamage = "EnemyMiddleDamage";
        
        /// <summary>
        /// 殴り
        /// </summary>
        public string Punch => _punch;
        
        /// <summary>
        /// ビーム攻撃の溜め
        /// </summary>
        public string BeamCharge => _beamCharge;

        /// <summary>
        /// ビーム攻撃
        /// </summary>
        public string BeamAttack => _beamAttack;
        
        /// <summary>
        /// 通常攻撃を受けた時の被弾音
        /// </summary>
        public string SmallDamage => _smallDamage;
        
        /// <summary>
        /// チャージ攻撃を受けた時の音
        /// </summary>
        public string MiddleDamage => _middleDamage;
    }
}
