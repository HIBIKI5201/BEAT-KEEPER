using System;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    public class InGameSceneManager : SceneManagerB
    {
        [SerializeField] GameObject _playerPrefab;
        
        private void Awake()
        {
            Instantiate(_playerPrefab);
        }

        private void Start()
        {
            ServiceLocator.GetInstance<MultiSceneManager>().SceneLoad(SceneListEnum.Stage);
        }
    }
}
