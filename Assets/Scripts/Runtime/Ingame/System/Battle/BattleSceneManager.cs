using BeatKeeper.Runtime.Ingame.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Battle
{
    /// <summary>
    ///    バトルシーンの管理クラス
    /// </summary>
    public class BattleSceneManager : SceneManagerB
    {
        public StageEnemyAdmin EnemyAdmin => _enemyAdmin;

        [SerializeField]
        private StageEnemyAdmin _enemyAdmin;
    }
}