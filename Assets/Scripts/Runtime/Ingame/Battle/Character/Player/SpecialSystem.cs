using UnityEngine;

namespace BeatKeeper
{
    public class SpecialSystem
    {
        public float SpecialEnergy => _specialEnergy;
        private float _specialEnergy;
        
        public void AddSpecialEnergy(float energy) => _specialEnergy = Mathf.Clamp(_specialEnergy + energy, 0, 1);
        
        public void ResetSpecialEnergy() => _specialEnergy = 0;
    }
}
