using System.Collections.Generic;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using R3;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// 敵のヘルスバーを管理するクラス
    /// </summary>
    public class UIElement_HealthBar : MonoBehaviour
    {
        /// <summary>
        /// ヘルスバーの初期化（呼ばれるタイミングはStageEnemyAdminのStart）
        /// </summary>
        public void RegisterEnemyEvent(EnemyManager enemy)
        {
            // ヘルスバーのUIを生成
            var healthBar = Instantiate(_healthBarPrefab, transform);
            
            // 生成したヘルスバーからCanvasGroupを取得してリストに追加する
            var barCanvasGroup = healthBar.GetComponent<CanvasGroup>();
            _barCanvasGroups.Add(barCanvasGroup);
            
            // 初期状態は非表示にする
            barCanvasGroup.alpha = 0;
            
            // 座標を変更
            healthBar.GetComponent<RectTransform>().anchoredPosition = _initialPosition;
            
            // ゲージUIを取得して敵のHP変動イベントを購読
            var bar = healthBar.transform.GetChild(2).GetComponent<Image>();
            enemy.HealthSystem.OnHealthChanged += h => OnEnemyHealthChange(h, bar, enemy);
        }
        
        [SerializeField] private GameObject _healthBarPrefab; // 敵のヘルスバーのPrefab
        [SerializeField] private Vector2 _initialPosition = new Vector2(0, 30); // ヘルスバーの初期位置
        [SerializeField] private Vector3 _defaultScale = Vector3.one; // デフォルトのScale

        [Header("アニメーションの設定")] 
        [SerializeField, Tooltip("表示にかける時間")] private float _showDuration = 0.2f;
        [SerializeField, Tooltip("非表示にかける時間")] private float _hideDuration = 0.15f;
        [SerializeField, Tooltip("パルスの強さ")] private float _pulseIntensity = 1.1f;
        [SerializeField, Tooltip("ゲージ減少アニメーションにかける時間")] private float _decreaseAnimationDuration = 0.15f;
        [SerializeField, Tooltip("通常時の色")] private Color _normalColor = Color.white;
        [SerializeField, Tooltip("フラッシュ時の色")] private Color _flashColor = Color.white;
        
        private List<CanvasGroup> _barCanvasGroups = new List<CanvasGroup>(); // ヘルスバーの親オブジェクトにアタッチされているCanvasGroupのリスト 
        private CompositeDisposable _disposable = new CompositeDisposable();
        private DG.Tweening.Sequence _decreaseSequence; // HP減少シーケンス
        private int _currentPhase = 0;

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                // フェーズが変更されたタイミングで次の敵のHPバーを表示する
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnPhaseChanged)
                    .AddTo(_disposable);
            }
            else
            {
                Debug.Log($"[{typeof(DamageTextManager)}] {typeof(PhaseManager)}が取得できませんでした");
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        private void OnDestroy()
        {
            // フェーズのリアクティブプロパティの購読解除
            _disposable?.Dispose();
            _decreaseSequence?.Kill();
        }
        
        /// <summary>
        /// ヘルスバーの更新
        /// </summary>
        /// <param name="health">現在のHP</param>
        /// <param name="bar">イベントを発火したEnemyと対応したヘルスバー</param>
        /// <param name="enemy">イベントを発火したEnemy</param>
        private void OnEnemyHealthChange(float health, Image bar, EnemyManager enemy)
        {
            PlayDecreaseSequence(health, bar, enemy);

            if (health <= 0)
            {
                // HPがゼロ以下になったらHPバーを非表示にする
                Hide(_barCanvasGroups[_currentPhase]);
 
                // 内部のフェーズ数のカウントを進める
                _currentPhase++;
            }
        }

        /// <summary>
        /// フェーズが変更されたときの処理
        /// </summary>
        private void OnPhaseChanged(PhaseEnum phase)
        {
            // 対応する敵のヘルスバーを表示
            Show(_barCanvasGroups[_currentPhase]);
            
            // NOTE: HPがゼロ以下になったときにヘルスバーを非表示にする処理を行うときにIndexを使用したいので
            // ここではフェーズの内部カウントは進めない
        }
        
        /// <summary>
        /// ヘルスバーの減少アニメーション
        /// </summary>
        private void PlayDecreaseSequence(float health, Image bar, EnemyManager enemy)
        {
            // 既存の減少シーケンス中があればキル（自然に繋がるように引数は渡さない）
            _decreaseSequence?.Kill();
            
            var rectTransform = bar.GetComponent<RectTransform>();
            rectTransform.localScale = _defaultScale;
            
            // FillAmountを計算しておく
            var targetFillAmount =  health / enemy.HealthSystem.MaxHealth;

            _decreaseSequence = DOTween.Sequence()

                // Y軸方向にパルス
                .Append(rectTransform.DOScaleY(_defaultScale.y * (1f + _pulseIntensity * 0.3f), 0.05f).SetEase(Ease.OutQuart))
                .Append(rectTransform.DOScaleY(_defaultScale.y, 0.1f).SetEase(Ease.OutQuart))

                // ヘルスバーの減少
                .Insert(0, bar.DOFillAmount(targetFillAmount, _decreaseAnimationDuration).SetEase(Ease.OutExpo))

                // フラッシュ効果
                .Insert(0, bar.DOColor(_flashColor, 0.05f).SetEase(Ease.OutQuart)
                    .OnComplete(() => bar.DOColor(_normalColor, 0.15f).SetEase(Ease.OutQuart)));
        }
        
        /// <summary>
        /// ヘルスバーを表示する
        /// </summary>
        private void Show(CanvasGroup target)
        {
            target.alpha = 0;
            target.DOFade(1f, _showDuration).SetEase(Ease.OutQuart);
        }

        /// <summary>
        /// ヘルスバーを非表示にする
        /// </summary>
        private void Hide(CanvasGroup target)
        {
            target.DOFade(0f, _hideDuration).SetEase(Ease.InQuart);
            target.transform.DOScale(0.8f, _hideDuration).SetEase(Ease.InQuart);
        }
    }
}