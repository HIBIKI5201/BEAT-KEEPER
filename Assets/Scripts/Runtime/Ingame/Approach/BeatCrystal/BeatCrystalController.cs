using DG.Tweening;
using UnityEngine;

namespace BeatKeeper
{
    public class BeatCrystalController : MonoBehaviour
    {
        [Header("アニメーションの設定")]
        [SerializeField] private float _defaultRotateDuration = 5f; // 回転アニメーションのデフォルトの速度
        [SerializeField] private float _lockOnRotationDuration = 3f; // ロックオン中の速度
        
        [Header("コンポーネント")]
        [SerializeField] private ParticleSystem _ambientLightParticle; // 周囲に漂う小さなパーティクル
        
        private Tweener _rotateTweener;
        
        private void Start()
        {
            DefaultSetting();
        }
        
        /// <summary>
        /// デフォルトのアニメーション
        /// </summary>
        public void DefaultSetting()
        {
            StartRotationAnimation(_defaultRotateDuration);
            // 音楽のビートに合わせた微妙な明滅（輝度15%〜25%の範囲）
            _ambientLightParticle.Play(); // パーティクルを生成
        }

        /// <summary>
        /// ロックオン中のエフェクト
        /// </summary>
        public void LockOn()
        {
            StartRotationAnimation(_lockOnRotationDuration);
            // ビートに合わせて強く明滅（輝度25%〜75%の範囲）
            // 色相が青からピンク/パープルに変化
            // 結晶の輪郭がより鮮明になり、エッジが光る
            // 内部のエネルギーコアの脈動が激しくなる
        }

        /// <summary>
        /// 回収時
        /// </summary>
        public void Get()
        {
            // 結晶が一瞬最大輝度（100%）で爆発的に光る
            // 爆発時に輝く光の波が周囲に広がる（半径2〜3メートル）
            // 結晶が細かい光の粒子に**分解**される（50〜100個程度のパーティクル）
            // パーティクルがプレイヤーに吸収されていく（0.5〜1秒程度でプレイヤーに到達）
            // 各パーティクルは軽い加速度を持ち、途中で尾を引く
            // 最終的にプレイヤーの周囲で螺旋を描きながら消滅
        }

        /// <summary>
        /// オブジェクトを回転させるTween
        /// </summary>
        private void StartRotationAnimation(float duration)
        {
            _rotateTweener.Kill(); // 現在の回転Tweenを止める
            _rotateTweener = transform.DORotate(new Vector3(0f, 360f, 0f), duration, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
        }
        
        /*
        ## 全体演出効果

        ### 画面効果（ロックオン時）

        - ビネットエフェクト（画面端を25%〜30%暗くする）
        - FOV値を通常の85°から75°程度に縮小（軽いズームイン効果）
        - 色彩の飽和度を10%程度上昇
        - 被写界深度効果で背景をやや不鮮明に
        - クリスタル以外の明るさを10%程度下げる

        ### 画面効果（回収時）

        - FOVを一瞬だけ70°まで縮小した後、元に戻す（パルス効果）
        - カメラに軽い衝撃波効果（0.1〜0.2秒の揺れ）
        - 画面全体に一瞬だけレンズフレア効果
        - ビネットと色彩効果を0.5秒かけて元に戻す

        ### 音響効果

        - BGMを10%程度減速させる
        - BGMにローパスフィルター（くぐもった音質）
        - 環境音をエコーがかかったような効果に
        - 回収時に「キラン」という爽快感のある効果音
        - 粒子がプレイヤーに吸収される際に上昇音階の効果音
        */
    }
}
