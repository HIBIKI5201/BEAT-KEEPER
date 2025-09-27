using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Stsge;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.UI.GridLayoutGroup;
using Debug = UnityEngine.Debug;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartSequenceManager : BaseSkipController
    {
        [SerializeField] TutorialManager _tutorialManager;
        [SerializeField] PlayableAsset _playableAsset;
        
        [Header("スキップ処理")]
        [SerializeField] private float _skipTiming;
        
        [SerializeField] private string _bgmName = "Phase1";
        
        private PlayableDirector _director;
        
        protected override async void Start()
        {
            var multiSceneManager = ServiceLocator.GetInstance<MultiSceneManager>();
            if (multiSceneManager)
            {
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Stage);
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Battle);
            }

            var movieManager = await ServiceLocator.GetInstanceAsync<MovieManager>();
            var director = movieManager.GetDirector(_playableAsset);
            var phaseManager = await ServiceLocator.GetInstanceAsync<PhaseManager>();
            
            //スタートシーケンスの再生終了時にチュートリアルシーケンスを再生する
            director.stopped += (_) =>
            {
                ServiceLocator.GetInstance<BattleSceneManager>()
                    .EnemyAdmin.SetActiveEnemy(0);
                _tutorialManager.StartTutorial();
                phaseManager.TransitionTo(PhaseEnum.Tutorial);
            };
            director.Play();
            //現在最初にBGMが再生されていないのでコメントアウト
            //director.playableGraph.GetRootPlayable(0).SetSpeed((float)Music.CurrentTempo / 60);

            SaveDirector(director);

            base.Start();
        }

        protected override void Update()
        {
            // タイムラインアセットがnullもしくはタイムラインの再生が終了していたらスキップ処理は行いたくないので早期return
            if (_director == null || _director.time >= _skipTiming)
            {
                return;
            }

            base.Update();
        }
        
        [ContextMenu(nameof(OnSkip))]
        protected override void OnSkip()
        {
            if (_director == null)
            {
                Debug.LogError("PlayableDirector is not set. Please run the scene in the editor to set it.");
                return;
            }
            
            if (!Music.IsPlaying)
            {
                // BGMが再生されていなかったらBGMを再生
                var bgmManager = ServiceLocator.GetInstance<BGMManager>();
                if (bgmManager)
                {
                    bgmManager.ChangeBGM(_bgmName);
                }
            }

            var cameraManager = ServiceLocator.GetInstance<CameraManager>();
            if (cameraManager)
            {
                var stageManager = ServiceLocator.GetInstance<StageSceneManager>();
                cameraManager.ChangeCamera(stageManager.PlayerCamera);
            }

            var text = GetComponentInChildren<UIElement_EncounterText>();
            if (text)
            {
                text.HideEncounterText();
            }

            // タイムラインの時間をとばす
            _director.time = _skipTiming;
        }
        
        [Conditional("UNITY_EDITOR")]
        private void SaveDirector(PlayableDirector director)
        {
            _director = director;
        }
    }
}
