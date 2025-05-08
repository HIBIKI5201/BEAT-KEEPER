using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public interface IEnemy : IAttackable
    {
        public EnemyData EnemyData { get; }
    }
}
