using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    /// <summary>
    ///     タイムアシストのデータ
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BattleBuffTimelineData),
        menuName = "BeatKeeper/" + nameof(BattleBuffTimelineData))]
    public class BattleBuffTimelineData : ScriptableObject
    {
        [Serializable]
        public struct BuffData
        {
            [SerializeField, Tooltip("バフが入る拍数"), Min(0)] private int _timing;
            public int Timing => _timing;

            [SerializeField, Min(1)] private float _value;
            public float Value => _value;
        }

        public BuffData[] Data => _data;

        [SerializeField] private BuffData[] _data;

        [ContextMenu("データのソート")]
        private void Sort() => Array.Sort(_data, (a, b) => a.Timing - b.Timing);
    }
}