using UI.MediatorUI;
using UI.View;
using VContainer;
using VContainer.Unity;
namespace DI
{
    public sealed class LoadingLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LoadingScreenMediator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<LoadingScreenView>();
        }
    }
}