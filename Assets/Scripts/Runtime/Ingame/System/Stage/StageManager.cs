using System;
using BeatKeeper.Runtime.Ingame.System;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using R3;
using UnityEngine;

namespace BeatKeeper
{
    public class StageManager : MonoBehaviour
    {
        private void Start()
        {
            var system = ServiceLocator.GetInstance<InGameSystem>();
            system.PhaseManager.CurrentPhaseProp
                .Subscribe(OnBattle)
                .AddTo(destroyCancellationToken);
        }

        private void OnBattle(PhaseEnum phase)
        {
            switch (phase)
            {
                case PhaseEnum.Battle1: 
                case PhaseEnum.Battle2:
                case PhaseEnum.Battle3:
                
                    break;
            }
        }
    }
}
