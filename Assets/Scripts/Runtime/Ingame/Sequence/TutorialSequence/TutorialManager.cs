using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using SymphonyFrameWork.System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private UIElement_ChartRingManager _chartRingManager;
        [SerializeField] private int _indicatorGenerateCount;
        [SerializeField] private ChartKindEnum _chartKindEnum;

        private List<RingIndicatorBase> _activeRingIndicator = new();
        private BGMManager _bgmManager;
        private int _currentIndicatorCount = 0;

        private async void Start()
        {
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
        }

        private void OnDestroy()
        {
            TutorialUnRegister();
        }

        public void StartTutorial()
        {
            _director.Play();
            TutorialRegister();
        }

        public void PlayVoice(int voiceNum)
        {

        }

        public void TutorialRegister()
        {
            _bgmManager.OnJustChangedBeat += TutorialIndicatorGenerate;
        }

        public void TutorialUnRegister()
        {
            _bgmManager.OnJustChangedBeat -= TutorialIndicatorGenerate;
        }

        public void TutorialIndicatorGenerate()
        {
            foreach (var ind in _activeRingIndicator)
            {
                ind.AddCount();
            }
            if (_indicatorGenerateCount <= _currentIndicatorCount)
            {
               　var ringObj = _chartRingManager.GenerateRing(_chartKindEnum, Vector2.zero, 0);
                var ringIndicator = ringObj.GetComponent<RingIndicatorBase>();
                if (ringIndicator)
                {
                    _activeRingIndicator.Add(ringIndicator);
                }
                _currentIndicatorCount = 0;
            }
            Debug.Log(_currentIndicatorCount);
            _currentIndicatorCount++;
        }
    }
}
