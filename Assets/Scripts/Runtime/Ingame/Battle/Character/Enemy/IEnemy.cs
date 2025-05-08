using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public interface IEnemy : IAttackable
    {
        public EnemyData EnemyData { get; }
    }
}
