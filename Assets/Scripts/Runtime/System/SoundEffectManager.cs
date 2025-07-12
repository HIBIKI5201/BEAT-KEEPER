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
