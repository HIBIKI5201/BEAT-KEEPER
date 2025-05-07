using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Approach
{
    /// <summary>
    /// クリスタルのスクリプトに実装してください
    /// </summary>
    public interface ICrystal
    {
        /// <summary>
        /// クリスタルの取得処理を実行します
        /// </summary>
        public void Get();
        //クリスタルイベントを発生させるためのスプラインインデックスを取得します
        public int CrystalEventSplineIndex { get;}
        //クリスタルイベントの終了発生させるためのスプラインインデックスを設定します
        public int CrystalEventEndSplineIndex { get;}
    }
}
