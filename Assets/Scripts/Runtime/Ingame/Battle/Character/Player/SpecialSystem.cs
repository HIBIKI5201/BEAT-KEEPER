using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class SpecialSystem
    {
        public ReadOnlyReactiveProperty<float> SpecialEnergy => _specialEnergy;
        private readonly ReactiveProperty<float> _specialEnergy = new();
        
        public void AddSpecialEnergy(float energy) => _specialEnergy.Value = Mathf.Clamp(_specialEnergy.Value + energy, 0, 1);
        
        public void ResetSpecialEnergy() => _specialEnergy.Value = 0;
    }
}
