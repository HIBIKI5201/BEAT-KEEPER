using CriWare;
using UnityEngine;

namespace BeatKeeper.Runtime.System
{
    [RequireComponent(typeof(CriAtomSource))]
    public class VoiceManager : MonoBehaviour
    {
        public static void PlayVoice(string cueName)
        {
            if (_instance == null)
            {
                Debug.LogError("VoiceManager is not initialized.");
                return;
            }
            _instance._criAtomSource.cueName = cueName;
            _instance._criAtomSource.Play();
        }

        private static VoiceManager _instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _instance = null;
        }

        private CriAtomSource _criAtomSource;
        private void Awake()
        {
            _instance = this;
            if (!TryGetComponent(out _criAtomSource))
            {
                Debug.LogError("CriAtomSource component is required on VoiceManager.");
                return;
            }
        }
    }
}
