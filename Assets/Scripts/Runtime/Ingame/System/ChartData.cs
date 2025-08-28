using BeatKeeper.Runtime.Ingame.Battle;
using System;
using UnityEngine;
using BeatKeeper;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    ///     譜面データを管理するクラス
    /// </summary>
    [CreateAssetMenu(fileName = "ChartData", menuName = "BeatKeeper/ChartData", order = 1)]
    public partial class ChartData : ScriptableObject
    {
        private const int CHART_LENGTH = 128;

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

        public ChartDataElement this[int index] => _chart[index % _chart.Length];

        public ChartDataElement[] Chart => _chart;
        [SerializeField, Tooltip("ビートの拍子")]
        private ChartDataElement[] _chart = new ChartDataElement[CHART_LENGTH];
        
        // 範囲ノーツの終点座標を管理（インデックス -> 終点座標）
        [SerializeField, Tooltip("範囲ノーツの終点座標")]
        private SerializableDictionary<int, Vector2> _rangeEndPositions = new SerializableDictionary<int, Vector2>();
        
        /// <summary>
        /// 指定したインデックスが攻撃かどうかを判定する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsEnemyAttack(int index)
        {
            index %= _chart.Length;

            var attackKind =
                ChartKindEnum.Normal | ChartKindEnum.Charge; //敵の攻撃

            return (_chart[index].AttackKind & attackKind) != 0;
        }
        
        /// <summary>
        /// 指定した拍数に終点ノーツの座標が設定されているかどうか
        /// </summary>
        public bool HasEndPosition(int index)
        {
            index %= _chart.Length;
            return _rangeEndPositions.ContainsKey(index);
        }

        /// <summary>
        /// 指定した拍数の終点ノーツの座標を取得する
        /// NOTE: Valueが入手出来なかった場合はnullを返す
        /// </summary>
        public Vector2? GetRangeEndPosition(int index)
        {
            index %= _chart.Length;
            return _rangeEndPositions.TryGetValue(index, out var endPos) ? endPos : null;
        }

        /// <summary>
        /// 範囲ノーツの終点座標を設定する（エディタ用）
        /// </summary>
        public void SetRangeEndPosition(int index, Vector2 endPosition)
        {
            index %= _chart.Length;
            _rangeEndPositions[index] = endPosition;
        }

        /// <summary>
        /// 範囲ノーツを削除する（エディタ用）
        /// </summary>
        /// <param name="index"></param>
        public void RemoveRangeEndPosition(int index)
        {
            index %= _chart.Length;
            _rangeEndPositions.Remove(index);
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

        [ContextMenu("Convert")]
        private void Convert()
        {
            Array.Resize(ref _chart, CHART_LENGTH);
            Array.Resize(ref _visible, CHART_LENGTH);
        }
#endif
    }
}
