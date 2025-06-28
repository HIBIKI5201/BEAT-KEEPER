using BeatKeeper.Runtime.Ingame.UI;
using DG.Tweening;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// クリアフェーズ全体の流れを管理するマネージャークラス
    /// </summary>
    public class ClearPhaseManager : MonoBehaviour
    {
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private BattleResultController _battleResultController;
        [SerializeField] private UIElement_EncounterText _encounterText;
        [SerializeField] private InGameUIManager _uiManager;
        [SerializeField] private GameObject[] _objects;
        [SerializeField] private Vector3[] _positions;
        private PhaseManager _phaseManager;
        private BGMManager _musicEngineHelper;
        private int _count;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private void Start()
        {
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<BGMManager>();
            _battleResultController.Hide(); // 最初は表示しないようにする

            //クリアフェーズを廃止する可能性があるため、以下のコードはコメントアウトしておく
            //_phaseManager.CurrentPhaseProp.Subscribe(value => {if(value == PhaseEnum.Clear) ClearPhaseStart(); }).AddTo(_disposables);
        }

        /// <summary>
        /// クリアフェーズが始まったら呼ばれる処理
        /// </summary>
        public void ClearPhaseStart()
        {
            _musicEngineHelper.OnJustChangedBar += Counter;
            _objects[0].gameObject.SetActive(false); // 現在のバトルのEnemyを非表示に
        }

        /// <summary>
        /// 拍数を元に遷移処理を行う
        /// </summary>
        private void Counter()
        {
            _count++;
            if (_count == 1)
            {
                // リザルト表示、NPCにフォーカス。NPCが褒めてくれる演出
                ShowBattleResult();
                _uiManager.BattleEnd();
                //_cameraManager.ChangeCamera(1);
            }
            else if (_count == 5)
            {
                // リザルト表示を隠す
                HideBattleResult();
            }
            else if (_count == 9)
            {
                _objects[1].gameObject.SetActive(true); // 次の敵が出現
                _objects[2].gameObject.SetActive(true);
                _encounterText.ShowEncounterText(2);
                _objects[2].transform.DOMove(_positions[0], 4f); // NPCを追いかけている状態
                //_cameraManager.ChangeCamera(); // カメラを向ける
            }
            else if (_count == 13)
            {
                // プレイヤーにカメラを戻して、武器を構えるモーション
                _uiManager.BattleStart();
                _encounterText.HideEncounterText();
                _objects[1].transform.DOMove(_positions[1], 4f); // 敵が戦闘位置まで移動
                _objects[3].transform.LookAt(_objects[1].transform); // プレイヤーを次の敵の方に向かせる
                //_cameraManager.ChangeCamera(0);
            }
            else if (_count == 17)
            {
                // バトルフェーズを始める
                ActivateBattlePhase();
            }
        }

        /// <summary>
        /// バトルリザルトを表示する
        /// </summary>
        private void ShowBattleResult()
        {
            _battleResultController.Show();
        }

        /// <summary>
        /// バトルリザルトを非表示にする
        /// </summary>
        private void HideBattleResult()
        {
            _battleResultController.Hide();
        }

        /// <summary>
        /// バトルフェーズに移行する
        /// </summary>
        private void ActivateBattlePhase()
        {
            _phaseManager.NextPhase(); // TODO: PhaseManager側に、次のシーンを再生する仕組みを追加する
            _musicEngineHelper.OnJustChangedBar -= Counter; // 購読を解除する
        }

        private void OnDestroy()
        {
            _musicEngineHelper.OnJustChangedBar -= Counter;
            _disposables?.Dispose();
        }
    }
}
