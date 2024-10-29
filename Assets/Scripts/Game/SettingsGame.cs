using System;
using UI.View;
using VContainer.Unity;
namespace Game
{
    public sealed class SettingsGame : IInitializable, IDisposable
    {
        private readonly IAudioManager _audioManager;
        private readonly SettingsPopup _settingsPopup;
        
        public SettingsGame(IAudioManager audioManager, SettingsPopup settingsPopup)
        {
            _audioManager = audioManager;
            _settingsPopup = settingsPopup;
        }
        public void Initialize()
        {
            _settingsPopup.MusicsSlider.onValueChanged.AddListener(_audioManager.SetMusicVolume);
            _settingsPopup.EffectsSlider.onValueChanged.AddListener(_audioManager.SetEffectsVolume);
        }

        public void Dispose()
        {
            _settingsPopup.MusicsSlider.onValueChanged.RemoveListener(_audioManager.SetMusicVolume);
            _settingsPopup.EffectsSlider.onValueChanged.RemoveListener(_audioManager.SetEffectsVolume);
        }
    }
}