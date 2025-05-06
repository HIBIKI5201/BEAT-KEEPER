using System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    public class InGameSystem : MonoBehaviour
    {
        private PhaseEnum _phase;
        
        private void Awake()
        {
             SceneLoader.LoadScene(SceneListEnum.Stage.ToString());
        }

        public void PhaseStart(PhaseEnum phase)
        {
            switch (_phase)
            {
                case PhaseEnum.Approach:
                    StartApproachPhase();
                    break;
                
                case PhaseEnum.Battle1:
                case PhaseEnum.Battle2:
                case PhaseEnum.Battle3:
                    StartBattlePhase(phase);
                    break;
                
                case PhaseEnum.Clear:
                    StartClearPhase();
                    break;
            }
        }

        private void StartApproachPhase()
        {
            Debug.Log("Starting approach phase");
        }

        private void StartBattlePhase(PhaseEnum phase)
        {
            Debug.Log("Starting battle phase");
        }

        private void StartClearPhase()
        {
            Debug.Log("Starting clear phase");
        }
    }
}
