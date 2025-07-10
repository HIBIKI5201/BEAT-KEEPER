using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///    プレイヤーのスペシャルエネルギーを管理するシステム
    /// </summary>
    public class SpecialSystem
    {
        public ReadOnlyReactiveProperty<float> SpecialEnergy => _specialEnergy;
        
        public void AddSpecialEnergy(float energy) => _specialEnergy.Value = Mathf.Clamp01(_specialEnergy.Value + energy);

        public void ResetSpecialEnergy() => _specialEnergy.Value = 0;

        private readonly ReactiveProperty<float> _specialEnergy = new();
    }
}
