using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using R3;

namespace BeatKeeper
{
    /// <summary>
    /// スプライトを使用してプレイヤーが与えたダメージ表記を管理するクラス
    /// </summary>
    public class DamageTextManager : MonoBehaviour
    {
        [Header("コンポーネントの参照")]
        [SerializeField] private UIElement_ScoreText _scoreText;
        [SerializeField] private CanvasGroup _containerCanvasGroup;
        [SerializeField] private RectTransform _plusSignTransform;
        [SerializeField] private Image[] _digitImages = new Image[8];

        [Header("アニメーションの設定")]
        [SerializeField, Tooltip("表示時間")] private float _displayTime = 0.2f;
        [SerializeField, Tooltip("テキストのY軸上の移動距離")] private float _moveDistanceY = 1.5f;
        [SerializeField, Tooltip("イージング")] private Ease _easeType = Ease.InExpo;

        private EnemyManager _enemy; // アクティブな敵のマネージャー
        private Sequence _animationSequence; // アニメーションシーケンス
        private Vector3 _initialPosition; // CanvasGroupの初期位置
        private readonly CompositeDisposable _disposable = new CompositeDisposable(); // R3のSubscribe解除管理用CompositeDisposable

        private static readonly int[] _digitDivisors = { 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1 }; // 桁数分解計算の最適化用。事前計算済み配列

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            if (!ValidateComponents())
            {
                return;
            }

            // アニメーション用の初期位置を記録
            _initialPosition = _containerCanvasGroup.transform.position;
            _containerCanvasGroup.alpha = 0; // 初期状態では非表示

            // Battleシーンのロードが完了するまで待機
            await SceneLoader.WaitForLoadSceneAsync("Battle");
            
            // フェーズ変更の監視を開始する
            SubscribeToPhaseChange();
        }
        
        /// <summary>
        /// Destory
        /// </summary>
        private void OnDestroy()
        {
            _animationSequence?.Kill();
            _disposable?.Dispose();
            
            if (_enemy != null)
            {
                _enemy.OnHitAttack -= HandleDisplayDamage;
            }
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
            _containerCanvasGroup.transform.position = _initialPosition;
            _containerCanvasGroup.alpha = 1;

            // アニメーション終了位置を計算
            var targetPosition = _initialPosition + Vector3.up * _moveDistanceY;

            // アニメーション実行（移動・フェードアニメーション）
            _animationSequence = DOTween.Sequence()
                .Append(_containerCanvasGroup.transform.DOMove(targetPosition, _displayTime).SetEase(_easeType))
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
            return true;
        }

        /// <summary>
        /// PhaseManagerの変更イベントを購読
        /// </summary>
        private void SubscribeToPhaseChange()
        {
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp.Subscribe(OnPhaseChanged).AddTo(_disposable);
            }
            else
            {
                Debug.Log($"[{nameof(DamageTextManager)}] {nameof(PhaseManager)}が取得できませんでした");
            }
        }

        /// <summary>
        /// フェーズ変更時、新しいアクティブな敵を取得する
        /// </summary>
        private void OnPhaseChanged(PhaseEnum phase)
        {
            if (_enemy != null)
            {
                _enemy.OnHitAttack -= HandleDisplayDamage;
            }

            _enemy = ServiceLocator.GetInstance<BattleSceneManager>().EnemyAdmin.GetActiveEnemy();

            if (_enemy != null)
            {
                _enemy.OnHitAttack += HandleDisplayDamage;
            }
            else
            {
                Debug.Log($"[{nameof(DamageTextManager)}] {nameof(EnemyManager)}が取得できませんでした");
            }
        }
    }
}
