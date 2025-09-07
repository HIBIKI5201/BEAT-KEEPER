using System.Collections.Generic;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    ///     シーンの初期化を管理するクラス
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SceneManagerB : MonoBehaviour
    {
        public void Register(Component self)
        {
            _initializeComponentHash.Add(self.GetInstanceID());
        }

        public void Unregister(Component self)
        {
            _initializeComponentHash.Remove(self.GetInstanceID());
        }

        public bool IsInitializeEnd() => _initializeComponentHash.Count == 0;

        private HashSet<int> _initializeComponentHash = new();
    }
}
