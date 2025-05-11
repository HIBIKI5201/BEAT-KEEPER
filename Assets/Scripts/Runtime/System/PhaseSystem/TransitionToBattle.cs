using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// アプローチフェーズからバトルフェーズへの遷移処理
    /// </summary>
    public class TransitionToBattle : MonoBehaviour
    {
        [SerializeField] private BGMChanger _bgmChanger;
        [SerializeField] private CameraManager _cameraManager;
        
        // 仮
        [SerializeField] private PhaseEnum _startPhase = PhaseEnum.Battle1;
        [SerializeField] private Transform _encountText;
        private float _defaultTextPosY;
        
        private PhaseEnum _nextPhase;
        private PhaseManager _phaseManager;
        private MusicEngineHelper _musicEngineHelper;
        private int _count;

        private void Start()
        {
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
            
            _defaultTextPosY = _encountText.position.y;
            _cameraManager.CameraChange(CameraAim.Player);
            TransitionStart(_startPhase); //TODO: テスト用。アプローチフェーズの処理と連携して呼び出すようにしたい
        }

        /// <summary>
        /// 遷移処理を開始する
        /// </summary>
        public void TransitionStart(PhaseEnum nextPhase)
        {
            _musicEngineHelper.OnJustChangedBar += Counter;
            _nextPhase = nextPhase;
            
            //TODO: 後で消す。テキスト
            _encountText.position = new Vector3(_encountText.position.x, _defaultTextPosY, _encountText.position.z);
            _encountText.gameObject.SetActive(true);
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
            _cameraManager.CameraChange(CameraAim.FirstBattleEnemy);
            _encountText.DOLocalMoveY(400f, 0.5f); // 遭遇UIを上からスライド
        }
        
        /// <summary>
        /// 遷移9拍目～12拍目
        /// </summary>
        private void PrepareForBattle()
        {
            _cameraManager.CameraChange(CameraAim.Player);
            _encountText.gameObject.SetActive(false); // 遭遇UIを非表示に
            _bgmChanger.ChangeBGM(_nextPhase); // BGMの遷移開始
            // プレイヤーの着地アニメーション再生
            // 敵がNPCから離れてバトルの初期位置まで移動する
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
            _phaseManager.TransitionTo(PhaseEnum.Battle1);
            _musicEngineHelper.OnJustChangedBar -= Counter; // 購読を解除する
        }

        private void OnDestroy()
        {
            _musicEngineHelper.OnJustChangedBar -= Counter;
        }
    }
}
