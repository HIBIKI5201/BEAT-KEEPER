using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public class TestEnemyManager : CharacterManagerB<EnemyData>
    {
        protected override void Awake()
        {
            base.Awake();
            
            Debug.Log(_data.Beat);
        }
    }
}
