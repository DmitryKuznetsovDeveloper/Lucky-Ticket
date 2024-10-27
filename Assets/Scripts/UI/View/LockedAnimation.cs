using DG.Tweening;
using UI.ParamsTween;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View
{
    public class LockedAnimation : MonoBehaviour
    {
        [SerializeField] private Image _frameIcon;
        [SerializeField] private Image _centerIcon;
        [SerializeField] private TweenParamsOut _params;

        private Tween _frameRotationTween;
        private Tween _centerRotationTween;

        private void Awake()
        {
            _frameRotationTween = _frameIcon.rectTransform
                .DORotate(new Vector3(0, 0, 360), _params.Duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(2, LoopType.Yoyo)
                .SetDelay(_params.Delay)
                .SetRecyclable(true)
                .SetAutoKill(false)
                .SetUpdate(true)
                .Pause();
            
            _centerRotationTween = _centerIcon.rectTransform
                .DORotate(new Vector3(0, 0, -360), _params.Duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(2, LoopType.Yoyo)
                .SetDelay(_params.Delay)
                .SetRecyclable(true)
                .SetAutoKill(false)
                .SetUpdate(true)
                .Pause();
        }

        private void OnEnable()
        {
            _frameRotationTween.Play().SetLoops(-1);
            _centerRotationTween.Play().SetLoops(-1);
        }

        private void OnDisable()
        {
            _frameRotationTween.Pause();
            _centerRotationTween.Pause();
        }

        private void OnDestroy()
        {
            _frameRotationTween.Kill();
            _centerRotationTween.Kill();
        }
    }
}