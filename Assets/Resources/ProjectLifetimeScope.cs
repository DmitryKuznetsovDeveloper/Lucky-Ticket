using Data;
using Game;
using UI.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Resources
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private AudioConfig _audioConfig;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource[] _effectSources;
        [SerializeField] private float _fadeDuration = 1.0f;
        [SerializeField] private GameObject _settingsPopupPrefab;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ApplicationExit>(Lifetime.Singleton).AsSelf();
            builder.Register<GameLauncher>(Lifetime.Singleton).AsSelf();
            
            builder.RegisterInstance(_audioConfig);
            builder.RegisterInstance(_musicSource);
            builder.RegisterInstance(_effectSources);
            builder.RegisterInstance(_fadeDuration);

            builder.Register<IAudioManager, AudioManager>(Lifetime.Singleton).As<IStartable>()
                .WithParameter(_musicSource)
                .WithParameter(_effectSources)
                .WithParameter(_audioConfig)
                .WithParameter(_fadeDuration);
            
            var settingsPopupInstance = Instantiate(_settingsPopupPrefab);
            settingsPopupInstance.name = "SettingsPopup";
            DontDestroyOnLoad(settingsPopupInstance);
            var settingsPopup = settingsPopupInstance.GetComponent<SettingsPopup>();
            builder.RegisterComponent(settingsPopup).AsSelf();
            builder.Register<SettingsGame>(Lifetime.Singleton).As<IInitializable>().AsSelf();
        }
    }
}
