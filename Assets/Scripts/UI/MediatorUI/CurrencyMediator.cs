using System;
using Game;
using UI.View;
using VContainer.Unity;
namespace UI.MediatorUI
{
    public sealed class CurrencyMediator : IInitializable,IDisposable
    {
        private readonly CurrencyManager _currencyManager;
        private readonly CurrencyView _currencyView;
    
        public CurrencyMediator(CurrencyManager currencyManager, CurrencyView currencyView)
        {
            _currencyManager = currencyManager;
            _currencyView = currencyView;
        }
        
        public void Initialize() => _currencyManager.OnChangeCurrency += _currencyView.SetCurrency;
        
        public void Dispose() => _currencyManager.OnChangeCurrency -= _currencyView.SetCurrency;



    }
}
