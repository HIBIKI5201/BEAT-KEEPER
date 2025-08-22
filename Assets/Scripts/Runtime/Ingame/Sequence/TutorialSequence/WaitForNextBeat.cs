using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    public class WaitForNextBeat : CustomYieldInstruction
    {
        private int _targetCount;
        private int _currentCount;
        private BGMManager _bgmManager;

        public WaitForNextBeat(int targetCount = 1)
        {
            _targetCount = targetCount;
            _currentCount = 0;

            _bgmManager = ServiceLocator.GetInstance<BGMManager>();
            _bgmManager.OnJustChangedBeat += OnJustChangedBeat;
        }

        public override bool keepWaiting
        {
            get { return _currentCount < _targetCount; }
        }

        private void OnJustChangedBeat()
        {
            _currentCount++;
            if (_currentCount >= _targetCount)
            {
                _bgmManager.OnJustChangedBeat -= OnJustChangedBeat;
            }

        }
    }
}
