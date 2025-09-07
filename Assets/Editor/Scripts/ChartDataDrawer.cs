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
        private const string RANGE_END_POSITIONS = "_rangeEndPositions";
        private const string ATTACK_KIND = "AttackKind";
        private const string POSITION = "Position";
        private SerializedProperty _array;
        private SerializedProperty _visible;
        private SerializedProperty _rangeEndPositions;

        void OnEnable()
        {
            _array = serializedObject.FindProperty(ARRAY_PROPATY);
            _visible = serializedObject.FindProperty(VISIBLE_PROPATY);
            _rangeEndPositions = serializedObject.FindProperty(RANGE_END_POSITIONS);

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
            //終点ノーツ設定用のディクショナリーフィールドは非表示
            DrawPropertiesExcluding(serializedObject, ARRAY_PROPATY, RANGE_END_POSITIONS);

            #region 譜面エディタ

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("譜面編集", EditorStyles.boldLabel);

            ChartData chartData = target as ChartData;
            
            for (int i = 0; i < _array.arraySize; i++)
            {
                //ChartDataElementの要素
                SerializedProperty element = _array.GetArrayElementAtIndex(i);
                SerializedProperty visibleProp = _visible.GetArrayElementAtIndex(i);
                SerializedProperty attackKindProp = element.FindPropertyRelative(ATTACK_KIND);
                SerializedProperty positionProp = element.FindPropertyRelative(POSITION);

                // ノーツの種類
                ChartKindEnum kind = (ChartKindEnum)attackKindProp.enumValueFlag;
                string kindName = kind.ToString();

                // 長押しノーツか判定
                bool isRangeNote = kind == ChartKindEnum.Charge;

                Color originalColor = GUI.backgroundColor;

                // 色変更（以前まで一括して緑だったが、範囲ノーツは青、通常ノーツは緑、なしは灰色に）
                if (isRangeNote)
                {
                    GUI.backgroundColor = Color.cyan;
                }
                else
                {
                    GUI.backgroundColor = kind != ChartKindEnum.None ? Color.green : Color.gray;
                }

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

                    // チャージNotesの終点を設定するためのGUIの表示/非表示を切り替える
                    // Chargeに変更された場合、自動的に範囲ノーツとして設定
                    ChartKindEnum newKind = (ChartKindEnum)value;
                    if (newKind == ChartKindEnum.Charge && !isRangeNote)
                    {
                        // デフォルトで右側に終点を設定
                        Vector2 startPos = positionProp.vector2Value;
                        Vector2 endPos = new Vector2(startPos.x + 200f, startPos.y);
                        chartData.SetRangeEndPosition(i, endPos);
                        EditorUtility.SetDirty(chartData);
                    }
                    else if (newKind != ChartKindEnum.Charge && isRangeNote)
                    {
                        // Charge以外に変更された場合、範囲ノーツ設定を削除
                        chartData.RemoveRangeEndPosition(i);
                        EditorUtility.SetDirty(chartData);
                    }
                }

                #endregion

                #region ポジション

                var halfWidth = SCREEN_SIZE_X / 2;
                var halfHeight = SCREEN_SIZE_Y / 2;

                Vector2 screenPos = new(
                    Mathf.Clamp(positionProp.vector2Value.x, -halfWidth, halfWidth),
                    Mathf.Clamp(positionProp.vector2Value.y, -halfHeight, halfHeight));

                // デフォルトもしくは始点ノーツの座標入力フィールド
                positionProp.vector2Value =
                    EditorGUILayout.Vector2Field(GUIContent.none, screenPos, GUILayout.Width(150));

                #endregion

                #region 範囲ノーツの終点座標

                // Chargeノーツの場合、終点座標を表示・編集
                if (kind == ChartKindEnum.Charge)
                {
                    Vector2? currentEndPos = chartData.GetRangeEndPosition(i);
                    Vector2 endPos = currentEndPos ?? new Vector2(screenPos.x + 200f, screenPos.y);
                    
                    // 終点座標を画面サイズ内にクランプ
                    endPos = new Vector2(
                        Mathf.Clamp(endPos.x, -halfWidth, halfWidth),
                        Mathf.Clamp(endPos.y, -halfHeight, halfHeight));
                    
                    Vector2 newEndPos = EditorGUILayout.Vector2Field(GUIContent.none, endPos, GUILayout.Width(150));
                    if (newEndPos != endPos || !currentEndPos.HasValue)
                    {
                        chartData.SetRangeEndPosition(i, newEndPos);
                        EditorUtility.SetDirty(chartData);
                    }
                }

                #endregion

                GUILayout.EndHorizontal();

                if ((i + 1) % 4 == 0) GUILayout.Space(10);
                GUI.backgroundColor = originalColor;
            }

            serializedObject.ApplyModifiedProperties();

            #endregion
        }

        private void SceneGUI(SceneView sceneView)
        {
            ChartData chartData = target as ChartData;
            Vector2 centerPos = new Vector2(Screen.width / 2, Screen.height / 2);

            // Sceneビューのカメラからスクリーン座標を変換
            Camera cam = sceneView.camera;
            if (cam == null) return;

            for (int i = 0; i < _array.arraySize; i++)
            {
                if (!_visible.GetArrayElementAtIndex(i)?.boolValue ?? false) continue;

                SerializedProperty element = _array.GetArrayElementAtIndex(i);
                SerializedProperty attackKindProp = element.FindPropertyRelative(ATTACK_KIND);
                ChartKindEnum kind = (ChartKindEnum)attackKindProp.enumValueFlag;
                
                // スクリーン座標（左下原点）からRayを飛ばす
                Vector2 screenPos = _array.GetArrayElementAtIndex(i).FindPropertyRelative(POSITION).vector2Value;
                Ray ray = cam.ScreenPointToRay(screenPos + centerPos);
                Vector3 worldPos = ray.origin + ray.direction * 10f; // 適当な距離で可視化

                // デフォルト（チャージノーツの場合は開始ノーツ）の描画
                Handles.color = Color.red;
                Handles.DrawSolidDisc(worldPos, -cam.transform.forward, 0.2f);

                // 範囲ノーツの場合、終点も描画
                if (chartData.HasEndPosition(i))
                {
                    Vector2? endPos = chartData.GetRangeEndPosition(i);
                    if (endPos.HasValue)
                    {
                        Ray endRay = cam.ScreenPointToRay(endPos.Value + centerPos);
                        Vector3 endWorldPos = endRay.origin + endRay.direction * 10f;
                        
                        // 終点の描画
                        Handles.color = Color.magenta;
                        Handles.DrawSolidDisc(endWorldPos, -cam.transform.forward, 0.15f);
                        
                        // 開始点から終点への線を描画
                        Handles.color = Color.yellow;
                        Handles.DrawLine(worldPos, endWorldPos);
                        
                        // 終点のラベル
                        Handles.Label(endWorldPos + Vector3.up * 0.5f, $"{i}E({endPos.Value.x:F0}, {endPos.Value.y:F0})");
                    }
                }
                
                // ラベル表示（オプション）
                Handles.Label(worldPos + Vector3.up * 0.5f, $"{i + 1}({screenPos.x:F0}, {screenPos.y:F0})");
            }

            // 再描画を強制
            SceneView.RepaintAll();
        }
    }
}