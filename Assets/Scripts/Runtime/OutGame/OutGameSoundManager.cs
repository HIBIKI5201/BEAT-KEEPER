using CriWare;
using UnityEngine;

namespace BeatKeeper
{
    public class OutGameSoundManager : MonoBehaviour
    {
        [SerializeField] private string _bgmName = "OutGameBGM";
        [SerializeField] private string _seName = "OutGameSE";
        [SerializeField] private CriAtomSource _criAtomSourceBGM;
        [SerializeField] private CriAtomSource _criAtomSourceSE;


        private void Awake()
        {
            _criAtomSourceBGM.cueName = _bgmName;
            _criAtomSourceSE.cueName = _seName;
        }

        public void GameStart()
        {
            _criAtomSourceSE.Play();
        }
    }
}
