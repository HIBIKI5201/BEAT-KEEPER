using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// リザルトのコンボ数やPerfect数などを表示するUIを管理するクラス
    /// </summary>
    public class UIContents_Result : MonoBehaviour
    {
        [SerializeField] private Text _amountText; // 値が入力されるテキストの参照

        private void Start()
        {
            if (_amountText == null)
            {
                Debug.LogWarning($"Textコンポーネントがアサインされていませんでした。2つ目の子オブジェクトから自動取得します: {this}");
                _amountText = transform.GetChild(1).GetComponent<Text>();
            }
        }
        
        /// <summary>
        /// 値を入力する
        /// </summary>
        public void SetAmount(int amount)
        {
            // 値を入力
            // TODO: 必要であればここに演出のコードを書く
            _amountText.text = amount.ToString();
        }
    }
}
