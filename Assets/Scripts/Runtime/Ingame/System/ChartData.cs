using BeatKeeper.Runtime.Ingame.Battle;
using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    [CreateAssetMenu(fileName = "ChartData", menuName = "BeatKeeper/ChartData", order = 1)]
    public partial class ChartData : ScriptableObject
    {
        private const int CHART_LENGTH = 64;

        private void Awake()
        {
            if (_chart.Length != CHART_LENGTH)
                Debug.LogWarning($"{name}の譜面データの長さが不適切です。");
        }

        private void Reset()
        {
            _chart = new ChartDataElement[CHART_LENGTH];
#if UNITY_EDITOR
            _visible = new bool[CHART_LENGTH];
#endif
        }

        public ChartDataElement[] Chart => _chart;
        [SerializeField, Tooltip("ビートの拍子")]
        private ChartDataElement[] _chart = new ChartDataElement[CHART_LENGTH];

        /// <summary>
        /// 指定したインデックスが攻撃かどうかを判定する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsEnemyAttack(int index)
        {
            index %= _chart.Length;

            var attackKind =
                ChartKindEnum.Normal | ChartKindEnum.Charge | ChartKindEnum.Super; //敵の攻撃

            return (_chart[index].AttackKind & attackKind) != 0;
        }

        [Serializable]
        public struct ChartDataElement
        {
            public ChartDataElement(int x)
            {
                AttackKind = ChartKindEnum.Normal;
                Position = Vector2.zero;
            }

            public ChartKindEnum AttackKind;
            public Vector2 Position;
        }

        //エディタ用データ
#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        private bool[] _visible = new bool[CHART_LENGTH];
#endif
    }
}
