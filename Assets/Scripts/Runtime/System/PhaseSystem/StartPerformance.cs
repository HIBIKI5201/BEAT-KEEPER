using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartPerformance : MonoBehaviour
    {
        [SerializeField] private BGMChanger _bgmChanger;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private InGameUIManager _uiManager;
        [SerializeField] private Tmp_EnemyMove _enemyMove;
        
        // 仮
        [SerializeField] private PhaseEnum _startPhase = PhaseEnum.Movie;
        private float _defaultTextPosY;
        
        private PhaseEnum _nextPhase;
        private PhaseManager _phaseManager;
        private MusicEngineHelper _musicEngineHelper;
        private int _count;

        private async void Start()
        {
            var multiSceneManager = ServiceLocator.GetInstance<MultiSceneManager>();
            if (multiSceneManager)
            {
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Stage);
            }
            
            _cameraManager = FindAnyObjectByType<CameraManager>();
            
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
            _cameraManager.ChangeCamera(CameraType.StartPerformance);
            _cameraManager.ChangeTarget(CameraAim.Player);
            TransitionStart(_startPhase); //TODO: テスト用。アプローチフェーズの処理と連携して呼び出すようにしたい
        }

        /// <summary>
        /// 遷移処理を開始する
        /// </summary>
        public void TransitionStart(PhaseEnum nextPhase)
        {
            _musicEngineHelper.OnJustChangedBar += Counter;
            _nextPhase = nextPhase;
        }

        /// <summary>
        /// 拍数を元に遷移処理を行う
        /// </summary>
        private void Counter()
        {
            _count++;
            if(_count == 1) ZoomInOnEnemy();
            else if(_count == 9) PrepareForBattle();
            else if(_count == 13) SetupWeaponStance();
            else if(_count == 17) ActivateBattlePhase();
        }
        
        /// <summary>
        /// 遷移1拍目～8拍目
        /// </summary>
        private void ZoomInOnEnemy()
        {
            _cameraManager.ChangeTarget(CameraAim.FirstBattleEnemy);
            _uiManager.ShowEncounterText(1); // 遭遇時のテキストを表示する
        }
        
        /// <summary>
        /// 遷移9拍目～12拍目
        /// </summary>
        private void PrepareForBattle()
        {
            _cameraManager.ChangeCamera(CameraType.PlayerTPS);
            _cameraManager.ChangeTarget(CameraAim.Player);
            _uiManager.HideEncounterText(); // 遭遇時のテキストを非表示にする
            _uiManager.BattleStart(); // バトルUIを表示する
            _bgmChanger.ChangeBGM(_nextPhase); // BGMの遷移開始
            // プレイヤーの着地アニメーション再生
            _enemyMove.MoveStart(); // 敵がNPCから離れてバトルの初期位置まで移動する
        }
        
        /// <summary>
        /// 遷移13拍目～16拍目
        /// </summary>
        private void SetupWeaponStance()
        {
            // 武器を構えるモーションを再生
        }
        
        /// <summary>
        /// 遷移17拍目：バトルフェーズに移行する
        /// </summary>
        private void ActivateBattlePhase()
        {
            _phaseManager.NextPhase();
            _musicEngineHelper.OnJustChangedBar -= Counter; // 購読を解除する
        }

        private void OnDestroy()
        {
            _musicEngineHelper.OnJustChangedBar -= Counter;
        }
    }
}
