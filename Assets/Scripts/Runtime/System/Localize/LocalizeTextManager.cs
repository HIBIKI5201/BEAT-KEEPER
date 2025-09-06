using UnityEngine;
using SymphonyFrameWork.System;

namespace BeatKeeper
{
    /// <summary>
    /// 言語設定に合わせて字幕取得を補助するクラス
    /// </summary>
    public class LocalizeTextManager : MonoBehaviour
    {
        [SerializeField] private LocalizeDataSO _startMovieData; // スタートムービー用
        [SerializeField] private LocalizeDataSO _tutorialSubtitleData; // チュートリアル用
        [SerializeField] private LocalizeDataSO _tutorialOperationData; // チュートリアル操作方法用
        [SerializeField] private LocalizeDataSO _clearMovieData; // クリアムービー用
        [SerializeField] private LocalizeDataSO _resultData; // リザルト用
        
        /// <summary>
        /// 現在設定されている言語タイプ
        /// </summary>
        private LanguageType _currentLanguage = LanguageType.Japanese;
        
        /// <summary>
        /// 現在設定されている字幕用の言語タイプ
        /// </summary>
        private LanguageType _currentSubtitleLanguage = LanguageType.Japanese;

        /// <summary>
        /// 字幕を使うか
        /// </summary>
        public bool DontUseSubtitle => _currentSubtitleLanguage == LanguageType.None;

        /// <summary>
        /// Awake
        /// </summary>
        private void Awake()
        {
            // シングルトンに登録
            ServiceLocator.SetInstance(this, ServiceLocator.LocateType.Singleton);
        }

        /// <summary>
        /// スタートムービー用のデータからメッセージを取得する
        /// </summary>
        public string GetStartMovieMessage(string cueName)
        {
            if (DontUseSubtitle)
            {
                Debug.LogWarning($"字幕を使用しない設定です");
                return string.Empty;
            }
            
            return _startMovieData.GetMessage(cueName, _currentSubtitleLanguage);
        }
        
        /// <summary>
        /// チュートリアル字幕用のデータからメッセージを取得する
        /// </summary>
        public string GetTutorialSubtitleMessage(string cueName)
        {
            if (DontUseSubtitle)
            {
                Debug.LogWarning($"字幕を使用しない設定です");
                return string.Empty;
            }
            
            return _tutorialSubtitleData.GetMessage(cueName, _currentSubtitleLanguage);
        }

        /// <summary>
        /// チュートリアル操作方法用のデータからメッセージを取得する
        /// </summary>
        public string GetTutorialOperationMessage(string key)
        {
            return _tutorialOperationData.GetMessage(key, _currentLanguage);
        }

        /// <summary>
        /// クリアムービー用のデータからメッセージを取得する
        /// </summary>
        public string GetClearMovieMessage(string cueName)
        {
            if (DontUseSubtitle)
            {
                Debug.LogWarning($"字幕を使用しない設定です");
                return string.Empty;
            }

            return _clearMovieData.GetMessage(cueName, _currentSubtitleLanguage);
        }

        /// <summary>
        /// リザルト用のデータからメッセージを取得する
        /// </summary>
        public string GetResultMessage(string cueName)
        {
            if (DontUseSubtitle)
            {
                Debug.LogWarning($"字幕を使用しない設定です");
                return string.Empty;
            }

            return _resultData.GetMessage(cueName, _currentSubtitleLanguage);
        }

        /// <summary>
        /// 言語設定を変更する
        /// </summary>
        public void ChangeLanguage(LanguageType languageType)
        {
            _currentLanguage = languageType;
            Debug.Log($"言語設定変更: {languageType}");
        }
        
        /// <summary>
        /// 字幕設定を変更する
        /// </summary>
        public void ChangeSubtitleLanguage(LanguageType languageType)
        {
            _currentSubtitleLanguage = languageType;
            Debug.Log($"字幕言語設定変更: {languageType}");
        }
    }
}
