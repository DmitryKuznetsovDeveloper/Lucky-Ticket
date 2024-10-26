using UI.MediatorUI;
using UI.View;
using VContainer;
using VContainer.Unity;
namespace DI
{
    public sealed class MainMenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MainMenuMediator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<MainMenuView>();
            builder.RegisterComponentInHierarchy<SettingsPopup>();
            
        }
    }
}
