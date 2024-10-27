using System;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View
{
    [RequireComponent(typeof(Button))]
    public sealed class BaseButtonView : MonoBehaviour
    {
        public Button BaseButton => _button;
        
        [SerializeField] private BaseButtonConfig _config;
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _bg;
        [SerializeField] private TMP_Text _labelButton;

        private Sequence _clickSequence;

        private void Awake() => _clickSequence = InitSequence(_config);

        private void OnDisable()
        {
            _clickSequence.Rewind();
            _clickSequence.Pause();
        }
        
        private void OnDestroy() => _clickSequence.Kill();

        public void SetLabel(string value) => _labelButton.text = value;
        public void SetLabel(int value) => _labelButton.text = $"{value}";
        public void PlayOnClick( Action action)
        {
            _clickSequence.OnComplete(() =>
            {
                _clickSequence.Pause();
                action?.Invoke();
            }).Restart();
        }
        
        private Sequence InitSequence(BaseButtonConfig config)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_root.DOScale(config.ScaleFactor, config.Params.Duration).From(1).SetLoops(2, LoopType.Yoyo));
            sequence.Join(_bg.DOColor(config.BgColor, config.Params.Duration).SetLoops(2, LoopType.Yoyo));
            sequence.SetEase(config.Params.EaseOut);
            sequence.SetDelay(config.Params.Delay);
            sequence.SetRecyclable(true);
            sequence.SetAutoKill(false);
            sequence.SetUpdate(true);
            sequence.Pause();
            return sequence;
        }
    }
}