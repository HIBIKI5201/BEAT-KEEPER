using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BeatKeeper.Runtime.System
{
    [Serializable]
    public class UnityEventWrapper
    {
        public UnityEventWrapper()
        {
            _unityEvent = new UnityEvent();
        }

        public event Action Event
        {
            add => _action += value;
            remove => _action -= value;
        }

        public void Invoke()
        {
            _action?.Invoke();
            _unityEvent?.Invoke();
        }

        private Action _action;
        [SerializeField]
        private UnityEvent _unityEvent = new();
    }

    [Serializable]
    public class UnityEventWrapper<T>
    {
        public UnityEventWrapper()
        {
            _unityEvent = new UnityEvent<T>();
        }

        public event Action<T> Event
        {
            add => _action += value;
            remove => _action -= value;
        }

        public void Invoke(T arg)
        {
            _action?.Invoke(arg);
            _unityEvent?.Invoke(arg);
        }

        private Action<T> _action;
        [SerializeField]
        private UnityEvent<T> _unityEvent = new();
    }
}
