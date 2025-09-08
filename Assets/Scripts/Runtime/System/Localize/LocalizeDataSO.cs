using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BeatKeeper
{
    /// <summary>
    /// ローカライズ用のスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizeDataSO", menuName = "BeatKeeper/Localize/LocalizeDataSO")]
    public class LocalizeDataSO : ScriptableObject
    {
        [SerializeField] private List<DialogueData> _dialogues;

        /// <summary>
        /// キューネームを元に渡した言語タイプのMessageを取得する
        /// </summary>
        public string GetMessage(string cueName, LanguageType languageType)
        {
            var dialogueData = _dialogues.FirstOrDefault(data => data.CueName == cueName);

            if (dialogueData == null)
            {
                Debug.LogWarning($"指定したキューネームからテキストメッセージが取得できませんでした: {cueName}");
                return string.Empty;
            }
            
            // 言語をチェックして適切なものを返す
            return languageType == LanguageType.Japanese 
                ? dialogueData.JapaneseMessage 
                : dialogueData.EnglishMessage;
        }
    }
}
