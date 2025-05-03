using BeatKeeper.Runtime.Ingame.Character;
using UnityEditor;
using UnityEngine;

namespace BeatKeeper.Editor.Ingame.Character
{
    [CustomEditor(typeof(EnemyData))]
    public class EnemyDataDrawer : UnityEditor.Editor
    {
        private const string ARRAY_PROPATY = "_beat";
        private SerializedProperty _array;
        
        void OnEnable()
        {
            _array = serializedObject.FindProperty(ARRAY_PROPATY);
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            //配列以外のパラメータを表示
            DrawPropertiesExcluding(serializedObject, ARRAY_PROPATY);

            #region 譜面エディタ
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("譜面編集", EditorStyles.boldLabel);
            
            for (int i = 0; i < _array.arraySize; i++)
            {
                SerializedProperty element = _array.GetArrayElementAtIndex(i);
                bool current = element.boolValue;

                //スタイル
                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = Color.white;

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = current ? Color.green : Color.gray;

                //要素配置
                GUILayout.BeginHorizontal();
                
                GUILayout.Label((i + 1).ToString(), GUILayout.Width(30));
                
                if (GUILayout.Button(current ? "〇" : "×", GUILayout.Width(50), GUILayout.Height(25)))
                {
                    element.boolValue = !current;
                }

                GUILayout.EndHorizontal();

                //節ごとにスペースを空ける
                if ((i + 1) % 4 == 0) GUILayout.Space(10);
                
                GUI.backgroundColor = originalColor;
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }
    }
}
