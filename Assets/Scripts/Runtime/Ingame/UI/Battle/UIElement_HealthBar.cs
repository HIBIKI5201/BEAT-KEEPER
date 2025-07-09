using System.Collections.Generic;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
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
            var bar = healthBar.transform.GetChild(0).GetComponent<Image>();
            enemy.HealthSystem.OnHealthChanged += h => OnEnemyHealthChange(h, bar, enemy);
        }
        
        [SerializeField] private GameObject _healthBarPrefab; // 敵のヘルスバーのPrefab
        [SerializeField] private Vector2 _initialPosition = new Vector2(0, 30); // ヘルスバーの初期位置

        private List<CanvasGroup> _barCanvasGroups = new List<CanvasGroup>(); // ヘルスバーの親オブジェクトにアタッチされているCanvasGroupのリスト 
        private CompositeDisposable _disposable = new CompositeDisposable();
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
        }
        
        /// <summary>
        /// ヘルスバーの更新
        /// </summary>
        /// <param name="health">現在のHP</param>
        /// <param name="bar">イベントを発火したEnemyと対応したヘルスバー</param>
        /// <param name="enemy">イベントを発火したEnemy</param>
        private void OnEnemyHealthChange(float health, Image bar, EnemyManager enemy)
        {
            bar.fillAmount = health / enemy.HealthSystem.MaxHealth;

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
        /// ヘルスバーを非表示にする
        /// </summary>
        private void Hide(CanvasGroup target) => target.alpha = 0;

        /// <summary>
        /// ヘルスバーを表示する
        /// </summary>
        private void Show(CanvasGroup target) => target.alpha = 1; 
    }
}