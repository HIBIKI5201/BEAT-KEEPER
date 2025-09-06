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
            return _startMovieData.GetMessage(cueName, _currentLanguage);
        }
        
        /// <summary>
        /// チュートリアル字幕用のデータからメッセージを取得する
        /// </summary>
        public string GetTutorialSubtitleMessage(string cueName)
        {
            return _tutorialSubtitleData.GetMessage(cueName, _currentLanguage);
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
            return _clearMovieData.GetMessage(cueName, _currentLanguage);
        }

        /// <summary>
        /// リザルト用のデータからメッセージを取得する
        /// </summary>
        public string GetResultMessage(string cueName)
        {
            return _resultData.GetMessage(cueName, _currentLanguage);
        }

        /// <summary>
        /// 言語設定を変更する
        /// </summary>
        public void ChangeLanguage(LanguageType languageType)
        {
            _currentLanguage = languageType;
            Debug.Log($"言語設定変更: {languageType}");
        }
    }
}
