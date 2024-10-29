using System;
using Game;
using UI.View;
using VContainer.Unity;
namespace UI.MediatorUI
{
    public sealed class GameScreenMediator : IInitializable, IDisposable
    {
        private readonly GameScreenView _gameScreenView;
        private readonly GameLauncher _gameLauncher;
        private readonly SettingsPopup _settingsPopup;
        private readonly IAudioManager _audioManager;
        
        public GameScreenMediator(GameScreenView gameScreenView, GameLauncher gameLauncher, SettingsPopup settingsPopup, IAudioManager audioManager)
        {
            _gameScreenView = gameScreenView;
            _gameLauncher = gameLauncher;
            _settingsPopup = settingsPopup;
            _audioManager = audioManager;
        }
        
        public void Initialize()
        {
            _gameScreenView.SettingsButton.BaseButton.onClick.AddListener(_audioManager.PlayClickSound);
            _gameScreenView.SettingsButton.BaseButton.onClick.AddListener(SettingsGame);
            _gameScreenView.BackButton.BaseButton.onClick.AddListener(_audioManager.PlayClickSound);
            _gameScreenView.BackButton.BaseButton.onClick.AddListener(BackMenu);
        }
        
        public void Dispose()
        {
            _gameScreenView.BackButton.BaseButton.onClick.RemoveAllListeners();
            _gameScreenView.SettingsButton.BaseButton.onClick.RemoveAllListeners();
        }

        private void BackMenu() => _gameScreenView.BackButton.PlayOnClick(_gameLauncher.LaunchMainMenuScreen);
        private void SettingsGame() => _gameScreenView.SettingsButton.PlayOnClick(_settingsPopup.Show);
    }
}