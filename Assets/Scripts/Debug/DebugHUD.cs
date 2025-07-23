using BeatKeeper.Runtime.Ingame.System;
using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace BeatKeeper.Runtime.Develop
{
    public class DebugHUD : MonoBehaviour
    {
        private float deltaTime = 0.0f;

        void Update()
        {
            // フレーム時間の加算
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(10, 10, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 1 / 50;
            style.normal.textColor = Color.white;

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;

            long totalMemory = GC.GetTotalMemory(false); // 管理メモリ（GCの影響受ける）
            long monoMemory = Profiler.GetMonoUsedSizeLong(); // Mono管理メモリ
            long totalAllocated = Profiler.GetTotalAllocatedMemoryLong(); // 全体の割り当て
            long totalReserved = Profiler.GetTotalReservedMemoryLong(); // 予約済み

            string text = string.Format(
                "FPS: {0:0.} ({1:0.0} ms)\n" +
                "Mono Memory: {2} MB\n" +
                "Total Allocated: {3} MB\n" +
                "Total Reserved: {4} MB\n",
                fps, msec,
                (monoMemory / (1024 * 1024)),
                (totalAllocated / (1024 * 1024)),
                (totalReserved / (1024 * 1024))
                );

            if (Music.Current != null)
            {
                (int just, int near) beat = (MusicEngineHelper.GetBeatSinceStart(), MusicEngineHelper.GetBeatNearerSinceStart());
                text += $"Beat: just {beat.just}, near {beat.near}";
                text += $"\nBar:{Music.Just.Bar}, Just:{Music.Just.Beat}";
            }

            GUI.Label(rect, text, style);
        }
    }
}
