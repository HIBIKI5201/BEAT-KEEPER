using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    public class SoundEffectDev : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _clip;

        void Start()
        {
            SoundEffectPlay();
        }

        /// <summary>
        ///     音を再生する
        /// </summary>
        [ContextMenu("Sound Effect Play")]
        private void SoundEffectPlay()
        {
            if (!_clip) return;

            var source = AudioManager.GetAudioSource("SE");
            if(source)
            {
                source.PlayOneShot( _clip );
            }
        }
    }
}
