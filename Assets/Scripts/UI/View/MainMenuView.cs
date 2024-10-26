using DG.Tweening;
using UnityEngine;
namespace UI.View
{
    public sealed class MainMenuView : MonoBehaviour
    {
        public BaseButtonView StarButton => _starButton;
        public BaseButtonView SettingsButton => _settingsButton;
        public BaseButtonView ExitButton => _exitButton;
        
        
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private BaseButtonView _starButton;
        [SerializeField] private BaseButtonView _settingsButton;
        [SerializeField] private BaseButtonView _exitButton;

        public void SetFadeButtons(float value,float duration) => _canvasGroup.DOFade(value,duration);
    }
}