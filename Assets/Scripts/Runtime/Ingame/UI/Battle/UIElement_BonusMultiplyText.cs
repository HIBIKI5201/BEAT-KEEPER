using R3;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// ボーナススコア倍率を表示するテキスト
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIElement_BonusMultiplyText : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private SpriteAtlas _numberSpriteAtlas; // 数字のスプライトアトラスの参照
        [SerializeField] private Image _ones; // 1の位
        [SerializeField] private Image _decimalPlace; // 小数点の位
        private CanvasGroup _canvasGroup;
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        private const string SPRITE_PREFIX = "number_"; // スプライト名のプレフィックス
        
        private void Start()
        {
            if (_ones == null || _decimalPlace == null || _numberSpriteAtlas == null)
            {
                Debug.LogError("設定されていない参照があります");
                return;
            }
            
            _canvasGroup = GetComponent<CanvasGroup>();
            _scoreManager.BonusMultiply.Subscribe(UpdateText).AddTo(_disposable);
        }

        /// <summary>
        /// スコア倍率の表記を更新する
        /// </summary>
        private void UpdateText(float value)
        {
            if (value <= 1)
            {
                Hide(); // 等倍の時は以降の処理は行わない
                return;
            }
            
            // 各桁を計算
            int intPart = (int)value; // 1の位は小数点切り捨て
            int floatPart = (int)((value - intPart) * 10); // 小数点部分。小数第一位まで対応
            
            // 画像変更処理
            SetSprite(_ones, GetNumberSprite(intPart));
            SetSprite(_decimalPlace, GetNumberSprite(floatPart));
            
            if (value > 1 && _canvasGroup.alpha == 0)
            {
                Show();
            }
        }
        
        /// <summary>
        /// 引数で渡した値の画像をSpriteAtlasから取得する
        /// </summary>
        private Sprite GetNumberSprite(int number)
        {
            if (_numberSpriteAtlas == null)
            {
                // SpriteAtlasが設定されていなかった場合はnullを返す
                return null;
            }
            
            // プレフィックスと受け取った値を連結してスプライト名を作成
            string spriteName = SPRITE_PREFIX + number.ToString();
            return _numberSpriteAtlas.GetSprite(spriteName);
        }
        
        /// <summary>
        /// nullチェックを行ってからスプライトを変更する
        /// </summary>
        private void SetSprite(Image targetImage, Sprite numberSprite)
        {
            if (targetImage == null)
            {
                return;
            }
            
            targetImage.sprite = numberSprite;
        }

        private void Show()
        {
            _canvasGroup.alpha = 1;
        }

        private void Hide()
        {
            _canvasGroup.alpha = 0;
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}
