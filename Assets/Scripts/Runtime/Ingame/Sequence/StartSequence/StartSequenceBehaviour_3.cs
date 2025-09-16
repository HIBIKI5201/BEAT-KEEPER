using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class StartSequenceBehaviour_3 : PlayableBehaviour
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            var countDownPerformer = ServiceLocator.GetInstance<UIElement_CountDown>();
            if (countDownPerformer)
            {
                countDownPerformer.StartGame();
            }
			else
            {
                Debug.LogError($"{typeof(UIElement_CountDown)}がServiceロケーターから取得できませんでした");
            }

            Debug.Log("StartPerformanceBehaviour_3");
        }
    }
}
