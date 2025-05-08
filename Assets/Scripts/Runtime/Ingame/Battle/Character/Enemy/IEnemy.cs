using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public interface IEnemy
    {
        public EnemyData EnemyData { get; }
        
        public void HitAttack(float damage);
    }
}
