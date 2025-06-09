using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceBehaviour_1 : PlayableBehaviour
    {
        private GameObject _owner;

        public void OnCreate(GameObject owner)
        {
            _owner = owner;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var text = _owner.GetComponentInChildren<Text>();
            if (text)
            {
                text.color = Color.clear;
            }
        }
    }
}
