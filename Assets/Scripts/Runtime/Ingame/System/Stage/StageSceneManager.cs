using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Stsge
{
    /// <summary>
    ///     ステージシーンのマネージャー
    /// </summary>
    public class StageSceneManager : MonoBehaviour
    {
        public CinemachineCamera PlayerCamera => _playerCamera;

        [SerializeField, Tooltip("プレイヤーカメラ")]
        private CinemachineCamera _playerCamera;

        private async void Start()
        {

            var phaseManager = await ServiceLocator.GetInstanceAsync<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnPhaseChanged)
                    .AddTo(destroyCancellationToken);
            }
        }

        private async void OnPhaseChanged(PhaseEnum phase)
        {
            if (phase == PhaseEnum.Battle && _playerCamera)
            {
                //カメラを敵に向ける
                _playerCamera.LookAt =
                    (await ServiceLocator.GetInstanceAsync<BattleSceneManager>())
                    .EnemyAdmin.GetActiveEnemy().transform;
            }

        }
    }
}
