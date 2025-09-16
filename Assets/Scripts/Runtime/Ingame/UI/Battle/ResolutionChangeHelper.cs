using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// ノーツUIの解像度変更を補助するクラス
    /// </summary>
    public class ResolutionChangeHelper : MonoBehaviour
    {
        private CanvasScaler _canvasScaler;
        private RingIndicatorBase _ringIndicator;
        private float _currentMultiplier = 1f;

        private void OnEnable()
        {
            if (_canvasScaler == null)
            {
                // 初期化されておらずCanvasScalerの参照が無ければリターン
                return;
            }

            CalculateResolution();
        }
        
        private void Start()
        {
            if (_canvasScaler == null)
            {
                // InGameUICanvasオブジェクトについているCanvasScalerを取得
                // NOTE: Awake、OnEnableで探すとnull参照になるので、最初はStartで行う
                var parent = transform.parent;
                _canvasScaler = parent.GetComponentInParent<CanvasScaler>();

                CalculateResolution();
            }
        }

        /// <summary>
        /// 解像度の計算を行う
        /// </summary>
        private void CalculateResolution()
        {
            // 基準解像度での倍率を計算
            var referenceScale = _canvasScaler.referenceResolution.x / Screen.width;
            
            // 今後値をかけ算として使用できるように変換
            var multiplier = 1f / referenceScale;
            
            if (Mathf.Abs(multiplier - _currentMultiplier) < 0.001f)
            {
                // 倍率変更がなければreturn
                return;
            } 
            
            // 子オブジェクトを全件取得してwidthとheightを変更
            var childrenObj = gameObject.GetComponentsInChildren<RectTransform>();
            foreach (var child in childrenObj)
            {
                child.sizeDelta *= multiplier;
            }

            if (_ringIndicator == null)
            {
                _ringIndicator = GetComponent<RingIndicatorBase>();
            }
            _ringIndicator.ApplyResolutionMultiply(multiplier);
            
            // 値を上書き
            _currentMultiplier = multiplier;
        }
    }
}
