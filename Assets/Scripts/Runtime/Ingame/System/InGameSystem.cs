using System;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    public class InGameSystem : MonoBehaviour
    {
        private PhaseEnum _phase;
        
        [SerializeField]
        private InGameData _inGameData;
        
        private PlayerManager _playerManager;
        public PlayerManager PlayerManager => _playerManager;
        
        private void Awake()
        {
             SceneLoader.LoadScene(SceneListEnum.Stage.ToString());

             if (_inGameData)
             {
                 if (_inGameData.PlayerPrefab)
                 {
                     var player = Instantiate(_inGameData.PlayerPrefab);
                     if (!player.TryGetComponent(out _playerManager))
                     {
                         Debug.LogWarning($"Player manager not found in {_inGameData.PlayerPrefab.name}");
                     }
                 }
                 else
                 {
                     Debug.LogWarning($"Player Prefab not found");
                 }
             }
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

            if (0 < _inGameData.EnemyPrefabs.Count)
            {
                Instantiate(_inGameData.EnemyPrefabs[0]);
            }
        }

        private void StartClearPhase()
        {
            Debug.Log("Starting clear phase");
        }
        
        [ContextMenu(nameof(StartBattlePhase))]
        private void StartBattlePhase() => StartBattlePhase(PhaseEnum.Battle1);
    }
}
