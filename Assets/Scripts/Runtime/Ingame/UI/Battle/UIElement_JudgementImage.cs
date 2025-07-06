using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;

namespace BeatKeeper
{
    /// <summary>
    /// 判定表示UIを管理するクラス
    /// </summary>
    public class UIElement_JudgementImage : MonoBehaviour
    {
        [SerializeField] private Image _image; // 自身のImageコンポーネント
        
        [Header("各判定の画像")]
        [SerializeField] private Sprite _perfect; 
        [SerializeField] private Sprite _good; 
        [SerializeField] private Sprite _miss;

        #region アニメーションの設定に関する変数

        [Header("アニメーション時間")]
        [SerializeField, Tooltip("登場アニメーションにかける時間")] private float _appearanceDuration = 0.15f;
        [SerializeField, Tooltip("縮小アニメーションにかける時間")] private float _shrinkDuration = 0.1f;
        [SerializeField, Tooltip("バウンスアニメーションにかける時間")] private float _bounceDuration = 0.3f;
        [SerializeField, Tooltip("バウンスアニメーションの振動回数")] private int _bounceVibrato = 3;
        [SerializeField, Tooltip("アニメーション間の待機時間")] private float _waitInterval = 0.5f;
        [SerializeField, Tooltip("退場アニメーションにかける時間")] private float _exitDuration = 0.4f;

        [Header("アニメーション設定")]
        [SerializeField, Tooltip("登場時の最大スケール倍率")] private float _punchScale = 1.3f;
        [SerializeField, Tooltip("縮小時のスケール倍率")] private float _shrinkScale = 0.95f;
        [SerializeField, Tooltip("バウンスアニメーションの強度")] private float _bounceIntensity = 0.8f;
        [SerializeField, Tooltip("退場アニメーション時のY軸方向の移動量")] private float _exitMoveDistance = 50f;

        [Header("アニメーションのイージング")]
        [SerializeField, Tooltip("登場アニメーションのイージング")] private Ease _entranceEase = Ease.OutBack;
        [SerializeField, Tooltip("縮小アニメーションのイージング")] private Ease _shrinkEase = Ease.InQuad;
        [SerializeField, Tooltip("バウンスアニメーションのイージング")] private Ease _bounceEase = Ease.OutElastic;
        [SerializeField, Tooltip("退場アニメーションのイージング")] private Ease _exitEase = Ease.OutQuad;


        #endregion
        
        private PlayerManager _playerManager; // Dispose用にキャッシュしておく

        private Vector3 _initialScale; // 初期の拡大率
        private Vector3 _initialPosition; // 初期位置
        private Sequence _judgementSequence; // アニメーション制御用のシーケンス
        
        #region Lifecycle

        private void Awake()
        {
            // NOTE: シーン読み込みの順番の関係上、UIが最初にチラ見えしてしまう可能性があるので、初期化だけAwakeで行う
            
            if (_image == null)
            {
                // Imageコンポーネントがアサインされていない場合、取得できるか試す
                if (!gameObject.TryGetComponent(out _image))
                {
                    Debug.LogError("Imageコンポーネントが取得できませんでした");
                    return;
                }
            }
            
            // 初期状態を保存
            _initialScale = _image.transform.localScale;
            _initialPosition = _image.transform.position;
            
            // 初期化
            Setup();
        }
        
        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            // PlayerManagerの参照が取得できるのを待つ
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();

            // PlayerManagerが正常に取得できた場合、イベントを購読
            if (_playerManager != null)
            {
                // アクション購読
                _playerManager.OnPerfectAttack += HandlePerfect;
                _playerManager.OnGoodAttack += HandleGood;
                // TODO: ミスの場合を検討中
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        private void OnDestroy()
        {
            if (_playerManager != null)
            {
                // アクションの登録解除
                _playerManager.OnPerfectAttack -= HandlePerfect;
                _playerManager.OnGoodAttack -= HandleGood;
            }

            // 実行中のアニメーションがあれば強制終了
            _judgementSequence?.Kill();
        }

        #endregion
        
        /// <summary>
        /// パーフェクト判定のときの処理
        /// </summary>
        private void HandlePerfect()
        {
            PlayJudgementAnimation(_perfect);
        }

        /// <summary>
        /// グッド判定のときの処理
        /// </summary>
        private void HandleGood()
        {
            // NOTE: ブラッシュアップでアニメーションの種類を分ける可能性があると思い分離
            PlayJudgementAnimation(_good);
        }

        /// <summary>
        /// アニメーション再生
        /// </summary>
        private void PlayJudgementAnimation(Sprite sprite)
        {
            // 既存のアニメーションを停止
            _judgementSequence?.Kill();
            _judgementSequence = DOTween.Sequence();

            // NOTE: OnCompleatだとKillされた場合呼ばれないためアニメーション開始前に初期化を行う
            Setup();
            
            // 画像変更
            _image.sprite = sprite;

            // 登場アニメーション（スケールを0から_punchScaleまで拡大＋フェードイン）
            _judgementSequence.Append(_image.transform.DOScale(_punchScale, _appearanceDuration).SetEase(_entranceEase));
            _judgementSequence.Join(_image.DOFade(1, _appearanceDuration));

            // 拡大後、少し縮ませる
            _judgementSequence.Append(_image.transform.DOScale(_initialScale * _shrinkScale, _shrinkDuration).SetEase(_shrinkEase));

            // 弾ませる TODO: Punchも書いてみたが派手すぎたので一旦コメントアウト 
            // _judgementSequence.Append(_image.transform.DOPunchScale(Vector3.one * _bounceIntensity, _bounceDuration, _bounceVibrato).SetEase(_bounceEase));

            // 表示時間を確保するために少し待機
            _judgementSequence.AppendInterval(_waitInterval);

            // 退場アニメーション（上に移動＋フェードアウト）
            _judgementSequence.Append(_image.transform.DOLocalMoveY(
                _image.transform.localPosition.y + _exitMoveDistance, _exitDuration).SetEase(_exitEase));
            _judgementSequence.Join(_image.DOFade(0, _exitDuration));
        }
        
        /// <summary>
        /// Imageを初期状態にする
        /// </summary>
        /// <returns></returns>
        private void Setup()
        {
            // 初期状態は非表示にする
            _image.transform.localScale = Vector3.zero;
            
            // 透明化
            Color textColor = _image.color;
            textColor.a = 0;
            _image.color = textColor;
            
            // 位置を初期位置にリセット
            _image.transform.position = _initialPosition;
        }
    }
}
