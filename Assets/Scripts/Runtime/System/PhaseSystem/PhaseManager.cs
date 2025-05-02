using R3;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// インゲームのフェーズを管理するクラス
    /// </summary>
    public class PhaseManager : MonoBehaviour
    {
        /// <summary>
        /// 現在のフェーズ
        /// </summary>
        public PhaseEnum CurrentPhase => _currentPhaseProp.Value;
        public ReadOnlyReactiveProperty<PhaseEnum> CurrentPhaseProp => _currentPhaseProp;
        private readonly ReactiveProperty<PhaseEnum> _currentPhaseProp = new ReactiveProperty<PhaseEnum>();

        private bool _skipChangedBGM;
        
        private void Start()
        {
            // MusicUnityクラスが存在するか検索する（フェーズシステム単体をテストしやすくするため）
            if (FindObjectOfType<MusicUnity>() == null)
            {
                _skipChangedBGM = true;
                Debug.Log("MusicUnityが見つかりません。BGMの変更をスキップします");
            }
        }
        
        [ContextMenu("あ")]
        private void Test()
        {
            SetPhase(PhaseEnum.Battle1);
        }
        
        /// <summary>
        /// フェーズを変更する
        /// </summary>
        public void SetPhase(PhaseEnum nextPhase)
        {
            _currentPhaseProp.Value = nextPhase;
            ChangeBGM();
            
            Debug.Log($"[PhaseManager] フェーズが変更されました 現在：{CurrentPhase}");
        }

        /// <summary>
        /// BGMを変更する
        /// </summary>
        private void ChangeBGM()
        {
            if (CurrentPhase == PhaseEnum.Clear)
            {
                return; //TODO: Clearフェーズの処理を作る
            }
            
            int index = (int)CurrentPhase;
            if (!_skipChangedBGM)
            {
                Music.SetHorizontalSequence(((BGMEnum)index).ToString());
            }
        }
    }
}
