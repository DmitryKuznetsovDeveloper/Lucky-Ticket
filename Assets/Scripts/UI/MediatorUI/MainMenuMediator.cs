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
        
        public MainMenuMediator(ApplicationExit applicationExit, GameLauncher gameLauncher, MainMenuView mainMenuView, SettingsPopup settingsPopup)
        {
            _applicationExit = applicationExit;
            _gameLauncher = gameLauncher;
            _mainMenuView = mainMenuView;
            _settingsPopup = settingsPopup;
        }
        public void Initialize()
        {
            _mainMenuView.StarButton.BaseButton.onClick.AddListener(StartGame);
            _mainMenuView.SettingsButton.BaseButton.onClick.AddListener(SettingsGame);
            _mainMenuView.ExitButton.BaseButton.onClick.AddListener(ExitGame);
        }
        
        public void Dispose()
        {
            _mainMenuView.StarButton.BaseButton.onClick.RemoveListener(StartGame);
            _mainMenuView.SettingsButton.BaseButton.onClick.RemoveListener(SettingsGame);
            _mainMenuView.ExitButton.BaseButton.onClick.RemoveListener(ExitGame);
        }

        private void StartGame() => _mainMenuView.StarButton.PlayOnClick(_gameLauncher.LaunchLoadingScreen);
        private void SettingsGame() => _mainMenuView.SettingsButton.PlayOnClick(_settingsPopup.Show);
        private void ExitGame() => _mainMenuView.ExitButton.PlayOnClick(_applicationExit.ExitApp);
    }
}