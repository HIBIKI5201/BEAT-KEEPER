using UnityEngine;
using SymphonyFrameWork.System;
using R3;
using SymphonyFrameWork.Utility;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    public class Crystal : MonoBehaviour,ICrystal
    {
        [SerializeField] int _crystalEventSplineIndex = 0;
        [SerializeField] int _crystalEventEndSplineIndex = 0;
        [SerializeField] int _score;
        public int CrystalEventSplineIndex { get => _crystalEventSplineIndex;}
        public int CrystalEventEndSplineIndex {get => _crystalEventEndSplineIndex;}
        public void Get() 
        {
        }
    }
}
