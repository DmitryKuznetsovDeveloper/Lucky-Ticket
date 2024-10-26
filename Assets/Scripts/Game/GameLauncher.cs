using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public sealed class GameLauncher
    {
        private const string LoadingScene = "Loading";
        private const string MainMenu = "MainMenu";
        public void LaunchLoadingScreen() => SceneManager.LoadScene(LoadingScene);
        public void LaunchMainMenuScreen() => SceneManager.LoadScene(MainMenu);
        public void LaunchScene(int value) => SceneManager.LoadScene(value);
        
        public AsyncOperation AsyncLaunchLoadingScreen(int value) => SceneManager.LoadSceneAsync(value);
    }
}