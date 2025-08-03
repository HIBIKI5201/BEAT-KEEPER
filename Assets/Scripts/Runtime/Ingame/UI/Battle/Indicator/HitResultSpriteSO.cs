using UnityEngine;
using System;

namespace BeatKeeper
{
    /// <summary>
    /// 中央の画像を設定するスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "HitResultSpriteSO", menuName = "BeatKeeper/UI/HitResultSpriteSO")]
    public class HitResultSpriteSO : ScriptableObject
    {
        [SerializeField] private HitResultData _perfect;
        [SerializeField] private HitResultData _good;
        [SerializeField] private HitResultData _miss;
        [SerializeField] private HitResultData _operation;
        
        /// <summary>
        /// Perfectの画像データ
        /// </summary>
        public HitResultData Perfect => _perfect;
        
        /// <summary>
        /// Goodの画像データ
        /// </summary>
        public HitResultData Good => _good;
        
        /// <summary>
        /// Missの画像データ
        /// </summary>
        public HitResultData Miss => _miss;

		/// <summary>
        /// 操作方法
        /// </summary>
		public HitResultData Operation => _operation;
    }

    [Serializable]
    public class HitResultData
    {
        public Sprite Sprite;
        public Vector2 SizeDelta = new Vector2(250, 60);
    }
}
