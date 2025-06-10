using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    [RequireComponent(typeof(PlayableDirector))]
    public class BreakMovieSequenceManager : MonoBehaviour
    {
        private PlayableDirector _playableDirector;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            if (!_playableDirector)
            {
                Debug.LogWarning("PlayableDirector component is missing on FinisherSequence.");
            }
        }

        private void Start()
        {
            var finisher = transform.parent.GetComponentInChildren<FinisherSequenceManager>();
            if (finisher)
            {
                finisher.OnFinisherSequenceEnd += OnFinisherSequenceEnd;
            }
        }

        private void OnFinisherSequenceEnd()
        {
            if (!_playableDirector) return;

            _playableDirector.Play();
        }
    }
}
