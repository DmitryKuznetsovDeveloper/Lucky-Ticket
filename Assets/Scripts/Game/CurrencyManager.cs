using System;
using VContainer.Unity;
namespace Game
{
    public sealed class CurrencyManager : IStartable
    {
        public event Action<int> OnChangeCurrency ;
        private int _balance;

        public CurrencyManager( int currency) => _balance = currency;
        
        public void Start() => OnChangeCurrency?.Invoke(_balance);

        public void AddCurrency(int amount)
        {
            _balance += amount;
            OnChangeCurrency?.Invoke(_balance);
        }

        public bool SpendCurrency(int amount)
        {
            if (_balance >= amount)
            {
                _balance -= amount;
                OnChangeCurrency?.Invoke(_balance);
                return true;
            }
            return false;
        }

    }
}

