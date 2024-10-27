using System.Collections.Generic;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI.View
{
    public sealed class TicketView : AnimationStatesUIElement<TicketView.AnimationStateTicket, TicketSettings, TicketAnimationsConfig>
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _rewardLabel;
        [SerializeField] private Image _scratchFrame;
        [SerializeField] private RawImage _scratchCenter;
        [SerializeField] private Image _artIcon;
        [SerializeField] private Image _frame;
        [SerializeField] private BaseButtonView _buyButton;
        [SerializeField] private GameObject _lockedCanvasGroup;

        public void SetTitleLabel(string value) => _titleLabel.text = value;
        public void SetRewardLabel(int value) => _rewardLabel.text = $"{value}";
        public void SetArtImage(Sprite image) => _artIcon.sprite = image;
        public void SetScratchFrame(Sprite image) => _scratchFrame.sprite = image;
        public void SetScratchCenter(Texture texture) => _scratchCenter.texture = texture;
        public void SetLocked(bool locked) => _lockedCanvasGroup.SetActive(locked);
        public void SetPriceLabel(int value) => _buyButton.SetLabel(value);
        public enum AnimationStateTicket { Normal, Selected,SelectedLocked, Locked }
        protected override void InitializeSequences()
        {
            AnimationSequences = new Dictionary<AnimationStateTicket, Sequence>()
            {
                { AnimationStateTicket.Normal, CreateSequence(_config.Normal) },
                { AnimationStateTicket.Selected, CreateSequence(_config.Selected) },
            };
        }
        protected override Sequence CreateSequence(TicketSettings config)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_root.DOScale(config.ScaleFactor, config.Params.Duration));
            sequence.Join(_frame.DOColor(config.FrameColor, config.Params.Duration));
            sequence.SetEase(config.Params.EaseOut);
            sequence.SetDelay(config.Params.Delay);
            sequence.SetRecyclable(true);
            sequence.SetAutoKill(false);
            sequence.SetUpdate(true);
            sequence.Pause();
            return sequence;
        }
        protected override AnimationStateTicket GetDefaultState() => AnimationStateTicket.Normal;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))PlayAnimation(AnimationStateTicket.Normal);
            if (Input.GetKeyDown(KeyCode.S))PlayAnimation(AnimationStateTicket.Selected);
        }
    }

}