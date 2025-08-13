using UnityEngine;
using System;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// ノーツのPerfect/Goodの色を統一して設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "RingIndicatorColorSO", menuName = "BeatKeeper/UI/RingIndicatorColorSO")]
    public class RingIndicatorColorSO : ScriptableObject
    {
        [Header("Perfect")]
        [SerializeField] private Color _perfectColor;
        [SerializeField] private Color _translucentPerfectColor;
        
        [Header("Good")]
        [SerializeField] private Color _goodColor;
        [SerializeField] private Color _translucentGoodColor;
        
        [Header("Default")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _translucentDefaultColor = Color.white;
        
        /// <summary>
        /// Perfect判定の色
        /// </summary>
        public Color PerfectColor => _perfectColor;
        
        /// <summary>
        /// Perfect判定の半透明の色
        /// </summary>
        public Color TranslucentPerfectColor => _translucentPerfectColor;
        
        /// <summary>
        /// Good判定の色
        /// </summary>
        public Color GoodColor => _goodColor;
        
        /// <summary>
        /// Good判定の半透明の色
        /// </summary>
        public Color TranslucentGoodColor => _translucentGoodColor;
        
        /// <summary>
        /// デフォルト色
        /// </summary>
        public Color DefaultColor => _defaultColor;
        
        /// <summary>
        /// デフォルトの透明色
        /// </summary>
        public Color TranslucentDefaultColor => _translucentDefaultColor;
    }
}
