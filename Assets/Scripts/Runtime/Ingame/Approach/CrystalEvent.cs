using UnityEngine;
using static SymphonyFrameWork.System.PauseManager;
using SymphonyFrameWork.Utility;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    /// <summary>
    /// クリスタルのイベントを管理します
    /// </summary>
    public class CrystalEvent : IPausable
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>.
        public CrystalEvent()
        {
            IPausable.RegisterPauseManager(this);
        }
        /// <summary>
        /// クリスタルの取得処理を開始します
        /// </summary>
        /// <param name="crystal">クリスタルインターフェイスを実装したオブジェクト</param>
        public void CrystalEventStart(ICrystal crystal)
        {
            
        }
        public void CrystalEventEnd()
        {
            
        }
        public void Pause()
        {
        }

        public void Resume()
        {
        }
    }
}
