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
                Debug.LogWarning("���ʃf�[�^�̒������s�K�؂ł��B");
        }

        public ChartDataElement[] Chart => _chart;
        [SerializeField, Tooltip("�r�[�g�̔��q")] private ChartDataElement[] _chart = new ChartDataElement[32];

        /// <summary>
        /// �w�肵���C���f�b�N�X���U�����ǂ����𔻒肷��
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsEnemyAttack(int index)
        {
            index %= _chart.Length;

            var attackKind = ChartKindEnum.Normal | ChartKindEnum.Charge | ChartKindEnum.Super; //�G�̍U��

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
    }
}
