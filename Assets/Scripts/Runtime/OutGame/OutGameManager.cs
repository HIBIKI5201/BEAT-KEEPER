using BeatKeeper.Runtime.System;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper
{
    /// <summary>
    /// アウトゲームのシーン遷移を管理するクラス
    /// </summary>
    public class OutGameManager : MonoBehaviour
    {
        private const string OutGameSceneName = "OutGame";
        private const string InGameSceneName = "InGame";
        private const string StageSceneName = "Stage";
        [SerializeField] private OutGameUIManager _outGameUIManager;
        [SerializeField] private OutGameSoundManager _outGameSoundManager;
        private PlayerInput _playerInput;
        private InputAction _anyKey;
        private SoundEffectManager _soundEffectManager;


        private async void Awake()
        {
            await SceneLoader.LoadScene(StageSceneName);
        }

        private void Start()
        {
            _soundEffectManager = ServiceLocator.GetInstance<SoundEffectManager>();
            _playerInput = FindAnyObjectByType<PlayerInput>();
            _anyKey = _playerInput.actions["AnyKey"];
            _anyKey.started += OnAnyKeyInput;
        }

        private void OnDisable()
        {
            _anyKey.started -= OnAnyKeyInput;
        }

        /// <summary>
        /// 何らかの入力を受け取ったときに呼び出されるメソッド。
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnAnyKeyInput(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Any key pressed, starting game...");
            _ = LoadInGameSceneAsync();
        }

        /// <summary>
        /// インゲームシーンを読み込むメソッド
        /// </summary>
        /// <returns></returns>
        private async Task LoadInGameSceneAsync()
        {
            _outGameSoundManager.GameStart();
            await _outGameUIManager.GameStart();
            await SceneLoader.UnloadScene(OutGameSceneName);
            await SceneLoader.LoadScene(InGameSceneName);
            SceneLoader.SetActiveScene(InGameSceneName);
        }
    }
}
