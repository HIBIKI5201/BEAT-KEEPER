using System;
using BeatKeeper.Runtime.Ingame.Character;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    [RequireComponent(typeof(PhaseManager))]
    public class InGameSystem : MonoBehaviour
    {
        [SerializeField]
        private InGameData _inGameData;
        
        private PhaseManager _phaseManager;
        public PhaseManager PhaseManager => _phaseManager;
        
        private PlayerManager _playerManager;
        public PlayerManager PlayerManager => _playerManager;
        
        private void Awake()
        {
             SceneLoader.LoadScene(SceneListEnum.Stage.ToString());

             _phaseManager = GetComponent<PhaseManager>();
             
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

             if (_phaseManager)
             {
                 _phaseManager.CurrentPhaseProp.Subscribe(e =>
                 {
                     switch (e)
                     {
                         case PhaseEnum.Approach: StartApproachPhase(); break;
                         case PhaseEnum.Clear: StartClearPhase(); break;
                         
                         case PhaseEnum.Battle1:
                         case PhaseEnum.Battle2:
                         case PhaseEnum.Battle3:
                             StartBattlePhase(e);
                             break;
                     }

                 }).AddTo(destroyCancellationToken);
             }
        }

        public void PhaseStart(PhaseEnum phase) => _phaseManager.TransitionTo(phase);

        private void StartApproachPhase()
        {
            Debug.Log("Starting approach phase");
        }

        private void StartBattlePhase(PhaseEnum phase)
        {
            Debug.Log("Starting battle phase");

            if (0 < _inGameData.EnemyPrefabs.Count)
            {
                var enemy = Instantiate(_inGameData.EnemyPrefabs[0]);
                if (enemy.TryGetComponent(out EnemyManager manager))
                {
                    _playerManager.SetTarget(manager);
                }
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
