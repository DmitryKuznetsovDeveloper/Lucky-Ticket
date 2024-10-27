using System;
using Data;
using Game;
using UI.View;
namespace UI.MediatorUI
{
    public sealed class GameScreenMediator : IDisposable
    {
        private readonly CurrencyManager _currencyManager;
        private readonly CurrencyView _currencyView;
        private readonly InfiniteCarousel _carousel;
        private FastTicketConfigs _currentConfig;
        private TicketView _currentTicket;
    
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
            _currentTicket = _carousel.GetCurrentTicketView();
        }

        //TODO: доделать
        public void OnBuyTicket()
        {
            //_currentTicket.Button.PlayOnClick();
        }
    
        public void Dispose()
        {
            _carousel.OnCenterConfigChanged -= OnConfigChanged;
            _currencyManager.OnChangeCurrency -= _currencyView.SetCurrency;
        }
    }
}