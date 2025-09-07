using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BeatKeeper
{
    public class MovieManager : MonoBehaviour
    {
        [SerializeField]
        private PlayableDirector[] _directors;

        private Dictionary<PlayableAsset, PlayableDirector> _timelineDict;

        private void Awake()
        {
            _timelineDict = new();

            foreach (var director in _directors)
            {
                if (!_timelineDict.TryAdd(director.playableAsset, director))
                {
                    Debug.LogWarning($"{director.playableAsset.name}が重複しています。"
                        + $"\n{_timelineDict[director.playableAsset].name} : {director.name}");
                }
            }
        }

        public PlayableDirector GetDirector(PlayableAsset timeline)
        {
            if (timeline == null)
            {
                Debug.LogError("timeline is null");
                return null;
            }

            if (_timelineDict.TryGetValue(timeline, out var director))
            {
                return director;
            }

            Debug.LogWarning($"No PlayableDirector found for TimelineAsset: {timeline.name}");
            return null;
        }
    }
}
