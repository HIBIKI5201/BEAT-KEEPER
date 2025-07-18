using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using R3;

/// <summary>
/// スコアUIの近くのプレイヤーが与えたダメージ表記を管理するクラス
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    [Header("コンポーネントの参照")]
    [SerializeField] private Text _damageText;
    [SerializeField] private Color _textColor;
    
    [Header("アニメーションの設定")]
    [SerializeField, Tooltip("表示時間")] private float _displayTime = 0.2f;
    [SerializeField, Tooltip("テキストのY軸上の移動距離")] private float _moveDistanceY = 1.5f;
    [SerializeField, Tooltip("イージング")] private Ease _easeType = Ease.InExpo;
    
    private EnemyManager _enemy; // 敵がダメージを受けた時のイベントを購読しているため、解除用のキャッシュを保持している
    private Sequence _textSequence; // テキストアニメーションのシーケンス
    private Vector3 _initPosition; // テキストオブジェクトの初期位置
    
    private CompositeDisposable _disposable = new CompositeDisposable();
    
    private async void Start()
    {
        if (_damageText == null)
        {
            Debug.LogError($"[{typeof(DamageTextManager)}] テキストコンポーネントが設定されていません");
            return;
        }
        
        // テキストコンポーネントの初期位置を保存・α値は0にして見えないようにしておく
        _initPosition = _damageText.transform.position;
        _damageText.color = new Color(_textColor.r, _textColor.g, _textColor.b, 0);
        
        // 敵のクラスの参照を取りたいので、敵オブジェクトが存在するバトルシーンが読み込まれるまで待機する
        await SceneLoader.WaitForLoadSceneAsync("Battle");

        SubscribeToPhaseChange();
    }

    /// <summary>
    /// フェーズ変更のリアクティブプロパティを購読する
    /// </summary>
    private void SubscribeToPhaseChange()
    {
        var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
        if (phaseManager)
        {
            // フェーズが変更されたタイミングで敵のHitイベントの登録をしなおす
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
    /// フェーズ変更時に敵がダメージを受けた時のアクション登録を行う
    /// </summary>
    private void OnPhaseChanged(PhaseEnum phase)
    {
        if (_enemy != null)
        {
            // 既存のアクション登録を解除
            _enemy.OnHitAttack -= HandleDisplayDamage;
        }
        
        // 新しい敵の参照を取得する
        _enemy = ServiceLocator.GetInstance<BattleSceneManager>().EnemyAdmin.GetActiveEnemy();

        if (_enemy != null)
        {
            // 敵がダメージを受けた時のイベントにダメージ表記UIの処理を追加する
            _enemy.OnHitAttack += HandleDisplayDamage;
        }
        else
        {
            Debug.Log($"[{typeof(DamageTextManager)}] {typeof(EnemyManager)}が取得できませんでした");
        }
    }
    
    /// <summary>
    /// ダメージを表示する
    /// </summary>
    private void HandleDisplayDamage(int damageAmount)
    {
        if (_damageText == null)
        {
            // コンポーネントが設定されていない場合はreturn
            return;
        }
        
        // 既存シーケンスがあったらKill
        _textSequence?.Kill();
        _textSequence = DOTween.Sequence();
        
        // テキストの初期化
        SetupText(damageAmount);
        
        // 現在位置から相対的に移動する（絶対位置ではなく相対位置を指定）
        var targetPosition = _initPosition + Vector3.up * _moveDistanceY;

        // テキストがスライドするアニメーション
        _textSequence.Append(_damageText.transform.DOMove(targetPosition, _displayTime).SetEase(_easeType));
        
        // スライドアニメーションと同時にフェードアウトも実行する
        _textSequence.Join(_damageText.DOFade(0, _displayTime).SetEase(_easeType));
    }

    /// <summary>
    /// テキストアニメーション時の初期化処理
    /// </summary>
    private void SetupText(int damageAmount)
    {
        _damageText.transform.position = _initPosition;
        _damageText.text = $"+{damageAmount}";
        _damageText.color = _textColor;
    }

    private void OnDestroy()
    {
        // アニメーションの停止
        _textSequence?.Kill();
        
        // フェーズ変更のリアクティブプロパティの購読解除
        _disposable?.Dispose();
        
        if (_enemy != null)
        {
            _enemy.OnHitAttack -= HandleDisplayDamage;
        }
    }
}