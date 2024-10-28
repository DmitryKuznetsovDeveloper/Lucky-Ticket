using Game;
using VContainer;
using VContainer.Unity;

namespace Resources
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ApplicationExit>(Lifetime.Singleton).AsSelf();
            builder.Register<GameLauncher>(Lifetime.Singleton).AsSelf();
        }
    }
}
