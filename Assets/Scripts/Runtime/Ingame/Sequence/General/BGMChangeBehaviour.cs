using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.System.Sequence
{
    public class BGMChangeBehaviour : PlayableBehaviour
    {
        public void SetBGMName(string name) => _bgmName = name;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var bgmManager = ServiceLocator.GetInstance<BGMManager>();
            if (bgmManager)
            {
                bgmManager.ChangeBGM(_bgmName);
            }
        }

        private string _bgmName = string.Empty;
    }
}
