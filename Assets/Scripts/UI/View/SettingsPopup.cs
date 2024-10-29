using UnityEngine;
using UnityEngine.UI;
namespace UI.View
{
    public sealed class SettingsPopup : BasePopupView
    {
        public Slider MusicsSlider => _musicsSlider;
        public Slider EffectsSlider => _effectsSlider;
        
        [SerializeField] private Slider _musicsSlider;
        [SerializeField] private Slider _effectsSlider;
    }
    
}