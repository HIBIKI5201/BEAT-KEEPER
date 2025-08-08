using BeatKeeper.Runtime.System;
using DG.Tweening;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BeatKeeper
{
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private float _fadeDuration = 0.5f;

        private InputBuffer _inputBuffer;

        private bool _lock;
        public void ResultShow()
        {
            var canvasGroup = _resultPanel.GetComponent<CanvasGroup>();
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, _fadeDuration);
        }

        /// <summary>
        /// 演出が完了した際に呼び出されるメソッド。
        /// </summary>
        public void AllProductionCompleted()
        {
            Debug.Log("All productions completed in ResultManager.");
            _lock = false;
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _inputBuffer.AnyKey.started += OnAnyKeyInput;
        }

        private void OnDisable()
        {
            _inputBuffer.AnyKey.started -= OnAnyKeyInput;
        }

        private void OnAnyKeyInput(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Any key input detected in ResultManager.");
            if (callbackContext.started && _lock == false)
            {
                _lock = true;
                _ = LoadOutGameScene();
            }
        }

        public async Task LoadOutGameScene()
        {
            await SceneLoader.UnloadScene(SceneListEnum.InGame.ToString());
            await SceneLoader.LoadScene(SceneListEnum.OutGame.ToString());
            SceneLoader.SetActiveScene(SceneListEnum.OutGame.ToString());
        }
    }
}
