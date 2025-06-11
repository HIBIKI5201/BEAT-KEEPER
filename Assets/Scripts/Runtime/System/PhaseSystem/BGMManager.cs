using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// フェーズ情報を元にBGMを変更するためのクラス
    /// </summary>
    public class BGMManager : MonoBehaviour
    {
        /* 旧BGM変更処理
        /// <summary>
        /// BGMを変更する
        /// </summary>
        private void OnPhaseChanged(PhaseEnum newPhase)
        {
            switch (newPhase)
            {
                case PhaseEnum.Movie:
                    int index = (int)newPhase;
                    Music.SetHorizontalSequence(((BGMEnum)index).ToString());
                    Debug.Log("[BGMChanger] BGMを変更しました" + newPhase);
                    break;

                case PhaseEnum.Clear:
                    //TODO: Clearフェーズの処理を作る(曲の変更ではなく音響効果の変更を行う予定)
                    break;
            }
        }
        */

        /// <summary>
        ///     BGMを変更する
        /// </summary>
        /// <param name="name"></param>
        public void ChangeBGM(string name)
        {
            Music.SetHorizontalSequence(name);
            Debug.Log($"{nameof(BGMManager)} BGMを変更しました");
        }
    }
}