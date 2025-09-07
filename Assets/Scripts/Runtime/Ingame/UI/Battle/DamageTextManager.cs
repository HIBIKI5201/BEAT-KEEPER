using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BeatKeeper
{
    /// <summary>
    /// スプライトを使用してプレイヤーが与えたダメージ表記を管理するクラス
    /// </summary>
    public class DamageTextManager : MonoBehaviour
    {
        [Header("コンポーネントの参照")] 
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private UIElement_ScoreText _scoreText;
        [SerializeField] private CanvasGroup _containerCanvasGroup;
        [SerializeField] private RectTransform _plusSignTransform;
        [SerializeField] private Image[] _digitImages = new Image[8];

        [Header("アニメーションの設定")]
        [SerializeField, Tooltip("表示時間")] private float _displayTime = 0.2f;
        [SerializeField, Tooltip("テキストのY軸上の移動距離")] private float _moveDistanceY = 1.5f;
        [SerializeField, Tooltip("イージング")] private Ease _easeType = Ease.InExpo;

        private Sequence _animationSequence; // アニメーションシーケンス
        private Vector3 _initialPosition; // CanvasGroupの初期位置

        private static readonly int[] _digitDivisors = { 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1 }; // 桁数分解計算の最適化用。事前計算済み配列

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
            if (!ValidateComponents())
            {
                return;
            }

            // アニメーション用の初期位置を記録
            _initialPosition = _containerCanvasGroup.transform.localPosition;
            _containerCanvasGroup.alpha = 0; // 初期状態では非表示

            _scoreManager.OnChangeScore += HandleDisplayDamage;
        }
        
        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _animationSequence?.Kill();
            _scoreManager.OnChangeScore -= HandleDisplayDamage;
        }

        #endregion
        
        /// <summary>
        /// ダメージ数値の表示とアニメーション実行
        /// </summary>
        private void HandleDisplayDamage(int damageAmount)
        {
            if (_containerCanvasGroup == null)
            {
                //  CanvasGroupが設定されていなければreturn
                return;
            }

            // 既存のアニメーションがあればKill
            _animationSequence?.Kill();

            // ダメージ数値の表示をセットアップ
            SetupDamageDisplay(damageAmount);

            // アニメーション開始位置にリセット
            _containerCanvasGroup.transform.localPosition = _initialPosition;
            _containerCanvasGroup.alpha = 1;

            // アニメーション終了位置を計算
            var targetPosition = _initialPosition + Vector3.up * _moveDistanceY;

            // アニメーション実行（移動・フェードアニメーション）
            _animationSequence = DOTween.Sequence()
                .Append(_containerCanvasGroup.transform.DOLocalMove(targetPosition, _displayTime).SetEase(_easeType))
                .Join(_containerCanvasGroup.DOFade(0, _displayTime).SetEase(_easeType));
        }

        /// <summary>
        /// ダメージ量に応じてスプライトの表示をセットアップする
        /// </summary>
        private void SetupDamageDisplay(int damage)
        {
            // 全ての数字イメージを一旦非表示に
            foreach (var img in _digitImages)
            {
                img.enabled = false;
            }
            _plusSignTransform.gameObject.SetActive(false);

            if (damage <= 0)
            {
                // ダメージが0以下なら以降の処理は行わない
                return;
            }

            // 8桁制限
            int tempDamage = Mathf.Min(damage, 99999999);
            int firstDigitIndex = -1;

            // 上の桁から調べて、最初に0でない数字が見つかった位置を記録
            for (int i = 0; i < _digitImages.Length; i++)
            {
                int digit = (tempDamage / _digitDivisors[i]) % 10;
                if (digit > 0 && firstDigitIndex == -1)
                {
                    // 最初のゼロではない桁を検出
                    firstDigitIndex = i;
                }

                if (firstDigitIndex != -1)
                {
                    // 最初のゼロではない桁が見つかった後は、0も含めて全ての桁を表示
                    _digitImages[i].enabled = true;
                    _digitImages[i].sprite = _scoreText.NumberSprites[digit];
                }
            }
            
            if (firstDigitIndex == -1 && tempDamage > 0)
            {
                // NOTE: エッジケース　1桁の数字（1-9）の処理
                firstDigitIndex = _digitImages.Length - 1;
                 _digitImages[firstDigitIndex].enabled = true;
                 _digitImages[firstDigitIndex].sprite = _scoreText.NumberSprites[tempDamage % 10];
            }

            if (firstDigitIndex != -1)
            {
                // プラス記号を最初の数字の左側に配置
                _plusSignTransform.gameObject.SetActive(true);
                var firstDigitRect = _digitImages[firstDigitIndex].rectTransform;
                _plusSignTransform.position = firstDigitRect.position - new Vector3(firstDigitRect.rect.width, 0, 0);
            }
        }

        /// <summary>
        /// 必要なコンポーネントが設定されているかチェック
        /// </summary>
        /// <returns></returns>
        private bool ValidateComponents()
        {
            if (_containerCanvasGroup == null)
            {
                Debug.LogError($"[{nameof(DamageTextManager)}] CanvasGroupが設定されていません");
                return false;
            }

            if (_scoreText == null)
            {
                Debug.LogError($"[{nameof(DamageTextManager)}] ScoreTextが設定されていません");
                return false;
            }

            if (_scoreManager == null)
            {
                Debug.LogError($"[{nameof(DamageTextManager)}] ScoreManagerが設定されていません");
                return false;
            }
            return true;
        }
    }
}
