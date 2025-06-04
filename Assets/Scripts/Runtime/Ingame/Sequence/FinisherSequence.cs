using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequence : MonoBehaviour
    {
        private EnemyManager _registeredEnemy;

        private void Start()
        {
            FinisherEventRegister(0); //ç≈èâÇÃìGÇìoò^
        }

        public async void FinisherEventRegister(int index)
        {
            var battleScene = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();

            _registeredEnemy = battleScene.EnemyAdmin.GetActiveEnemy();
            _registeredEnemy.OnFinisherable += OnFinisherable;
        }

        private void OnFinisherable()
        {
            var inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            inputBuffer.Finishier.started += Finisher;
        }

        private void Finisher(InputAction.CallbackContext context)
        {
            Debug.Log("Finisher Sequence Start");
        }
    }
}
