using System;
using Data;
using Game;
using UI.View;
using VContainer.Unity;
namespace UI.MediatorUI
{
    public sealed class GameScreenMediator : ITickable,IDisposable
    {
        private readonly CurrencyManager _currencyManager;
        private readonly CurrencyView _currencyView;
        private readonly InfiniteCarousel _carousel;
        private FastTicketConfigs _currentConfig;
    
        public GameScreenMediator(CurrencyManager currencyManager, CurrencyView currencyView, InfiniteCarousel carousel)
        {
            _currencyManager = currencyManager;
            _currencyView = currencyView;
            _carousel = carousel;
        
            _carousel.OnCenterConfigChanged += OnConfigChanged;
            _currencyManager.OnChangeCurrency += _currencyView.SetCurrency;
        }

        private void OnConfigChanged(FastTicketConfigs config)
        {
            _currentConfig = config;
        }


        public void Tick()
        {
        }
    
        public void Dispose()
        {
            _carousel.OnCenterConfigChanged -= OnConfigChanged;
            _currencyManager.OnChangeCurrency -= _currencyView.SetCurrency;
        }
    }
}