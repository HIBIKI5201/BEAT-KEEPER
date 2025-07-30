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
            add
            {
                if (value == null)
                {
                    Debug.LogWarning("Attempted to add a null action to UnityEventWrapper.");
                    return;
                }

                UnityAction action = new(value);
                _actionToUnityActionMap.Add(value, action);
                _unityEvent.AddListener(action);
            }
            remove
            {
                if (value == null)
                {
                    Debug.LogWarning("Attempted to remove a null action to UnityEventWrapper.");
                    return;
                }

                if (_actionToUnityActionMap.TryGetValue(value, out var action))
                {
                    _actionToUnityActionMap.Remove(value);
                    _unityEvent.RemoveListener(action);
                }
                else
                {
                    Debug.LogWarning("Attempted to remove an action that was not registered in UnityEventWrapper: " + value);
                }
            }
        }

        public void Invoke()
        {
            _unityEvent.Invoke();
        }

        private Dictionary<Action, UnityAction> _actionToUnityActionMap = new();
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
            add
            {
                if (value == null)
                {
                    Debug.LogWarning("Attempted to add a null action to UnityEventWrapper.");
                    return;
                }

                UnityAction<T> action = new(value);
                _actionToUnityActionMap.Add(value, action);
                _unityEvent.AddListener(action);
            }
            remove
            {
                if (value == null)
                {
                    Debug.LogWarning("Attempted to remove a null action to UnityEventWrapper.");
                    return;
                }

                if (_actionToUnityActionMap.TryGetValue(value, out var action))
                {
                    _actionToUnityActionMap.Remove(value);
                    _unityEvent.RemoveListener(action);
                }
                else
                {
                    Debug.LogWarning("Attempted to remove an action that was not registered in UnityEventWrapper: " + value);
                }
            }
        }

        public void Invoke(T arg)
        {
            _unityEvent.Invoke(arg);
        }

        private Dictionary<Action<T>, UnityAction<T>> _actionToUnityActionMap = new();
        [SerializeField]
        private UnityEvent<T> _unityEvent = new();
    }
}
