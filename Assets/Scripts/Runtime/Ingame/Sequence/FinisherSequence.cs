using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequence : MonoBehaviour
    {
        private InputBuffer _inputBuffer;
        private EnemyManager _registeredEnemy;

        private void Start()
        {
            FinisherEventRegister(); //最初の敵を登録

            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
        }

        private void OnDestroy()
        {
            if (_registeredEnemy != null)
            {
                _registeredEnemy.OnFinisherable -= OnFinisherable;
            }

            if (_inputBuffer)
                _inputBuffer.Finishier.started -= Finisher;
        }

        /// <summary>
        ///     Finisher可能時のイベントを購買する
        /// </summary>
        public async void FinisherEventRegister()
        {
            var battleScene = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();

            _registeredEnemy = battleScene.EnemyAdmin.GetActiveEnemy();
            _registeredEnemy.OnFinisherable += OnFinisherable;
        }

        /// <summary>
        ///     Finisher可能時にフィニッシャー入力を受け付ける
        /// </summary>
        private void OnFinisherable()
        {
            _inputBuffer.Finishier.started += Finisher;
        }

        /// <summary>
        ///     Finisher入力を受け取った際の処理
        /// </summary>
        /// <param name="context"></param>
        private void Finisher(InputAction.CallbackContext context)
        {
            Debug.Log("Finisher Sequence Start");

            _inputBuffer.Finishier.started -= Finisher;
        }
    }
}
