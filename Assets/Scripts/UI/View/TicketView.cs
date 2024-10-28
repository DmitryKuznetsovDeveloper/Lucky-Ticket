using System.Collections.Generic;
using Data;
using DG.Tweening;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI.View
{
    public sealed class TicketView : AnimationStatesUIElement<TicketView.AnimationStateTicket, TicketSettings, TicketAnimationsConfig>
    {
        public SimpleScratch SimpleScratch => _simpleScratch;
        public BaseButtonView Button => _buyButton;
        [SerializeField] private RectTransform _root;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _rewardLabel;
        [SerializeField] private Image _artIcon;
        [SerializeField] private Image _frame;
        [SerializeField] private BaseButtonView _buyButton;
        [SerializeField] private RectTransform _locked;
        [SerializeField] private SimpleScratch _simpleScratch;

        public void SetTitleLabel(string value) => _titleLabel.text = value;
        public void SetRewardLabel(int value) => _rewardLabel.text = $"{value}";
        public void SetArtImage(Sprite image) => _artIcon.sprite = image;
        public void SetLocked(bool locked) => _locked.gameObject.SetActive(locked);
        public void SetBuyButton(bool value) => _buyButton.gameObject.SetActive(value);
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
    }

}