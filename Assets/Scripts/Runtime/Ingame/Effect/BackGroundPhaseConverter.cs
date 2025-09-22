using UnityEngine;
using UnityEngine.Rendering;

namespace BeatKeeper
{
    public class BackGroundPhaseConverter : MonoBehaviour
    {
        [ContextMenu(nameof(ToPhase1))]
        public void ToPhase1()
        {
            if (_database != null)
            {
                foreach (var data in _database.Datas)
                {
                    data.Original.CopyPropertiesFromMaterial(data.Phase1);
                }
            }

            if (_grobalVolume == null)
            {
                _grobalVolume.profile = _phase1Profile;
            }

            if (_trafficLightManagers != null)
            {
                foreach (var manager in _trafficLightManagers)
                {
                    manager.ToPhase1();
                }
            }

        }

        [ContextMenu(nameof(ToPhase3))]
        public void ToPhase3()
        {
            if (_database != null)
            {
                foreach (var data in _database.Datas)
                {
                    data.Original.CopyPropertiesFromMaterial(data.Phase3);
                }
            }


            if (_grobalVolume != null)
            {
                _grobalVolume.profile = _phase3Profile;
            }

            if (_trafficLightManagers != null)
            {
                foreach (var manager in _trafficLightManagers)
                {
                    manager.ToPhase3();
                }
            }
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

        private TrafficLightManager[] _trafficLightManagers;

        private void Start()
        {
            _trafficLightManagers = FindObjectsByType<TrafficLightManager>(FindObjectsSortMode.None);
        }
    }
}
