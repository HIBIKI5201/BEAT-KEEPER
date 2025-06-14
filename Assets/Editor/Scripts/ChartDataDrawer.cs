﻿using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using System;
using UnityEditor;
using UnityEngine;

namespace BeatKeeper.Editor.Ingame.Character
{
    [CustomEditor(typeof(ChartData))]
    public class ChartDataDrawer : UnityEditor.Editor
    {
        private const int SCREEN_SIZE_X = 1920;
        private const int SCREEN_SIZE_Y = 1080;
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
                //ChartDataElementの要素
                SerializedProperty element = _array.GetArrayElementAtIndex(i);
                SerializedProperty attackKindProp = element.FindPropertyRelative("AttackKind");
                SerializedProperty positionProp = element.FindPropertyRelative("Position");

                ChartKindEnum kind = (ChartKindEnum)attackKindProp.enumValueFlag;
                string kindName = kind.ToString();

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = kind != ChartKindEnum.None ? Color.green : Color.gray;

                GUILayout.BeginHorizontal();

                GUILayout.Label($"{i + 1}", GUILayout.Width(30));

                if (GUILayout.Button(kindName, GUILayout.Width(80), GUILayout.Height(25)))
                {
                    int value = (int)kind;
                    value = 0 < value ? value << 1 : 1;

                    // AttackKindEnum の最大値を超えたらリセット
                    if (value > (1 << (Enum.GetValues(typeof(ChartKindEnum)).Length - 2)))
                        value = 0;

                    attackKindProp.enumValueFlag = value;
                }

                var halfWidth = SCREEN_SIZE_X / 2;
                var halfHeight = SCREEN_SIZE_Y / 2;

                Vector2 positionClamp = new(
                    Mathf.Clamp(positionProp.vector2Value.x, -halfWidth, halfWidth), 
                    Mathf.Clamp(positionProp.vector2Value.y, -halfHeight, halfHeight));
                positionProp.vector2Value = EditorGUILayout.Vector2Field(GUIContent.none, positionClamp, GUILayout.Width(150));

                GUILayout.EndHorizontal();

                if ((i + 1) % 4 == 0) GUILayout.Space(10);
                GUI.backgroundColor = originalColor;
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }
    }
}
