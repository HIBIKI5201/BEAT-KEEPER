namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     敵キャラクターのインターフェース
    /// </summary>
    public interface IEnemy : IHitable
    {
        public EnemyData EnemyData { get; }
        public bool IsFinisherable { get; }
    }
}
