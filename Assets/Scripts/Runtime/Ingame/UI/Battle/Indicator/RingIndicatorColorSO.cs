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
    }
}
