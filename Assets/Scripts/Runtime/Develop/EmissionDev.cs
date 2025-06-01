using SymphonyFrameWork.System;
using TMPro.EditorUtilities;
using UnityEngine;

namespace BeatKeeper.Runtime.Develop
{
    public class EmmitionDev : MonoBehaviour
    {
        private float _durationOfBeat;

        private Material _material;
        private Color _color;

        private void Start()
        {
            var musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();

            musicEngine.OnJustChangedBeat += OnBeat;
            _durationOfBeat = (float)musicEngine.DurationOfBeat;

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

                await Awaitable.WaitForSecondsAsync(_durationOfBeat / (count + 20));
            }

            _material.SetColor("_Color", _color);
        }
    }
}
