using UnityEngine;
using UnityEngine.Playables;
using BeatKeeper.Runtime.Ingame.Sequence;
namespace BeatKeeper
{
    public class CriAtomSequenceBehaviour : PlayableBehaviour
    {
        public string CueName;
        public string Text;
        public GameObject Owner;
        private bool _isPlayed;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (string.IsNullOrEmpty(CueName) || _isPlayed) return;
            _isPlayed = true;
            if (Owner)
            {
                var tutorialManager = Owner.GetComponent<TutorialManager>();
                if (tutorialManager)
                {
                    tutorialManager.PlayVoice(CueName,Text);
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            double time = playable.GetTime();
            double duration = playable.GetDuration();

            // クリップを最後まで再生し終わったときのみリセット
            if (time >= duration)
            {
                _isPlayed = false;
            }
        }

    }

}
