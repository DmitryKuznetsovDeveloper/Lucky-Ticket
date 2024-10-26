using Game;
using VContainer;
using VContainer.Unity;
namespace DI
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Регистрируем глобальные зависимости
            builder.Register<ApplicationExit>(Lifetime.Singleton).AsSelf();
            builder.Register<GameLauncher>(Lifetime.Singleton).AsSelf();
        }
    }
}
