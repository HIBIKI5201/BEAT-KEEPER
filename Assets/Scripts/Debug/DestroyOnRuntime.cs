using System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    ///     ゲーム開始時に破壊するオブジェクト
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class DestroyOnRuntime : MonoBehaviour
    {
        private void Awake()
        {
             Destroy(this.gameObject);
        }
    }
}
