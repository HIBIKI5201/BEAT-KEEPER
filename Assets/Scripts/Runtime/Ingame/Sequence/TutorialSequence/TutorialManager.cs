using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private UIElement_ChartRingManager _chartRingManager;
        [SerializeField] private int _indicatorGenerateCount;

        private BGMManager _bgmManager;
        private ChartKindEnum _chartKindEnum;
        private int _currentIndicatorCount = 0;

        private async void Start()
        {
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
        }

        public void StartTutorial()
        {
            _director.Play();
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
            if (_indicatorGenerateCount <= _currentIndicatorCount)
            {
                _chartRingManager.GenerateRing(_chartKindEnum, Vector2.zero, 0);
                _currentIndicatorCount = 0;
            }
            _currentIndicatorCount++;
        }
    }
}
