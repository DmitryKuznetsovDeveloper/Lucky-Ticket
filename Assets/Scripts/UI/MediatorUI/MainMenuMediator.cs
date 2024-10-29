using System;
using Game;
using UI.View;
using VContainer.Unity;

namespace UI.MediatorUI
{
    public sealed class MainMenuMediator : IInitializable, IDisposable
    {
        private readonly ApplicationExit _applicationExit;
        private readonly GameLauncher _gameLauncher;
        private readonly MainMenuView _mainMenuView;
        private readonly SettingsPopup _settingsPopup;
        private readonly IAudioManager _audioManager;
        
        public MainMenuMediator(ApplicationExit applicationExit, GameLauncher gameLauncher, MainMenuView mainMenuView, SettingsPopup settingsPopup, IAudioManager audioManager)
        {
            _applicationExit = applicationExit;
            _gameLauncher = gameLauncher;
            _mainMenuView = mainMenuView;
            _settingsPopup = settingsPopup;
            _audioManager = audioManager;
        }
        public void Initialize()
        {
            _mainMenuView.StarButton.BaseButton.onClick.AddListener( _audioManager.PlayClickSound);
            _mainMenuView.StarButton.BaseButton.onClick.AddListener(StartGame);
            _mainMenuView.SettingsButton.BaseButton.onClick.AddListener(_audioManager.PlayClickSound);
            _mainMenuView.SettingsButton.BaseButton.onClick.AddListener(SettingsGame);
            _mainMenuView.ExitButton.BaseButton.onClick.AddListener(_audioManager.PlayClickSound);
            _mainMenuView.ExitButton.BaseButton.onClick.AddListener(ExitGame);
        }
        
        public void Dispose()
        {
            _mainMenuView.StarButton.BaseButton.onClick.RemoveAllListeners();
            _mainMenuView.SettingsButton.BaseButton.onClick.RemoveAllListeners();
            _mainMenuView.ExitButton.BaseButton.onClick.RemoveAllListeners();
        }

        private void StartGame() =>
            _mainMenuView.StarButton.PlayOnClick(_gameLauncher.LaunchLoadingScreen);
        private void SettingsGame() => _mainMenuView.SettingsButton.PlayOnClick(_settingsPopup.Show);
        private void ExitGame() => _mainMenuView.ExitButton.PlayOnClick(_applicationExit.ExitApp);
    }
}