using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace BeatKeeper.Editor
{
    [CustomEditor(typeof(LocalizeDataSO))]
    public class LocalizeDataSOEditor : UnityEditor.Editor
    {
        private SerializedProperty dialoguesProperty;
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private bool showAddSection = false;
        private string newCueName = "";
        private string newJapaneseMessage = "";
        private string newEnglishMessage = "";
        
        // フィルター用
        private bool showEmptyOnly = false;
        private bool showValidOnly = false;

        private void OnEnable()
        {
            dialoguesProperty = serializedObject.FindProperty("_dialogues");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            LocalizeDataSO localizeData = (LocalizeDataSO)target;
            
            // ヘッダー情報
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"総データ数: {dialoguesProperty.arraySize}", EditorStyles.boldLabel);
            
            // 検索とフィルター
            DrawSearchAndFilters();
            
            EditorGUILayout.Space();
            
            // 追加セクション
            DrawAddSection();
            
            EditorGUILayout.Space();
            
            // データリスト
            DrawDialogueList();
            
            // ユーティリティボタン
            DrawUtilityButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawSearchAndFilters()
        {
            EditorGUILayout.LabelField("検索・フィルター", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField("検索 (CueName)", searchFilter);
            if (GUILayout.Button("クリア", GUILayout.Width(60)))
            {
                searchFilter = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            showEmptyOnly = EditorGUILayout.Toggle("空のメッセージのみ表示", showEmptyOnly);
            showValidOnly = EditorGUILayout.Toggle("完全なデータのみ表示", showValidOnly);
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawAddSection()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            showAddSection = EditorGUILayout.Foldout(showAddSection, "新しいダイアログを追加", true);
            
            if (showAddSection)
            {
                EditorGUILayout.Space(5);
                newCueName = EditorGUILayout.TextField("Cue Name", newCueName);
                
                EditorGUILayout.LabelField("Japanese Message");
                newJapaneseMessage = EditorGUILayout.TextArea(newJapaneseMessage, GUILayout.Height(60));
                
                EditorGUILayout.LabelField("English Message");
                newEnglishMessage = EditorGUILayout.TextArea(newEnglishMessage, GUILayout.Height(60));
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !string.IsNullOrWhiteSpace(newCueName);
                if (GUILayout.Button("追加"))
                {
                    AddNewDialogue();
                }
                GUI.enabled = true;
                
                if (GUILayout.Button("クリア"))
                {
                    ClearAddFields();
                }
                EditorGUILayout.EndHorizontal();
                
                // 重複チェック
                if (!string.IsNullOrWhiteSpace(newCueName) && HasDuplicateCueName(newCueName))
                {
                    EditorGUILayout.HelpBox($"警告: '{newCueName}' は既に存在しています", MessageType.Warning);
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDialogueList()
        {
            EditorGUILayout.LabelField("ダイアログリスト", EditorStyles.boldLabel);
            
            var filteredIndices = GetFilteredIndices();
            
            if (filteredIndices.Count == 0)
            {
                EditorGUILayout.HelpBox("表示するデータがありません", MessageType.Info);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            for (int i = 0; i < filteredIndices.Count; i++)
            {
                int actualIndex = filteredIndices[i];
                DrawDialogueElement(actualIndex, i);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawDialogueElement(int index, int displayIndex)
        {
            SerializedProperty element = dialoguesProperty.GetArrayElementAtIndex(index);
            SerializedProperty cueName = element.FindPropertyRelative("_cueName");
            SerializedProperty japaneseMessage = element.FindPropertyRelative("_japaneseMessage");
            SerializedProperty englishMessage = element.FindPropertyRelative("_englishMessage");
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            // ヘッダー行
            EditorGUILayout.BeginHorizontal();
            
            bool isValid = IsDialogueValid(cueName.stringValue, japaneseMessage.stringValue, englishMessage.stringValue);
            
            // 状態アイコン
            string statusIcon = isValid ? "✓" : "⚠";
            Color statusColor = isValid ? Color.green : Color.orange;
            
            var originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(statusIcon, GUILayout.Width(20));
            GUI.color = originalColor;
            
            // CueName (太字で表示)
            EditorGUILayout.LabelField($"[{displayIndex + 1}] {cueName.stringValue}", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // 削除ボタン
            GUI.color = Color.red;
            if (GUILayout.Button("削除", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("確認", $"'{cueName.stringValue}' を削除しますか？", "削除", "キャンセル"))
                {
                    dialoguesProperty.DeleteArrayElementAtIndex(index);
                    return;
                }
            }
            GUI.color = originalColor;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(3);
            
            // CueNameフィールド
            EditorGUILayout.PropertyField(cueName, new GUIContent("Cue Name"));
            
            // 重複チェック
            if (HasDuplicateCueName(cueName.stringValue, index))
            {
                EditorGUILayout.HelpBox("重複したCue Nameが存在します", MessageType.Error);
            }
            
            // メッセージフィールド
            EditorGUILayout.LabelField("Japanese Message");
            japaneseMessage.stringValue = EditorGUILayout.TextArea(japaneseMessage.stringValue, GUILayout.Height(40));
            
            EditorGUILayout.LabelField("English Message");
            englishMessage.stringValue = EditorGUILayout.TextArea(englishMessage.stringValue, GUILayout.Height(40));
            
            // 文字数表示
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"JP: {japaneseMessage.stringValue.Length}文字", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"EN: {englishMessage.stringValue.Length}文字", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawUtilityButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ユーティリティ", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("空のメッセージを削除"))
            {
                RemoveEmptyMessages();
            }
            
            if (GUILayout.Button("CueNameでソート"))
            {
                SortByCueName();
            }
            
            if (GUILayout.Button("重複を検出"))
            {
                DetectDuplicates();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("CSVエクスポート"))
            {
                ExportToCSV();
            }
            
            if (GUILayout.Button("CSVインポート"))
            {
                ImportFromCSV();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private List<int> GetFilteredIndices()
        {
            var indices = new List<int>();
            
            for (int i = 0; i < dialoguesProperty.arraySize; i++)
            {
                var element = dialoguesProperty.GetArrayElementAtIndex(i);
                var cueName = element.FindPropertyRelative("_cueName").stringValue;
                var jpMessage = element.FindPropertyRelative("_japaneseMessage").stringValue;
                var enMessage = element.FindPropertyRelative("_englishMessage").stringValue;
                
                // 検索フィルター
                if (!string.IsNullOrWhiteSpace(searchFilter) && 
                    !cueName.ToLower().Contains(searchFilter.ToLower()))
                {
                    continue;
                }
                
                // 空のメッセージフィルター
                if (showEmptyOnly && !string.IsNullOrWhiteSpace(jpMessage) && !string.IsNullOrWhiteSpace(enMessage))
                {
                    continue;
                }
                
                // 完全なデータフィルター
                if (showValidOnly && !IsDialogueValid(cueName, jpMessage, enMessage))
                {
                    continue;
                }
                
                indices.Add(i);
            }
            
            return indices;
        }
        
        private bool IsDialogueValid(string cueName, string jpMessage, string enMessage)
        {
            return !string.IsNullOrWhiteSpace(cueName) && 
                   !string.IsNullOrWhiteSpace(jpMessage) && 
                   !string.IsNullOrWhiteSpace(enMessage);
        }
        
        private bool HasDuplicateCueName(string cueName, int excludeIndex = -1)
        {
            if (string.IsNullOrWhiteSpace(cueName)) return false;
            
            int count = 0;
            for (int i = 0; i < dialoguesProperty.arraySize; i++)
            {
                if (i == excludeIndex) continue;
                
                var element = dialoguesProperty.GetArrayElementAtIndex(i);
                var existingCueName = element.FindPropertyRelative("_cueName").stringValue;
                
                if (cueName.Equals(existingCueName, System.StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
            
            return count > 0;
        }
        
        private void AddNewDialogue()
        {
            dialoguesProperty.arraySize++;
            var newElement = dialoguesProperty.GetArrayElementAtIndex(dialoguesProperty.arraySize - 1);
            
            newElement.FindPropertyRelative("_cueName").stringValue = newCueName;
            newElement.FindPropertyRelative("_japaneseMessage").stringValue = newJapaneseMessage;
            newElement.FindPropertyRelative("_englishMessage").stringValue = newEnglishMessage;
            
            ClearAddFields();
            
            EditorUtility.SetDirty(target);
        }
        
        private void ClearAddFields()
        {
            newCueName = "";
            newJapaneseMessage = "";
            newEnglishMessage = "";
        }
        
        private void RemoveEmptyMessages()
        {
            for (int i = dialoguesProperty.arraySize - 1; i >= 0; i--)
            {
                var element = dialoguesProperty.GetArrayElementAtIndex(i);
                var cueName = element.FindPropertyRelative("_cueName").stringValue;
                var jpMessage = element.FindPropertyRelative("_japaneseMessage").stringValue;
                var enMessage = element.FindPropertyRelative("_englishMessage").stringValue;
                
                if (string.IsNullOrWhiteSpace(cueName) || 
                    (string.IsNullOrWhiteSpace(jpMessage) && string.IsNullOrWhiteSpace(enMessage)))
                {
                    dialoguesProperty.DeleteArrayElementAtIndex(i);
                }
            }
            
            EditorUtility.SetDirty(target);
        }
        
        private void SortByCueName()
        {
            var dialogues = new List<DialogueData>();
            LocalizeDataSO localizeData = (LocalizeDataSO)target;
            
            // リフレクションを使用してprivateフィールドにアクセス
            var field = typeof(LocalizeDataSO).GetField("_dialogues", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var dialoguesList = (List<DialogueData>)field.GetValue(localizeData);
                var sortedDialogues = dialoguesList.OrderBy(d => d.CueName).ToList();
                field.SetValue(localizeData, sortedDialogues);
                
                EditorUtility.SetDirty(target);
            }
        }
        
        private void DetectDuplicates()
        {
            var duplicates = new List<string>();
            var cueNames = new HashSet<string>();
            
            for (int i = 0; i < dialoguesProperty.arraySize; i++)
            {
                var element = dialoguesProperty.GetArrayElementAtIndex(i);
                var cueName = element.FindPropertyRelative("_cueName").stringValue;
                
                if (!string.IsNullOrWhiteSpace(cueName))
                {
                    if (cueNames.Contains(cueName))
                    {
                        if (!duplicates.Contains(cueName))
                        {
                            duplicates.Add(cueName);
                        }
                    }
                    else
                    {
                        cueNames.Add(cueName);
                    }
                }
            }
            
            if (duplicates.Count > 0)
            {
                string message = "重複したCue Nameが見つかりました:\n" + string.Join("\n", duplicates);
                EditorUtility.DisplayDialog("重複検出", message, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("重複検出", "重複は見つかりませんでした", "OK");
            }
        }
        
        private void ExportToCSV()
        {
            string path = EditorUtility.SaveFilePanel("CSV Export", "", "localize_data.csv", "csv");
            if (string.IsNullOrEmpty(path)) return;
            
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("CueName,Japanese,English");
            
            LocalizeDataSO localizeData = (LocalizeDataSO)target;
            var field = typeof(LocalizeDataSO).GetField("_dialogues", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var dialoguesList = (List<DialogueData>)field.GetValue(localizeData);
                foreach (var dialogue in dialoguesList)
                {
                    csv.AppendLine($"\"{dialogue.CueName}\",\"{dialogue.JapaneseMessage.Replace("\"", "\"\"")}\",\"{dialogue.EnglishMessage.Replace("\"", "\"\"")}\"");
                }
            }
            
            System.IO.File.WriteAllText(path, csv.ToString(), System.Text.Encoding.UTF8);
            EditorUtility.DisplayDialog("エクスポート完了", $"CSVファイルを保存しました:\n{path}", "OK");
        }
        
        private void ImportFromCSV()
        {
            string path = EditorUtility.OpenFilePanel("CSV Import", "", "csv");
            if (string.IsNullOrEmpty(path)) return;
            
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
                if (lines.Length <= 1)
                {
                    EditorUtility.DisplayDialog("エラー", "有効なCSVデータが見つかりません", "OK");
                    return;
                }
                
                if (!EditorUtility.DisplayDialog("確認", "既存のデータを置き換えますか？", "置き換える", "キャンセル"))
                {
                    return;
                }
                
                dialoguesProperty.arraySize = 0;
                
                for (int i = 1; i < lines.Length; i++) // ヘッダーをスキップ
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = ParseCSVLine(line);
                    if (parts.Length >= 3)
                    {
                        dialoguesProperty.arraySize++;
                        var element = dialoguesProperty.GetArrayElementAtIndex(dialoguesProperty.arraySize - 1);
                        
                        element.FindPropertyRelative("_cueName").stringValue = parts[0];
                        element.FindPropertyRelative("_japaneseMessage").stringValue = parts[1];
                        element.FindPropertyRelative("_englishMessage").stringValue = parts[2];
                    }
                }
                
                EditorUtility.SetDirty(target);
                EditorUtility.DisplayDialog("インポート完了", $"{dialoguesProperty.arraySize}件のデータをインポートしました", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("エラー", $"CSVの読み込みに失敗しました:\n{e.Message}", "OK");
            }
        }
        
        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentField = new System.Text.StringBuilder();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            
            result.Add(currentField.ToString());
            return result.ToArray();
        }
    }
}