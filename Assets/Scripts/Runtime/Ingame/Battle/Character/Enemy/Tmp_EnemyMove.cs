using DG.Tweening;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// 開始演出時の敵の動きの仮実装
    /// </summary>
    public class Tmp_EnemyMove : MonoBehaviour
    {
        [SerializeField] private Transform _enemy;
        [SerializeField] private Vector3 _targetPos; // 目標地点
        [SerializeField] private float _duration; // 目標地点に到達するまでの時間
        
        /// <summary>
        /// 移動を開始する
        /// プレイヤーが武器を構えるモーションの間に、NPCから離れるような動きをとる
        /// </summary>
        public void MoveStart()
        {
            _enemy.DOMove(_targetPos, _duration);
        }
    }
}
