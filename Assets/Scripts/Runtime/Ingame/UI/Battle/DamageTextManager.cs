using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;
using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEngine.Pool;
using UnityEngine.UI;

public class DamageTextManager : MonoBehaviour
{
    [Header("プールの設定")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _defaultPoolSize = 10;
    [SerializeField] private int _maxPoolSize = 30;
    [SerializeField] private Transform _parent;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _criticalColor = Color.yellow;
    
    [Header("アニメーションの設定")]
    [SerializeField] private float _popupDuration = 0.3f;
    [SerializeField] private float _holdDuration = 0.5f;
    [SerializeField] private float _fadeDuration = 0.4f;
    [SerializeField] private float _moveDistance = 1.5f;
    [SerializeField] private Vector2 _randomOffset = new Vector2(0.5f, 0.3f);
    [SerializeField] private float _scaleMultiplier = 1.5f;
    
    [Header("カメラ参照")]
    private Camera _gameCamera;
    [SerializeField] private Canvas _targetCanvas;
    
    private ObjectPool<GameObject> _textPool;
    private RectTransform _canvasRectTransform;
    private EnemyManager _enemy;
    
    private void Awake()
    {
        InitializePool();
        
        // キャンバスのRectTransformを取得
        if (_targetCanvas != null)
        {
            _canvasRectTransform = _targetCanvas.GetComponent<RectTransform>();
        }
    }

    private async void Start()
    {
        _gameCamera = Camera.main;

        await SceneLoader.WaitForLoadSceneAsync("Battle"); // バトルシーンが読み込まれるまで待機する

        // バトルシーンが読み込まれたら敵の参照を取得する
        _enemy = ServiceLocator.GetInstance<BattleSceneManager>().EnemyAdmin.Enemies[0];

        if (_enemy != null)
        {
            // 敵がダメージを受けた時のイベントにダメージ表記UIの処理を追加する
            _enemy.OnHitAttack += OnEnemyOnHitAttack;
        }
        else
        {
            Debug.Log($"[{typeof(DamageTextManager)}] {typeof(EnemyManager)}が取得できませんでした");
        }
    }

    /// <summary>
    /// ダメージ表記UIを表示する
    /// </summary>
    private void OnEnemyOnHitAttack(int value) => DisplayDamage(value, _enemy.transform.position);

    #region オブジェクトプールとアニメーション

    /// <summary>
    /// オブジェクトプールの初期化
    /// </summary>
    private void InitializePool()
    {
        _textPool = new ObjectPool<GameObject>(
            createFunc: CreateTextInstance,
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            collectionCheck: true,
            defaultCapacity: _defaultPoolSize,
            maxSize: _maxPoolSize
        );
    }
    
    /// <summary>
    /// ダメージテキストのオブジェクトを作成するときに呼ばれるメソッド
    /// </summary>
    private GameObject CreateTextInstance()
    {
        GameObject textInstance = Instantiate(_prefab, _parent);
        textInstance.SetActive(false);
        return textInstance;
    }
    
    /// <summary>
    /// ダメージを表示する
    /// </summary>
    private void DisplayDamage(int damage, Vector3 worldPosition, bool isCritical = false)
    {
        // ワールド座標をキャンバス座標に変換
        Vector2 canvasPosition = WorldToCanvasPosition(worldPosition);
        
        // UIオブジェクトを取得して位置を設定
        GameObject textObject = _textPool.Get();
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = canvasPosition;
        
        Text textComponent = textObject.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = damage.ToString();
            textComponent.color = isCritical ? _criticalColor : _normalColor;
            
            // スケールと色をリセットする
            rectTransform.localScale = Vector3.one;
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
            
            // ランダムな座標を作る
            Vector2 randomPos = new Vector2(
                Random.Range(-_randomOffset.x, _randomOffset.x),
                Random.Range(0, _randomOffset.y)
            );
            
            Sequence sequence = DOTween.Sequence();
            
            // 少し拡大して、上方向にスライドする
            sequence.Append(rectTransform.DOScale(Vector3.one * _scaleMultiplier, _popupDuration).SetEase(Ease.OutBack));
            sequence.Join(rectTransform.DOAnchorPos(
                canvasPosition + randomPos + Vector2.up * _moveDistance, 
                _popupDuration + _holdDuration)
                .SetEase(Ease.OutCubic));
            
            // 待機
            sequence.AppendInterval(_holdDuration);
            
            // 消える処理
            sequence.Append(textComponent.DOColor(new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0f), 
                _fadeDuration).SetEase(Ease.InQuad));
            
            // 手放す処理。テキストを非表示にする
            sequence.OnComplete(() => _textPool.Release(textObject));
        }
    }
    
    /// <summary>
    /// ワールド座標をUIキャンバス上の座標に変換
    /// </summary>
    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        if (_gameCamera == null || _canvasRectTransform == null)
        {
            Debug.LogError("カメラまたはキャンバスが設定されていません");
            return Vector2.zero;
        }
        
        // ワールド座標をスクリーン座標に変換
        Vector2 screenPoint = _gameCamera.WorldToScreenPoint(worldPosition);
        
        // スクリーン座標からキャンバス上の座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform, screenPoint, _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? 
                null : _gameCamera, out Vector2 canvasPosition);
        
        return canvasPosition;
    }

    #endregion
    
    private void OnDestroy()
    {
        _textPool.Clear();
        if(_enemy != null) _enemy.OnHitAttack -= OnEnemyOnHitAttack;
    }
}