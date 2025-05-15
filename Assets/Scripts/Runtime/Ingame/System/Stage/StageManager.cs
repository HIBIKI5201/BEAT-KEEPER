using System;
using BeatKeeper.Runtime.Ingame.Character;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using R3;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Stsge
{
    public class StageManager : MonoBehaviour
    {
        public CameraManager CameraManager => _cameraManager;
        [SerializeField] private CameraManager _cameraManager;
    }
}
