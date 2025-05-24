using System.Collections.Generic;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    [DefaultExecutionOrder(-1000)]
    public class SceneManagerB : MonoBehaviour
    {
        private HashSet<int> _initializeComponentHash = new();
        
        public void Register(Component self)
        {
            _initializeComponentHash.Add(self.GetInstanceID());
        }

        public void Unregister(Component self)
        {
            _initializeComponentHash.Remove(self.GetInstanceID());
        }
        
        public bool IsInitializeEnd() => _initializeComponentHash.Count == 0;
    }
}
