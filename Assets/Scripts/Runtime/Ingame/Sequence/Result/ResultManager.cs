using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using CriWare;

namespace BeatKeeper
{
    public class ResultManager : MonoBehaviour
    {
	    [SerializeField] private ResultUIController _resultUIController;
        [SerializeField] private CanvasGroup _resultPanel;
        [SerializeField] private CanvasGroup _endPanel;
        [SerializeField] private float _fadeDuration = 0.5f;
		[SerializeField] private float _moveXDistance = 20f;
		
		[Header("ボイスの設定")]
		[SerializeField] private string _endingVoice = "voice_ending";

		[Header("SEの設定")]
		[SerializeField] private string _operationSe = "Result_Operation";

        private InputBuffer _inputBuffer;
		private Vector3 _originalResultPanelPosition; 
		private CriAtomExPlayback _playback;
        private bool _isEndPanelOpen;
        private bool _lock;

		private bool _isValid => _resultPanel != null && _endPanel != null;

		private void Awake()
        {
            // 元の位置を保存
            if (_resultPanel != null)
            {
                _originalResultPanelPosition = _resultPanel.transform.localPosition;
            }
        }        

		/// <summary>
        /// リザルトを表示する
        /// </summary>
		public void ResultShow()
        {
			if(_endPanel != null)
			{
				// 2枚目のパネルを確実に非表示にしておく
            	_endPanel.alpha = 0;
			} 
			
			if(_resultPanel != null)
			{
				// 開始位置を左にオフセット
                Vector3 startPosition = _originalResultPanelPosition + Vector3.left * _moveXDistance;
                _resultPanel.transform.localPosition = startPosition;

				// リザルトパネルを表示する
            	_resultPanel.DOFade(1, _fadeDuration);
            	_resultPanel.transform.DOLocalMove(_originalResultPanelPosition, _fadeDuration);
			}
			
			if (_resultUIController != null)
			{
				// リザルト表示演出を始める
				_resultUIController.SetResult();
			}
        }

        /// <summary>
        /// 演出が完了した際に呼び出されるメソッド。
        /// </summary>
        public void AllProductionCompleted()
        {
            Debug.Log("All productions completed in ResultManager.");
            _lock = false;

			// キー入力があったときのイベントを登録
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
			if(_inputBuffer != null)
			{
				_inputBuffer.AnyKey.started += OnAnyKeyInput;
			}
        }

        private void OnDisable()
        {
			if(_inputBuffer != null)
			{
				_inputBuffer.AnyKey.started -= OnAnyKeyInput;
			}

			// ボイスが再生中であれば止める
			_playback.Stop();
        }

		/// <summary>
        /// キー入力を受け取ったときに呼ばれるメソッド
        /// </summary>
        private void OnAnyKeyInput(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("Any key input detected in ResultManager.");

			SoundEffectManager.PlaySoundEffect(_operationSe);

            // もし最後のパネルが表示されていない・かつ2枚のパネルの参照が取得できている場合
            if (!_isEndPanelOpen && _isValid)
            {
                _isEndPanelOpen = true;
                
                // パネルを入れ替える
                _resultPanel.DOFade(0, _fadeDuration);
                _endPanel.DOFade(1, _fadeDuration);

                // ランクのボイス再生を止める
                _resultUIController.StopVoice();
                
                _playback = VoiceManager.PlayVoice(_endingVoice);
				return;
            }
            
            // 最後のパネルが表示済みだったらロードアウト処理を実行
			// NOTE: パネルの一方がnullだった場合はこちらのif文のみが実行されて、エラーが出ないようにする
            if (callbackContext.started && _lock == false)
            {
                _lock = true;
                _ = LoadOutGameScene();
            }
        }

        public async Task LoadOutGameScene()
        {
            ServiceLocator.GetInstance<BGMManager>().ClearAllTimingActions();
            await SceneLoader.UnloadScene(SceneListEnum.InGame.ToString());
            await SceneLoader.UnloadScene(SceneListEnum.Stage.ToString());
            await SceneLoader.UnloadScene(SceneListEnum.Battle.ToString());
            await SceneLoader.LoadScene(SceneListEnum.OutGame.ToString());
            SceneLoader.SetActiveScene(SceneListEnum.OutGame.ToString());
        }
    }
}
