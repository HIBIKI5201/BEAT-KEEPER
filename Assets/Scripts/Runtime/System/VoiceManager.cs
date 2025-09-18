using CriWare;
using UnityEngine;

namespace BeatKeeper.Runtime.System
{
    [RequireComponent(typeof(CriAtomSource))]
    public class VoiceManager : MonoBehaviour
    {
        public static CriAtomExPlayback PlayVoice(string cueName)
        {
            if (_instance == null)
            {
                Debug.LogError("VoiceManager is not initialized.");
                return default;
            }
            _instance._criAtomSource.cueName = cueName;
            return _instance._criAtomSource.Play();
        }

        public static void ChangePhaseSelector(string phaseName)
        {
            if (_instance == null)
            {
                Debug.LogError("VoiceManager is not initialized.");
                return;
            }
            CriAtomExPlayer player = _instance._criAtomSource.player;
            player.SetSelectorLabel(PHASE_SELECTOR_NAME, phaseName);
            player.UpdateAll();
        }

        private const string PHASE_SELECTOR_NAME = "Selector_Phase";

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
