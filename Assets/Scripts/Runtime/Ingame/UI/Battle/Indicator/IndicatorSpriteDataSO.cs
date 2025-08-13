using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// Perfect/Good判定用に白色のリング素材を設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "IndicatorSpriteDataSO", menuName = "BeatKeeper/UI/IndicatorSpriteDataSO")]
    public class IndicatorSpriteDataSO : ScriptableObject
    {
        [SerializeField, Tooltip("中央の太めのリング")] private Sprite _hitLine;
        [SerializeField, Tooltip("デコレーション")] private Sprite _decoration;
        [SerializeField, Tooltip("収縮する細めのリング")] private Sprite _ring;
        
        /// <summary>
        /// 中央の太めのリング
        /// </summary>
        public Sprite HitLine => _hitLine;
        
        /// <summary>
        /// デコレーション
        /// </summary>
        public Sprite Decoration => _decoration;
        
        /// <summary>
        /// 収縮する細めのリング
        /// </summary>
        public Sprite Ring => _ring;
    }
}
