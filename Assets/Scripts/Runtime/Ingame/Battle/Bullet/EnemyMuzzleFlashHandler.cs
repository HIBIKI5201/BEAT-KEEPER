using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// 敵のマズルフラッシュエフェクトの再生タイミングを管理するクラス
    /// </summary>
    public class EnemyMuzzleFlashHandler : MonoBehaviour
    {
        [SerializeField] private MuzzleFlashController _muzzleFlash = new MuzzleFlashController();
        private EnemyManager _enemyManager;
        
        private void Start()
        {
            if (!TryGetComponent(out _enemyManager))
            {
                Debug.LogError($"{typeof(EnemyManager)}が取得できませんでした");
                return;
            }
            
            // 敵が攻撃したタイミングでマズルフラッシュのエフェクトが再生されるようにする
            // TODO: 仕様次第で銃口の数が変わったらその処理を足す必要がある
            _enemyManager.OnNormalAttack += _muzzleFlash.Fire;
        }

        private void OnDestroy()
        {
            if(_enemyManager) _enemyManager.OnNormalAttack -= _muzzleFlash.Fire;
        }
    }
}
