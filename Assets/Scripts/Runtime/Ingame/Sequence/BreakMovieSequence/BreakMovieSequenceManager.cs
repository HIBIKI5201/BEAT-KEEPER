using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class BreakMovieSequenceManager : MonoBehaviour
    {
        private PlayableDirector _playableDirector;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            if (_playableDirector == null)
            {
                Debug.LogWarning("PlayableDirector component is missing on FinisherSequence.");
            }
        }
    }
}
