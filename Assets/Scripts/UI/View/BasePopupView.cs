using Data;
using DG.Tweening;
using UI.View;
using UnityEngine;
using UnityEngine.UI;

public class BasePopupView : MonoBehaviour , IPopup
{
    [SerializeField] private BasePopupConfig _config;
    [SerializeField] private Button _closePopup;
    [SerializeField] private Image _bgMain;
    [SerializeField] private RectTransform _root;
    [SerializeField] private CanvasGroup _rootCanvasGroup;
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    
    private Sequence _showSequence;
    private Sequence _hideSequence;

    private void Awake()
    {
        _hideSequence = InitSequence(_config.HideParams);
        _showSequence = InitSequence(_config.ShowParams);
        _closePopup.onClick.AddListener(Hide);
        SwitchCanvasGroup(false);
    }

    private void OnDisable()
    {
        _closePopup.onClick.RemoveListener(Hide);
        _showSequence.Rewind();
        _hideSequence.Rewind();
        _showSequence.Pause();
        _hideSequence.Pause();
    }
        
    private void OnDestroy()
    {
        _showSequence.Kill();
        _hideSequence.Kill();
    }

    public void Show()
    {
        SwitchCanvasGroup(true);
        _showSequence.OnComplete(() => _showSequence.Pause()).Restart();
    }
    
    public void Hide()
    {
        SwitchCanvasGroup(false);
        _hideSequence.OnComplete(() => _hideSequence.Pause()).Restart();
    }
    
    private Sequence InitSequence(PopupStateParams config)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(_root.DOScale(config.ScaleFactorTarget, config.Params.Duration).From(config.ScaleFactorFrom).SetEase(config.Params.EaseOut));
        sequence.Join(_rootCanvasGroup.DOFade(config.RootFadeTarget, config.Params.Duration).From(config.RootFadeFrom).SetEase(config.Params.EaseIn));
        sequence.Join(_bgMain.DOColor(config.BgColorTarget, config.Params.Duration).From(config.BgColorFrom).SetEase(config.Params.EaseIn));
        sequence.SetDelay(config.Params.Delay);
        sequence.SetRecyclable(true);
        sequence.SetAutoKill(false);
        sequence.SetUpdate(true);
        sequence.Pause();
        return sequence;
    }

    private void SwitchCanvasGroup(bool value)
    {
        _mainCanvasGroup.interactable = value;
        _mainCanvasGroup.blocksRaycasts = value;
    }


}
