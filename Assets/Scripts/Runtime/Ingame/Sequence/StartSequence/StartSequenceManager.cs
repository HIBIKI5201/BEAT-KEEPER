using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartSequenceManager : MonoBehaviour
    {
        private PhaseEnum _nextPhase;
        private PhaseManager _phaseManager;

        private BGMManager _bgmManager;
        private int _count;

        private async void Start()
        {
            var multiSceneManager = ServiceLocator.GetInstance<MultiSceneManager>();
            if (multiSceneManager)
            {
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Stage);
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Battle);
            }

            var director = GetComponent<PlayableDirector>();
            director.Play();
            director.playableGraph.GetRootPlayable(0).SetSpeed((float)Music.CurrentTempo / 60);
        }
    }
}
