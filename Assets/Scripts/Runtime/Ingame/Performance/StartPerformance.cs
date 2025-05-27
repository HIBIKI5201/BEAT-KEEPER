using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartPerformance : MonoBehaviour
    {
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
            
            var director = GetComponent<PlayableDirector>();
            director.Play();
        }
    }
}
