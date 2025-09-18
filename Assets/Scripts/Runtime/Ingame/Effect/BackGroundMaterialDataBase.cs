using System;
using UnityEngine;

namespace BeatKeeper
{
    [CreateAssetMenu(fileName = nameof(BackGroundMaterialDataBase),
        menuName = "BeatKeeper/" + nameof(BackGroundMaterialDataBase))]
    public class BackGroundMaterialDataBase : ScriptableObject
    {
        public BackGroundMaterialData this[int index] => _backGroundDatas[index];
        public BackGroundMaterialData[] Datas => _backGroundDatas;

        [SerializeField]
        private BackGroundMaterialData[] _backGroundDatas;
    }

    [Serializable]
    public struct BackGroundMaterialData
    {
        public Material Original => _ogirinalMaterial;
        public Material Phase1 => _phase1Material;
        public Material Phase3 => _phase3Material;

        [SerializeField]
        private Material _ogirinalMaterial;

        [SerializeField]
        private Material _phase1Material;
        [SerializeField]
        private Material _phase3Material;
    }
}
