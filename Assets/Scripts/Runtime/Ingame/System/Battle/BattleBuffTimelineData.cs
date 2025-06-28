using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
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

        [SerializeField] private BuffData[] _data;
        public BuffData[] Data => _data;

        [ContextMenu("データのソート")]
        private void Sort() => Array.Sort(_data, (a, b) => a.Timing - b.Timing);
    }
}