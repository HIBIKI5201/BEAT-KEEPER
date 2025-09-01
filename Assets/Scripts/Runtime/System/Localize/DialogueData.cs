using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// ボイスと字幕のデータ
    /// </summary>
    public class DialogueData
    {
        private string _cueName;
        private string[] _subtitles;
        
        /// <summary>
        /// CRIでのボイス再生に使用するキューネーム
        /// </summary>
        public string CueName => _cueName;
        
        /// <summary>
        /// 字幕の配列
        /// </summary>
        public string[] Subtitles => _subtitles;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DialogueData(string cueName, string[] subtitles)
        {
            _cueName = cueName;
            _subtitles = subtitles;
        }
    }
}
