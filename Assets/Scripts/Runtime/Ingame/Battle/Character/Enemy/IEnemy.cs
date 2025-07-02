namespace BeatKeeper.Runtime.Ingame.Character
{
    public interface IEnemy : IHitable
    {
        public EnemyData EnemyData { get; }
    }
}
