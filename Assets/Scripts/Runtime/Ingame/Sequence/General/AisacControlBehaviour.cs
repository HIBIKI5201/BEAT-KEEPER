using BeatKeeper.Runtime.Ingame.System;
using CriWare;
using SymphonyFrameWork.System;
using SymphonyFrameWork.Utility;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class AisacControlBehaviour : PlayableBehaviour
    {
        public void SetData(string aisac, float duration,float start, float end)
        {
            _aisac = aisac;
            _duration = duration;
            _start = start;
            _end = end;
        }

        public override async void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var bgmManager = ServiceLocator.GetInstance<BGMManager>();
            await SymphonyTask.WaitUntil(() => bgmManager.AtomSource != null);

            var player = bgmManager.AtomSource.player;

            float timer = 0;
            while (timer < _duration)
            {
                float proportion = timer / _duration;
                player.SetAisacControl(_aisac, _start + _end * proportion);

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
