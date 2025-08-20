using UnityEngine;
using CriWare;
using UnityEngine;
using UnityEngine.Playables;
using BeatKeeper.Runtime.System;
namespace BeatKeeper
{

    public class CriAtomSequenceBehaviour : PlayableBehaviour
    {
        public string CueName;
        public GameObject Owner;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (string.IsNullOrEmpty(CueName)) return;
            VoiceManager.PlayVoice(CueName);
        }
    }

}
