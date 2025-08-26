using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     敵の攻撃警告・回避UI
    /// </summary>
    public class EnemyIndicator : RingIndicatorBase
    {
        public override int EffectLength => 3;

        /// <summary>
        /// ビートごとに実行される処理
        /// </summary>
        /// <param name="count"></param>
        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                // 1拍目　点滅して表示 -> 1拍目から縮小するように修正
                case 1:
                    InitializeComponents();
                    // StartBlinkEffect(); // 警告のような点滅アニメーション
                    StartContractionEffect(); // 収縮アニメーション
                    break;
				
				// 	誤反応対策として念のために直前にイベント登録を行う
				case 2:
					_player.OnPerfectAvoid += HandlePerfect;
					_player.OnGoodAvoid += HandleGood;
                    _player.OnFailedAvoid += PlayFailEffect;
					break;
            }
        }

        public override void End()
        {
            // イベント購読解除
            _player.OnPerfectAvoid -= HandlePerfect;
            _player.OnGoodAvoid -= HandleGood;
            _player.OnFailedAvoid -= PlayFailEffect;

            base.End();
            
            // UIのリセット
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);

            //敵攻撃はノックバックを与えるので確認
            _chartRingManager.CheckAllRingIndicatorRemainTime();
        }
        
        #region チュートリアル用のメソッド

        public void OnPlayerAvoidSuccess(bool isPerfect)
        {
            OnPlayerSuccessForced(isPerfect);
        }
        
        #endregion

        [Header("追加の色設定")]
        [SerializeField] private Color _warningColor = Color.red;

        private void Start()
        {
			_centerImage.enabled = true;
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // Tweenの配列を作成
            _tweens = new Tween[3];

            // 初期状態の設定
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

		protected override void HandlePerfect() => OnPlayerSuccess(true);
        protected override void HandleGood() => OnPlayerSuccess(false);
    }
}
