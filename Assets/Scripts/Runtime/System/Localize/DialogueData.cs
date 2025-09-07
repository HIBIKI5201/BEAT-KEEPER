using UnityEngine;
using System;

namespace BeatKeeper
{
    /// <summary>
    /// ボイスと字幕のデータ
    /// </summary>
	[Serializable]
    public class DialogueData
    {
        [SerializeField] private string _cueName;
		[SerializeField] private string _japaneseMessage;
		[SerializeField] private string _englishMessage;
        
        /// <summary>
        /// CRIでのボイス再生に使用するキューネーム
        /// </summary>
        public string CueName => _cueName;
        
		/// <summary>
        /// 日本語テキスト
        /// </summary>
		public string JapaneseMessage => _japaneseMessage;

		/// <summary>
        /// 英語テキスト
        /// </summary>
		public string EnglishMessage => _englishMessage;
    }
}
