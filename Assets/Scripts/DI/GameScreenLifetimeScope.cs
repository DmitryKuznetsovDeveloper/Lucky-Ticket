using Data;
using Game;
using UI.MediatorUI;
using UI.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace DI
{
    public class GameScreenLifetimeScope : LifetimeScope
    {
        [SerializeField] private CurrencyConfig _currencyConfig;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CurrencyManager>(Lifetime.Singleton).WithParameter(_currencyConfig.Currency).As<IStartable>().AsSelf();
            builder.Register<CurrencyMediator>(Lifetime.Singleton).As<IInitializable>().AsSelf();
            builder.RegisterComponentInHierarchy<CurrencyView>().AsSelf();
            
            builder.RegisterComponentInHierarchy<InfiniteCarousel>().AsSelf();
            builder.Register<GameScreenMediator>(Lifetime.Singleton).As<IInitializable>();
            builder.RegisterComponentInHierarchy<GameScreenView>();
        }
    }
}