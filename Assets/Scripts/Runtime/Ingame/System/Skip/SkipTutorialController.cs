using UnityEngine;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;

namespace BeatKeeper
{
    /// <summary>
    /// チュートリアルスキップを管理するクラス
    /// </summary>
    public class SkipTutorialController : BaseSkipController
    {
        [SerializeField] private PlayableDirector _director;
        
        /// <summary>
        /// スキップ時間
        /// NOTE: TutorialManagerと同じ計算
        /// </summary>
        private double JumpTime => _director.playableAsset.duration* 0.95f;

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        protected override async void Start()
        {
            if (_director == null)
            {
                _director = GetComponent<PlayableDirector>();
            }

            base.Start();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected override void Update()
        {
            // タイムラインアセットがnull
            // タイムラインの再生が終了している
            // チュートリアル再生中ではなかったらスキップ処理は行いたくないので早期return
            if (_director == null || _director.time >= JumpTime || !IsPlayingTutorial())
            {
                return;
            }

            base.Update();
        }

        #endregion
        
        /// <summary>
        /// チュートリアルスキップ
        /// </summary>
        protected override void OnSkip()
        {
            if (_director == null)
            {
                Debug.LogError($"PlayableDirectorがnullのためスキップできません: {typeof(SkipTutorialController)}");
                return;
            }

            // タイムラインの時間を飛ばす
            _director.time = JumpTime;
            _director.Evaluate();
        }

        /// <summary>
        /// チュートリアルがプレイ中か
        /// </summary>
        private bool IsPlayingTutorial()
        {
            if (_director == null)
            {
                return false;
            }

            return _director.time > 0;
        }
    }
}
