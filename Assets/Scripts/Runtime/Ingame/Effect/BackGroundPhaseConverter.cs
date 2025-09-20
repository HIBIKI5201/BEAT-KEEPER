using UnityEngine;
using UnityEngine.Rendering;

namespace BeatKeeper
{
    public class BackGroundPhaseConverter : MonoBehaviour
    {
        [ContextMenu(nameof(ToPhase1))]
        public void ToPhase1()
        {
            if (_database == null) return;

            foreach (var data in _database.Datas)
            {
                data.Original.CopyPropertiesFromMaterial(data.Phase1);
            }

            if (_grobalVolume == null) return;

            _grobalVolume.profile = _phase1Profile;
        }

        [ContextMenu(nameof(ToPhase3))]
        public void ToPhase3()
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
}
