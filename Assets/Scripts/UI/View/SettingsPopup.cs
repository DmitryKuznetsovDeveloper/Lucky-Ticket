using UnityEngine;
using UnityEngine.UI;
namespace UI.View
{
    public class SettingsPopup : BasePopupView
    {
        public float MusicsVolume => _musicsSlider.value;
        public float EffectsVolume => _effectsSlider.value;
        
        [SerializeField] private Slider _musicsSlider;
        [SerializeField] private Slider _effectsSlider;
    }
}