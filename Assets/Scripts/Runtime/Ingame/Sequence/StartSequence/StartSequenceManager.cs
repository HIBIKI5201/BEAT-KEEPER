using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    /// 開始演出
    /// </summary>
    public class StartSequenceManager : MonoBehaviour
    {
        [SerializeField] PlayableDirector _tutorialPlayableDirector;
        private async void Start()
        {
            var multiSceneManager = ServiceLocator.GetInstance<MultiSceneManager>();
            if (multiSceneManager)
            {
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Stage);
                await multiSceneManager.WaitForSceneLoad(SceneListEnum.Battle);
            }

            var director = GetComponent<PlayableDirector>();
            //スタートシーケンスの再生終了時にチュートリアルシーケンスを再生する
            director.stopped += (_) =>
            {
                _tutorialPlayableDirector.Play();
            };
            director.Play();
            //現在最初にBGMが再生されていないのでコメントアウト
            //director.playableGraph.GetRootPlayable(0).SetSpeed((float)Music.CurrentTempo / 60);
        }
    }
}
