using BeatKeeper.Runtime.Ingame.Battle;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequence : MonoBehaviour
    {
        private void Start()
        {
            FinisherEventRegister(0); //Å‰‚Ì“G‚ğ“o˜^
        }

        public async void FinisherEventRegister(int index)
        {
            var battleScene = await ServiceLocator.GetInstanceAsync<BattleSceneManager>();

            var enemy = battleScene.EnemyAdmin.Enemies[index];
            enemy.OnFinisherable += OnFinisherable;
        }

        private void OnFinisherable()
        {

        }
    }
}
