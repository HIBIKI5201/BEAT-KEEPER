using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class AisacControlBehaviour : PlayableBehaviour
    {
        public void SetData(string aisac, float duration, float start, float end)
        {
            _aisac = aisac;
            _duration = duration;
            _start = start;
            _end = end;
        }

        public override async void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var bgmManager = ServiceLocator.GetInstance<BGMManager>();

            float timer = 0;
            float to = _end - _start;
            while (timer < _duration)
            {
                float proportion = timer / _duration;
                float volume = _start + to * proportion;
                bgmManager.ChangeAisacValue(_aisac, volume);

                timer += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }
        }

        private string _aisac;
        private float _duration = 0;
        private float _start;
        private float _end;
    }
}
