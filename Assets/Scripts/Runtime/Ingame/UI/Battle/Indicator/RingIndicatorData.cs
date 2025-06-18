using BeatKeeper.Runtime.Ingame.Battle;
using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     リングインジケーターのデータベース
    /// </summary>
    [CreateAssetMenu(fileName = nameof(RingIndicatorData), menuName = "BeatKeeper/UI/RingIndicatorData")]
    public class RingIndicatorData : ScriptableObject
    {
        public RingData[] RingDatas => _ringDatas;

        [SerializeField, Tooltip("リングのデータ群")]
        private RingData[] _ringDatas;
    }

    /// <summary>
    ///     リングインジケーターの情報
    /// </summary>
    [Serializable]
    public class RingData
    {
        public ChartKindEnum AttackKind => _attackKind;
        public GameObject RingPrefab => _ringPrefab;
        public int DefaultCapacity => _defaultCapacity;

        [SerializeField] private ChartKindEnum _attackKind;

        [SerializeField] private GameObject _ringPrefab;

        [SerializeField]
        [Tooltip("リングの事前用意数（ある程度の同時出現数を入力）")]
        private int _defaultCapacity = 3;
    }
}
