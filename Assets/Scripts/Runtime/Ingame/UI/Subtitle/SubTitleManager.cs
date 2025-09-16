using SymphonyFrameWork.System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    public class SubTitleManager : MonoBehaviour
    {
        [SerializeField, Header("最短のテキスト表示時間")] private float _shortestDisplayTime = 2f;
        [SerializeField, Header("一文字ごとの表示時間の係数")] private float _perCharacterTimeMultiplier = 0.2f;
        [SerializeField, Header("テキストボックスおよびテキストのオブジェクト")] private Image _textBox;
        [SerializeField] private Text _subTitleText;
        [SerializeField] private int _textBoxHeightPerLine = 80;
        private LocalizeTextManager _localizeTextManager;
        private float _currentTime = 0f;


        private async void Awake()
        {
            StartCoroutine(TextHide());
            _textBox.enabled = false;
            _subTitleText.enabled = false;
            _localizeTextManager = await ServiceLocator.GetInstanceAsync<LocalizeTextManager>();
        }

        /// <summary> スタートムービーのテキストを表示する </summary>
        /// <param name="cueName"></param>
        public void StartMovieTextShow(string cueName) => TextShow(_localizeTextManager.GetStartMovieMessage(cueName));

        /// <summary> チュートリアルの字幕を表示する </summary>
        /// <param name="cueName"></param>
        public void TutorialSubtitleTextShow(string cueName) => TextShow(_localizeTextManager.GetTutorialSubtitleMessage(cueName));

        /// <summary> クリアムービーの字幕を表示する </summary>
        /// <param name="cueName"></param>
        public void ClearMovieTextSHow(string cueName) => TextShow(_localizeTextManager.GetClearMovieMessage(cueName));

        /// <summary> リザルトの字幕を表示する </summary>
        /// <param name="cueName"></param>
        public void ResultTextShow(string cueName) => TextShow(_localizeTextManager.GetResultMessage(cueName));

        /// <summary>
        /// テキストを表示する。
        /// </summary>
        /// <param name="text"></param>
        private void TextShow(string text)
        {
            int count = text.Count(c => c == '\n') + 1;
            _textBox.rectTransform.sizeDelta = new Vector2(_textBox.rectTransform.sizeDelta.x, _textBoxHeightPerLine * count);
            _textBox.enabled = true;
            _subTitleText.enabled = true;
            _subTitleText.text = text;
            _currentTime = Mathf.Max(Time.time + _shortestDisplayTime, Time.time + _perCharacterTimeMultiplier * text.Length);
        }

        /// <summary>
        /// 一定時間後にテキストを非表示するコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerator TextHide()
        {
            while (true)
            {
                if (_currentTime <= Time.time)
                {
                    _textBox.enabled = false;
                    _subTitleText.enabled = false;
                }
                yield return null;
            }
        }
    }
}
