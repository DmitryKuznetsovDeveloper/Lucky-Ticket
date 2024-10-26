using DG.Tweening;
using TMPro;
using UI.ParamsTween;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View
{
    public sealed class LoadingScreenView : MonoBehaviour
    {
        public Button StartGameButton => _startGameButton;
        public TMP_Text LoadingLabel => _loadingLabel;
        
        [Header("Slider Settings")]
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _loadingLabel;
        [SerializeField] private RectTransform _decorMove;
        [SerializeField] private CanvasGroup _gradientCanvasGroup;
        [SerializeField] private RectTransform _gradientRectTransform;
        [SerializeField] private TweenParamsOut _tweenParams;

        private readonly Vector2 _startGradientAnchorsMin = new Vector2(0, 0);
        private readonly Vector2 _endGradientAnchorsMin = new Vector2(1, 0);
        private readonly Vector2 _startGradientAnchorsMax = new Vector2(0, 1);
        private readonly Vector2 _endGradientAnchorsMax = new Vector2(1, 1);
        private readonly Vector2 _startDecorMove = new Vector2(0f, 0.5f);
        private readonly Vector2 _endDecorMove = new Vector2(1, 0.5f);
        private Sequence _sequenceLoadingSlider;
        public void SetSlider(float value) => _slider.DOValue(value, _tweenParams.Duration);
        private void Awake()
        {
            _sequenceLoadingSlider = DOTween.Sequence();
            _sequenceLoadingSlider.Append(_gradientCanvasGroup.DOFade(1, _tweenParams.Duration).From(0f));
            _sequenceLoadingSlider.Join(_loadingLabel.DOFade(1f, _tweenParams.Duration).From(0f));
            _sequenceLoadingSlider.Join(_gradientRectTransform.DOAnchorMax(_endGradientAnchorsMax, _tweenParams.Duration).From(_startGradientAnchorsMax));
            _sequenceLoadingSlider.Append(_gradientRectTransform.DOAnchorMin(_endGradientAnchorsMin, _tweenParams.Duration).From(_startGradientAnchorsMin).SetDelay(_tweenParams.Delay));
            _sequenceLoadingSlider.Join(_decorMove.DOAnchorMin(_endDecorMove, _tweenParams.Duration).From(_startDecorMove));
            _sequenceLoadingSlider.Join(_decorMove.DOAnchorMax(_endDecorMove, _tweenParams.Duration).From(_startDecorMove));
            _sequenceLoadingSlider.Join(_decorMove.DOPivot(_endDecorMove, _tweenParams.Duration).From(_startDecorMove));
            _sequenceLoadingSlider.Join(_loadingLabel.DOFade(0f, _tweenParams.Duration).SetDelay(_tweenParams.Delay));
            _sequenceLoadingSlider.Join(_gradientCanvasGroup.DOFade(0, _tweenParams.Duration));
            _sequenceLoadingSlider
                .SetRecyclable(true)
                .SetEase(_tweenParams.EaseOut)
                .SetLoops(-1, LoopType.Restart)
                .SetAutoKill(false)
                .SetUpdate(true)
                .Pause();
        }

        private void PauseSliderAnimation() => _sequenceLoadingSlider?.Pause();
        private void ShowLoadingScreen() => _sequenceLoadingSlider?.Restart();
        private void OnEnable() => ShowLoadingScreen();
        private void OnDisable()
        {
            _sequenceLoadingSlider.OnComplete(PauseSliderAnimation)?.Rewind();
            _startGameButton.onClick.RemoveAllListeners();
        }
        private void OnDestroy() => _sequenceLoadingSlider?.Kill();
    }
}