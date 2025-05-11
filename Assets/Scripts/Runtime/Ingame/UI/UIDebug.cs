using UnityEngine;

namespace BeatKeeper
{
    public class UIDebug : MonoBehaviour
    {
        [SerializeField] private Transform _enemy;
        [SerializeField] private DamageTextManager dmgTextManager;
        private int count = 0;
        
        [ContextMenu("Damage Text")]
        public void DamageText()
        {
            dmgTextManager.DisplayDamage(500, _enemy.position, false);
        }

        
    }
}
