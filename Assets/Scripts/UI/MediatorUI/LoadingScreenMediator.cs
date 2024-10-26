using Cysharp.Threading.Tasks;
using Game;
using UI.View;
using UnityEngine;
using VContainer.Unity;

namespace UI.MediatorUI
{
    public sealed class LoadingScreenMediator : IStartable
    {
        private readonly LoadingScreenView _loadingScreenView;
        private readonly GameLauncher _gameLauncher;
        private const int SceneToLoadIndex = 2;
        
        public LoadingScreenMediator(LoadingScreenView loadingScreenView, GameLauncher gameLauncher)
        {
            _loadingScreenView = loadingScreenView;
            _gameLauncher = gameLauncher;
        }

         async void IStartable.Start()
        {
            _loadingScreenView.StartGameButton.gameObject.SetActive(false);
            _loadingScreenView.LoadingLabel.gameObject.SetActive(true);
            _loadingScreenView.SetSlider(0);
            _loadingScreenView.StartGameButton.onClick.AddListener(OnContinueButtonClick);
            await LoadSceneAsync();
        }

        private async UniTask LoadSceneAsync()
        {
            AsyncOperation asyncLoad = _gameLauncher.AsyncLaunchLoadingScreen(SceneToLoadIndex);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    _loadingScreenView.StartGameButton.gameObject.SetActive(true);
                    _loadingScreenView.LoadingLabel.gameObject.SetActive(false);
                    _loadingScreenView.SetSlider(1);
                }
                else
                    _loadingScreenView.SetSlider(asyncLoad.progress);

                await UniTask.Yield(); 
            }
        }
        
        private void OnContinueButtonClick() => _gameLauncher.LaunchScene(SceneToLoadIndex);
    }
}