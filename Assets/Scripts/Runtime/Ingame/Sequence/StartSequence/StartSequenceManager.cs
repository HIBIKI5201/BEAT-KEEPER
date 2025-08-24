using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartSequenceManager : MonoBehaviour
    {
        [SerializeField] TutorialManager _tutorialManager;
        [SerializeField] PlayableAsset _playableAsset;
        private async void Start()
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
        }

        private PlayableDirector _director;

        [Conditional("UNITY_EDITOR")]
        private void SaveDirector(PlayableDirector director)
        {
            _director = director;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField]
        private float _skipTiming;

        [ContextMenu(nameof(SkipStart))]
        private void SkipStart()
        {
            if (_director == null)
            {
                Debug.LogError("PlayableDirector is not set. Please run the scene in the editor to set it.");
                return;
            }
            _director.time = _skipTiming;
        }
#endif
    }
}
