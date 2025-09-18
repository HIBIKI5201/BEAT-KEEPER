using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Outgame.UI;
using BeatKeeper.Runtime.System;
using CriWare;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace BeatKeeper.Runtime.Outgame.System
{
    /// <summary>
    /// アウトゲームのシーン遷移および入力を管理するクラス
    /// </summary>
    public class OutGameManager : MonoBehaviour
    {
        /// <summary>
        /// ゲーム状態の列挙型
        /// </summary>
        private enum GameState
        {
            WaitingForStart,     // スタート待ち（Press Any Key状態）
            LanguageSetting,     // 言語設定中
            SubtitleSetting,     // 字幕設定中
            GameStarting,        // ゲーム開始処理中
            Finished             // 完了
        }
        
        private const SceneListEnum OutGameScene = SceneListEnum.OutGame;
        private const SceneListEnum InGameScene = SceneListEnum.InGame;
        private const SceneListEnum StageScene = SceneListEnum.Stage;

        [SerializeField] private OutGameUIManager _outGameUIManager;
        [SerializeField] private CriAtomSource _criAtomSourceSE;
        [SerializeField] private string _bgmName = "Phase1";
        [SerializeField] private float _bgmFadeOutTime = 0.5f;

        private InputBuffer _inputBuffer;
        private bool _look;
        
        // ローカライズ用のテキストデータを管理するマネージャー
        private LocalizeTextManager _localizeTextManager; 
        
        private BGMManager _bgmManager;
        
        // 現在の状態
        private GameState _currentState = GameState.WaitingForStart;

        #region Life cycle

        private async void Awake()
        {
            _look = false;
            await SceneLoader.LoadScene(StageScene.ToString());
            var bgmManager = ServiceLocator.GetInstance<BGMManager>();
            bgmManager.ChangeBGM(_bgmName);

            // サービスロケーターから取得（Systemシーンが読み込まれるまで待つ）
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            _localizeTextManager = await ServiceLocator.GetInstanceAsync<LocalizeTextManager>();
        }

        private void Start()
        {
            Debug.Log("OutGameManager Start");
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            
            // 入力イベントの購読
            RegisterInputEvents();
        }
        
        private void OnDisable()
        {
            UnregisterInputEvents();
        }

        #endregion

        /// <summary>
        /// タイトル画面のみの表示に戻る
        /// </summary>
        private async Task HandleHideSetting()
        {
            // 状態をスタート待ちに戻す
            _currentState = GameState.WaitingForStart;
            
            // 設定UIを隠す
            _outGameUIManager.HideSettingCanvas();

            await Task.Delay(1000);
                
            // すぐにイベントを購読すると、すぐにstartedが反応してしまうため
            // 時間をおいて再度AnyKeyの入力イベントを購読する
            _inputBuffer.AnyKey.started += OnAnyKeyInput;
        }

        /// <summary>
        /// 言語設定画面を開く
        /// </summary>
        private void HandleShowLanguageSetting()
        {
            _currentState = GameState.LanguageSetting;
            _outGameUIManager.ShowSettingCanvas();
        }
        
        /// <summary>
        /// 言語設定の確定処理
        /// </summary>
        private void HandleLanguageSettingConfirm()
        {
            _currentState = GameState.SubtitleSetting;
            
            if (_localizeTextManager == null)
            {
                // 念のため、取得できていなかったらServiceLocatorからもう一度取得できるか試す
                ServiceLocator.GetInstance<LocalizeTextManager>();
            }
            
            // 言語設定を確定
            _localizeTextManager.ChangeLanguage((LanguageType)_outGameUIManager.LanguageId);
            
            // UI更新
            _outGameUIManager.ShowSubtitleCanvas();
        }

        /// <summary>
        /// 字幕設定の確定処理とゲーム開始
        /// </summary>
        private async Task HandleSubtitleSettingConfirm()
        {
            _currentState = GameState.GameStarting;
            
            try
            {
                if (_localizeTextManager == null)
                {
                    // 念のため、取得できていなかったらServiceLocatorからもう一度取得できるか試す
                    ServiceLocator.GetInstance<LocalizeTextManager>();
                }
                
                // 字幕設定を確定
                _localizeTextManager.ChangeSubtitleLanguage((LanguageType)_outGameUIManager.SubtitleId);
                
                // SE再生
                _criAtomSourceSE?.Play();
               
                // BGMフェードアウト
                // NOTE: フェードアウトしながらゲーム開始処理は進んでほしいので、awaitはしない
                _bgmManager.FadeOutBGM(_bgmFadeOutTime).Forget();                

                // ゲーム開始処理
                await _outGameUIManager.GameStart();
                
                // シーン遷移
                await TransitionToInGameScene();
                
                _currentState = GameState.Finished;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ゲーム開始処理中にエラーが発生しました: {ex}");
                // エラー時は状態を戻す
                _currentState = GameState.SubtitleSetting;
            }
        }

        /// <summary>
        /// インゲームシーンへの遷移処理
        /// </summary>
        private async Task TransitionToInGameScene()
        {
            await SceneLoader.UnloadScene(OutGameScene.ToString());
            await SceneLoader.LoadScene(InGameScene.ToString());
            SceneLoader.SetActiveScene(InGameScene.ToString());
        }
        
        /// <summary>
        /// 入力イベントを購読する
        /// </summary>
        private void RegisterInputEvents()
        {
            _inputBuffer.AnyKey.started += OnAnyKeyInput;
            _inputBuffer.LeftNavigation.performed += OnLeftNavigationKeyInput;
            _inputBuffer.RightNavigation.performed += OnRightNavigationKeyInput;
            _inputBuffer.Attack.started += OnAttackKeyInput;
            _inputBuffer.Interact.started += OnInteractKeyInput;
        }
        
        /// <summary>
        /// 入力イベントの購読を解除する
        /// </summary>
        private void UnregisterInputEvents()
        {
            if (_inputBuffer == null) return;
            
            _inputBuffer.AnyKey.started -= OnAnyKeyInput;
            _inputBuffer.LeftNavigation.performed -= OnLeftNavigationKeyInput;
            _inputBuffer.RightNavigation.performed -= OnRightNavigationKeyInput;
            _inputBuffer.Attack.started -= OnAttackKeyInput;
            _inputBuffer.Interact.started -= OnInteractKeyInput;
        }
        
        /// <summary>
        /// 何らかの入力を受け取ったときに呼び出されるメソッド。
        /// </summary>
        private void OnAnyKeyInput(InputAction.CallbackContext callbackContext)
        {
            Debug.Log("OnAnyKeyInput called");
            
            if (_currentState != GameState.WaitingForStart) return;
            
            // 言語設定キャンバスを開く
            _currentState = GameState.LanguageSetting;
            _outGameUIManager.ShowSettingCanvas();
            
            // 不要になるため入力購読を解除
            _inputBuffer.AnyKey.started -= OnAnyKeyInput;
        }

        /// <summary>
        /// アタックキー＝決定キーの入力を受け取ったときに呼び出されるメソッド
        /// </summary>
        private async void OnAttackKeyInput(InputAction.CallbackContext callbackContext)
        {
            switch (_currentState)
            {
                case GameState.LanguageSetting: // 言語設定
                    HandleLanguageSettingConfirm();
                    break;
                    
                case GameState.SubtitleSetting: // 字幕設定
                    await HandleSubtitleSettingConfirm();
                    break;
                    
                default:
                    // その他の状態では何もしない
                    break;
            }
        }

        /// <summary>
        /// インタラクトキー＝キャンセルキーの入力を受け取ったときに呼び出されるメソッド
        /// </summary>
        private async void OnInteractKeyInput(InputAction.CallbackContext callbackContext)
        {
            switch (_currentState)
            {
                // 言語設定の場合、タイトル画面に戻る
                case GameState.LanguageSetting:
                    HandleHideSetting();
                    break;
                
                // 字幕設定の場合、言語設定に戻る
                case GameState.SubtitleSetting:
                    HandleShowLanguageSetting();
                    break;
                    
                default:
                    // その他の状態では何もしない
                    break;
            }
        }

        /// <summary>
        /// ナビゲーションキーの入力を受け取ったときに呼び出されるメソッド
        /// </summary>
        private void OnLeftNavigationKeyInput(InputAction.CallbackContext callbackContext)
        {
            bool isLanguageSetting = _currentState == GameState.LanguageSetting;
            _outGameUIManager.MoveSelection(isLanguageSetting, -1);
        }
        
        /// <summary>
        /// ナビゲーションキーの入力を受け取ったときに呼び出されるメソッド
        /// </summary>
        private void OnRightNavigationKeyInput(InputAction.CallbackContext callbackContext)
        {
            bool isLanguageSetting = _currentState == GameState.LanguageSetting;
            _outGameUIManager.MoveSelection(isLanguageSetting, 1);
        }
    }
}
