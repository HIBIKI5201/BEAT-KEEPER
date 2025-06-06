using BeatKeeper.Runtime.Ingame.Battle;
using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    [CreateAssetMenu(fileName = "ChartData", menuName = "BeatKeeper/ChartData", order = 1)]
    public class ChartData : ScriptableObject
    {
        private void Awake()
        {
            if (_chart.Length != 32)
                Debug.LogWarning("譜面データの長さが不適切です。");
        }

        public ChartDataElement[] Chart => _chart;
        [SerializeField, Tooltip("ビートの拍子")] private ChartDataElement[] _chart = new ChartDataElement[32];

        /// <summary>
        /// 指定したインデックスが攻撃かどうかを判定する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsAttack(int index)
        {
            index %= _chart.Length;
            return _chart[index].AttackKind != AttackKindEnum.None;
        }

        [Serializable]
        public struct ChartDataElement
        {
            public ChartDataElement(int x)
            {
                AttackKind = AttackKindEnum.Normal;
                Position = Vector2.zero;
            }

            public AttackKindEnum AttackKind;
            public Vector2 Position;
        }
    }
}
