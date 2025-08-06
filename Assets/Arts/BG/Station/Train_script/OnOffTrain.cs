using UnityEngine;
using UnityEngine.Splines;

namespace BeatKeeper
{
    public class OnOffTrain : MonoBehaviour
    {
        
            public SplineAnimate animate;

        void Start()
        {
            if (animate != null)
            {
                Debug.Log("再生開始");
                animate.Play();
            }
            else
            {
                Debug.LogWarning("SplineAnimate が割り当てられていません！");

            }

        }

        void Update()
        {
        }
    }
}


public class SplineStarter : MonoBehaviour
{
    public SplineAnimate animate;

    void Start()
    {
        if (animate != null)
        {
            Debug.Log("再生開始");
            animate.Play();
        }
        else
        {
            Debug.LogWarning("SplineAnimate が割り当てられていません！");

    }
}
    }

