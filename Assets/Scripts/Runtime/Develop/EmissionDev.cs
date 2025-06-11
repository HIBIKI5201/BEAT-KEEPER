using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Develop
{
    public class EmmitionDev : MonoBehaviour
    {
        private Material _material;
        private Color _color;

        private void Start()
        {
            var musicEngine = ServiceLocator.GetInstance<BGMManager>();

            musicEngine.OnJustChangedBeat += OnBeat;

            var renderer = GetComponent<Renderer>();
            _material = renderer.material;

            _material.EnableKeyword("_EMISSION");
            _color = _material.GetColor("_Color");
        }

        private async void OnBeat()
        {
            int count = 10;

            for(int i = 0; i < count; i++)
            {
                var color = _color * i / 5;
                _material.SetColor("_Color", color);

                await Awaitable.WaitForSecondsAsync((float)MusicEngineHelper.DurationOfBeat / (count + 20));
            }

            _material.SetColor("_Color", _color);
        }
    }
}
