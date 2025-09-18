using BeatKeeper.Runtime.Ingame.System;
using CriWare;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using System.ComponentModel.Design;
using UnityEngine;

namespace BeatKeeper
{
    [RequireComponent(typeof(CriAtomSource))]
    public class PhaseSelectorBinder : MonoBehaviour
    {
        [SerializeField]
        private string _selector = "Selector_Phase";

        private CriAtomSource _source;

        private void Awake()
        {
            _source = GetComponent<CriAtomSource>();
        }

        private async void Start()
        {
            BGMManager bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            bgmManager.OnBGMChanged += HandlePhaseChanged;
        }

        private void HandlePhaseChanged(string phase)
        {
            CriAtomExPlayer player = _source.player;
            player.SetSelectorLabel(_selector, phase);
            player.UpdateAll();
        }
    }
}
