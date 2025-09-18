using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeatKeeper
{
    public class BackGroundPhaseReciever : MonoBehaviour
    {
        public void ToPhase1Material()
        {
            if (_database == null) return;

            foreach (var data in _database.Datas)
            {
                data.Original.CopyPropertiesFromMaterial(data.Phase1);
            }

            if (_grobalVolume == null) return;

            _grobalVolume.profile = _phase1Profile;
        }

        public void ToPhase3Material()
        {
            if (_database == null) return;

            foreach (var data in _database.Datas)
            {
                data.Original.CopyPropertiesFromMaterial(data.Phase3);
            }

            if (_grobalVolume == null) return;

            _grobalVolume.profile = _phase3Profile;
        }

        [SerializeField]
        private BackGroundMaterialDataBase _database;

        [Space]
        [SerializeField]
        private Volume _grobalVolume;

        [SerializeField]
        private VolumeProfile _phase1Profile;
        [SerializeField]
        private VolumeProfile _phase3Profile;
    }

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
