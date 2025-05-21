using System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using UnityEditor;
using UnityEngine;

namespace BeatKeeper.Editor.Ingame.Character
{
    [CustomEditor(typeof(EnemyData))]
    public class EnemyDataDrawer : UnityEditor.Editor
    {
        private const string ARRAY_PROPATY = "_chart";
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
                int value = element.intValue;
                AttackKindEnum kind = (AttackKindEnum)value;
                
                string name = kind.ToString();

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.normal.textColor = Color.white;

                Color originalColor = GUI.backgroundColor;

                bool isAttack = kind != AttackKindEnum.None;
                //攻撃するならグリーン
                GUI.backgroundColor = isAttack ? Color.green : Color.gray;

                //要素配置
                GUILayout.BeginHorizontal();
                
                GUILayout.Label((i + 1).ToString(), GUILayout.Width(30)); //番号を表示
                
                if (GUILayout.Button(kind.ToString(), GUILayout.Width(50), GUILayout.Height(25)))
                {
                    //1以上なら左シフト、0なら1に
                    value = 0 < value ? value << 1 : 1;
                    
                    //もしAttackKindの最大値より大きければ0にリセット
                    //-2はNoneとzero originの補正
                    if (1 << Enum.GetValues(typeof(AttackKindEnum)).Length - 2 < value)
                        value = 0;

                    element.intValue = value;
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
