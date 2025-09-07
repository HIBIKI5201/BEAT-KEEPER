using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;

namespace BeatKeeper
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CameraBinder : MonoBehaviour
    {
        private const string TRACK_NAME = "Cinemachine Track";

        private PlayableDirector _director;

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            Camera mainCam = Camera.main;

            if (mainCam != null)
            {
                CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();

                if (brain == null) return;

                // Timelineの全Trackをチェック
                foreach (var output in _director.playableAsset.outputs)
                {
                    // CinemachineTrackを探す
                    if (output.streamName.Contains(TRACK_NAME))
                    {
                        // 動的にCinemachineBrainをバインド
                        _director.SetGenericBinding(output.sourceObject, brain);
                    }
                }
            }


        }
    }
}
