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
            FinisherEventRegister(); //�ŏ��̓G��o�^

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
        ///     Finisher�\���̃C�x���g���w������
        /// </summary>
        public async void FinisherEventRegister()
        {
            var battleScene = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();

            _registeredEnemy = battleScene.EnemyAdmin.GetActiveEnemy();
            _registeredEnemy.OnFinisherable += OnFinisherable;
        }

        /// <summary>
        ///     Finisher�\�ɂȂ�����t�B�j�b�V���[���͂��󂯕t����
        /// </summary>
        private void OnFinisherable()
        {
            _inputBuffer.Finishier.started += Finisher;

            var player = ServiceLocator.GetInstance<PlayerManager>();
            if (player)
            {
                player.InputUnregister(); // �v���C���[�̓��͂��ꎞ�I�ɖ�����
            }
        }

        /// <summary>
        ///     Finisher���͂��󂯎�����ۂ̏���
        /// </summary>
        /// <param name="context"></param>
        private void Finisher(InputAction.CallbackContext context)
        {
            Debug.Log("Finisher Sequence Start");

            _inputBuffer.Finishier.started -= Finisher;
        }
    }
}
