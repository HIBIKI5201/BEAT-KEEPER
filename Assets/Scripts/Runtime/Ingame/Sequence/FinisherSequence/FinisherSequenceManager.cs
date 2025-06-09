using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceManager : MonoBehaviour
    {
        private InputBuffer _inputBuffer;
        private EnemyManager _registeredEnemy;
        private PlayableDirector _playableDirector;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            if (_playableDirector == null)
            {
                Debug.LogWarning("PlayableDirector component is missing on FinisherSequence.");
            }
        }

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
        ///     Finisher可能になったらフィニッシャー入力を受け付ける
        /// </summary>
        private void OnFinisherable()
        {
            _inputBuffer.Finishier.started += Finisher;

            var player = ServiceLocator.GetInstance<PlayerManager>();
            if (player)
            {
                player.InputUnregister(); // プレイヤーの入力を一時的に無効化
            }

            var text = GetComponentInChildren<Text>();
            if (text)
                text.color = Color.white; // テキストの色を白に変更
        }

        /// <summary>
        ///     Finisher入力を受け取った際の処理
        /// </summary>
        /// <param name="context"></param>
        private void Finisher(InputAction.CallbackContext context)
        {
            Debug.Log("Finisher Sequence Start");

            _inputBuffer.Finishier.started -= Finisher;

            _playableDirector.Play();
        }
    }
}
