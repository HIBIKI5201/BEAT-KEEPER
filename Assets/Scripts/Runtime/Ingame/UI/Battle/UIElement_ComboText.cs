using BeatKeeper.Runtime.Ingame.Character;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// コンボのテキストを管理するクラス
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIElement_ComboText : MonoBehaviour
    {
        [Header("初期設定")]
        [SerializeField] private Image[] _numberImages = new Image[3];
        [SerializeField] private SpriteAtlas _numberSpriteAtlas; // 数字のSpriteAtlas
        
        [SerializeField] private int _showThreshold = 5; // コンボを表示するしきい値
        
        private PlayerManager _playerManager; // ComboSystem取得用
        private CanvasGroup _canvasGroup;
        private CompositeDisposable _disposables = new CompositeDisposable();
        
        private const string SPRITE_PREFIX = "number_"; // スプライト名のプレフィックス
        private const int MAX_COMBO = 999; // コンボの最大値

        private void Awake()
        {
            // NOTE: Awake段階で非表示にしないと見えてしまうので、先にCanvasGroupだけ初期化を行ってしまう
            _canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }
        
        private async void Start()
        {
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();
            
            if (_numberImages.Length < 2 || _numberImages == null)
            {
                Debug.LogError($"[{typeof(UIElement_ComboText)}] Imageコンポーネントが足りません");
                return;
            }
            
            if (_numberSpriteAtlas == null)
            {
                Debug.LogError($"[{typeof(UIElement_ComboText)}] SpriteAtlasが設定されていません");
                return;
            }
            
            _playerManager.ComboSystem.ComboCount.Subscribe(UpdateText).AddTo(_disposables);
        }

        /// <summary>
        /// コンボ数を更新する
        /// </summary>
        private void UpdateText(int comboCount)
        {
            // 数字の更新処理
            UpdateNumberDisplay(comboCount);
            
            if (comboCount == 0)
            {
                // コンボがゼロになったら非表示にする
                Hide();
            }
            else if (_canvasGroup.alpha == 0 && comboCount >= _showThreshold)
            {
                // コンボがしきい値以上で、かつ非表示状態だったら表示処理を行う
                Show();
            }
        }

        /// <summary>
        /// 数字の表示を更新する
        /// </summary>
        private void UpdateNumberDisplay(int comboCount)
        {
            // 最大コンボ数でクランプ
            int clampedCombo = Mathf.Clamp(comboCount, 0, MAX_COMBO);
            
            // NOTE: 一旦最大コンボ数は3桁とおいて実装をすすめる
            
            // 各桁を計算
            int hundreds = clampedCombo / 100;
            int tens = (clampedCombo / 10) % 10;
            int ones = clampedCombo % 10;

            // 画像変更処理
            SetSprite(_numberImages[0], GetNumberSprite(hundreds));
            SetSprite(_numberImages[1], GetNumberSprite(tens));
            SetSprite(_numberImages[2], GetNumberSprite(ones));
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

        private void Show() => _canvasGroup.alpha = 1;
        
        private void Hide() => _canvasGroup.alpha = 0;

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
