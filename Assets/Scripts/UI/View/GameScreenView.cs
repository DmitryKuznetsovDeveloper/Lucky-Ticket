using UnityEngine;
namespace UI.View
{
    public sealed class GameScreenView : MonoBehaviour
    {
        public BaseButtonView SettingsButton => _settingsButton;
        public BaseButtonView BackButton => _backButton;
        
        [SerializeField] private BaseButtonView _settingsButton;
        [SerializeField] private BaseButtonView _backButton;
    }
}
