using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// Perfect/Good/Missの画像を設定するスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "HitResultSpriteSO", menuName = "BeatKeeper/UI/HitResultSpriteSO")]
    public class HitResultSpriteSO : ScriptableObject
    {
        [SerializeField] private Sprite _perfect;
        [SerializeField] private Sprite _good;
        [SerializeField] private Sprite _miss;
        
        /// <summary>
        /// Perfectの画像
        /// </summary>
        public Sprite Perfect => _perfect;
        
        /// <summary>
        /// Goodの画像
        /// </summary>
        public Sprite Good => _good;
        
        /// <summary>
        /// Missの画像
        /// </summary>
        public Sprite Miss => _miss;
    }
}
