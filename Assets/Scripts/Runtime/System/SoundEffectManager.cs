using CriWare;
using UnityEngine;

namespace BeatKeeper.Runtime.System
{
    /// <summary>
    ///     CRIのサウンドエフェクトを管理するクラス。
    /// </summary>
    [RequireComponent(typeof(CriAtomSource))]
    public class SoundEffectManager : MonoBehaviour
    {
        /// <summary>
        ///     指定されたキュー名のサウンドエフェクトを再生します。
        /// </summary>
        /// <param name="cueName"></param>
        public static void PlaySoundEffect(string cueName)
        {
            if (_instance == null)
            {
                Debug.LogError("SoundEffectManager is not initialized.");
                return;
            }

            _instance._criAtomSource.cueName = cueName;
            _instance._criAtomSource.Play();
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
        private static SoundEffectManager _instance;

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
                Debug.LogError("CriAtomSource component is required on SoundEffectManager.");
                return;
            }
        }
    }
}
