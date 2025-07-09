using BeatKeeper.Runtime.Ingame.Battle;
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
        private const string VISIBLE_PROPATY = "_visible";
        private const string ATTACK_KIND = "AttackKind";
        private const string POSITION = "Position";
        private SerializedProperty _array;
        private SerializedProperty _visible;

        void OnEnable()
        {
            _array = serializedObject.FindProperty(ARRAY_PROPATY);
            _visible = serializedObject.FindProperty(VISIBLE_PROPATY);

            SceneView.duringSceneGui += SceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= SceneGUI;

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
                SerializedProperty visibleProp = _visible.GetArrayElementAtIndex(i);
                SerializedProperty attackKindProp = element.FindPropertyRelative(ATTACK_KIND);
                SerializedProperty positionProp = element.FindPropertyRelative(POSITION);

                ChartKindEnum kind = (ChartKindEnum)attackKindProp.enumValueFlag;
                string kindName = kind.ToString();

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = kind != ChartKindEnum.None ? Color.green : Color.gray;

                GUILayout.BeginHorizontal();

                GUILayout.Label($"{i}", GUILayout.Width(30));

                bool visible = visibleProp.boolValue;
                visibleProp.boolValue = GUILayout.Toggle(visible, string.Empty, GUILayout.Width(20));

                #region 譜面種類

                if (GUILayout.Button(kindName, GUILayout.Width(80), GUILayout.Height(25)))
                {
                    int value = (int)kind;
                    value = 0 < value ? value << 1 : 1;

                    // AttackKindEnum の最大値を超えたらリセット
                    if (value > (1 << (Enum.GetValues(typeof(ChartKindEnum)).Length - 2)))
                        value = 0;

                    attackKindProp.enumValueFlag = value;
                }

                #endregion

                #region ポジション

                var halfWidth = SCREEN_SIZE_X / 2;
                var halfHeight = SCREEN_SIZE_Y / 2;

                Vector2 screenPos = new(
                    Mathf.Clamp(positionProp.vector2Value.x, -halfWidth, halfWidth),
                    Mathf.Clamp(positionProp.vector2Value.y, -halfHeight, halfHeight));
                positionProp.vector2Value = EditorGUILayout.Vector2Field(GUIContent.none, screenPos, GUILayout.Width(150));

                #endregion

                GUILayout.EndHorizontal();

                if ((i + 1) % 4 == 0) GUILayout.Space(10);
                GUI.backgroundColor = originalColor;
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        private void SceneGUI(SceneView sceneView)
        {
            Vector2 centerPos = new Vector2(Screen.width / 2, Screen.height / 2);

            // Sceneビューのカメラからスクリーン座標を変換
            Camera cam = sceneView.camera;
            if (cam == null) return;

            for (int i = 0; i < _array.arraySize; i++)
            {
                if (!_visible.GetArrayElementAtIndex(i)?.boolValue ?? false) continue;

                // スクリーン座標（左下原点）からRayを飛ばす
                Vector2 screenPos = _array.GetArrayElementAtIndex(i).FindPropertyRelative(POSITION).vector2Value;
                Ray ray = cam.ScreenPointToRay(screenPos + centerPos);
                Vector3 worldPos = ray.origin + ray.direction * 10f; // 適当な距離で可視化

                Handles.color = Color.red;
                Handles.DrawSolidDisc(worldPos, -cam.transform.forward, 0.2f);

                // ラベル表示（オプション）
                Handles.Label(worldPos + Vector3.up * 0.5f, $"{i + 1}({screenPos.x:F0}, {screenPos.y:F0})");
            }

            // 再描画を強制
            SceneView.RepaintAll();
        }
    }
}