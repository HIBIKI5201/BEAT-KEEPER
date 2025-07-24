using BeatKeeper.Runtime.Outgame.UI;
using BeatKeeper.Runtime.System;
using CriWare;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Outgame.System
{
    /// <summary>
    /// アウトゲームのシーン遷移および入力を管理するクラス
    /// </summary>
    public class OutGameManager : MonoBehaviour
    {
        private const string OutGameScene = "OutGame";
        private const SceneListEnum InGameScene = SceneListEnum.InGame;
        private const SceneListEnum StageScene = SceneListEnum.Stage;

        [SerializeField] private OutGameUIManager _outGameUIManager;
        [SerializeField]private CriAtomSource _criAtomSourceSE;

        private InputBuffer _inputBuffer;

        private async void Awake()
        {
            await SceneLoader.LoadScene(StageScene.ToString());
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _inputBuffer.AnyKey.started += OnAnyKeyInput;
        }

        private void OnDisable()
        {
            _inputBuffer.AnyKey.started -= OnAnyKeyInput;
        }

        /// <summary>
        /// 何らかの入力を受け取ったときに呼び出されるメソッド。
        /// </summary>
        private void OnAnyKeyInput(InputAction.CallbackContext callbackContext)
        {
            _ = LoadInGameSceneAsync();
        }

        /// <summary>
        /// インゲームシーンを読み込むメソッド
        /// </summary>
        private async Task LoadInGameSceneAsync()
        {
            _criAtomSourceSE.Play();
            await _outGameUIManager.GameStart();
            await SceneLoader.UnloadScene(OutGameScene);
            await SceneLoader.LoadScene(InGameScene.ToString());
            SceneLoader.SetActiveScene(InGameScene.ToString());
        }
    }
}
