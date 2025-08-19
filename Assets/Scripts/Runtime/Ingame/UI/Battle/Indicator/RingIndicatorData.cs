using BeatKeeper.Runtime.Ingame.Battle;
using SymphonyFrameWork.Attribute;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     リングインジケーターのデータベース
    /// </summary>
    [CreateAssetMenu(fileName = nameof(RingIndicatorData), menuName = "BeatKeeper/UI/RingIndicatorData")]
    public class RingIndicatorData : ScriptableObject
    {
        public RingData[] RingDatas => _ringDatas;

        public RingData GetRingData(ChartKindEnum attackKind)
        {
            foreach (var ringData in _ringDatas)
            {
                if (ringData.AttackKind == attackKind)
                {
                    return ringData;
                }
            }

            Debug.LogWarning($"No RingData found for attack kind: {attackKind}");
            return null;
        }

        public bool TryGetRingData(ChartKindEnum attackKind, out RingData data)
        {
            foreach (var ringData in _ringDatas)
            {
                if (ringData.AttackKind == attackKind)
                {
                    data = ringData;
                    return true;
                }
            }

            Debug.LogWarning($"No RingData found for attack kind: {attackKind}");
            data = null;
            return false;
        }

        [SerializeField, Tooltip("リングのデータ群")]
        private RingData[] _ringDatas;

        private void OnValidate()
        {
            for (int i = 0; i < _ringDatas.Length; i++)
            {
                RingData data = _ringDatas[i];

                if (data.RingPrefab == null)
                {
                    Debug.LogWarning($"RingPrefab is null for RingData at index {i}. Please assign a valid prefab.");
                    continue;
                }

                if (data.RingPrefab.TryGetComponent(out RingIndicatorBase ring))
                {
                    _ringDatas[i] = new RingData(
                        data.AttackKind,
                        data.RingPrefab,
                        data.DefaultCapacity,
                        ring.EffectLength
                    );
                }
            }
        }
    }

    /// <summary>
    ///     リングインジケーターの情報
    /// </summary>
    [Serializable]
    public class RingData
    {
        public RingData(ChartKindEnum attackKind, GameObject ringPrefab, int defaultCapacity, int effectLength)
        {
            _attackKind = attackKind;
            _ringPrefab = ringPrefab;
            _defaultCapacity = defaultCapacity;
            _effectLength = effectLength;
        }

        public ChartKindEnum AttackKind => _attackKind;
        public GameObject RingPrefab => _ringPrefab;
        public int DefaultCapacity => _defaultCapacity;
        public int EffectLength => _effectLength;

        [SerializeField] private ChartKindEnum _attackKind;

        [SerializeField] private GameObject _ringPrefab;

        [SerializeField]
        [Tooltip("リングの事前用意数（ある程度の同時出現数を入力）")]
        private int _defaultCapacity = 3;

        [SerializeField, ReadOnly]
        private int _effectLength;
    }
}
