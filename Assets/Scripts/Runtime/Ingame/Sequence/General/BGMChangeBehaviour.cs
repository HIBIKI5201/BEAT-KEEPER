using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class BGMChangeBehaviour : PlayableBehaviour
    {
        private string _bgmName = string.Empty;

        public void SetBGMName(string name) => _bgmName = name;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var bgmManager = ServiceLocator.GetInstance<BGMManager>();
            if (bgmManager)
            {
                bgmManager.ChangeBGM(_bgmName);
            }
        }
    }
}
