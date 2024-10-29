using System;
using VContainer.Unity;
namespace Game
{
    public sealed class CurrencyManager : IStartable
    {
        private GameLauncher _gameLauncher;
        public event Action<int> OnChangeCurrency;
        private int _balance;

        public CurrencyManager(int currency, GameLauncher gameLauncher)
        {
            _balance = currency;
            _gameLauncher = gameLauncher;
        }

        public void Start() => OnChangeCurrency?.Invoke(_balance);

        //TODO: Вообще это дичь полная, но так как времени нет и в целом уже надо сдать работу оставлю тупо перезапуск игры если проиграл 
        public void AddCurrency(int amount)
        {
            _balance += amount;
            if (_balance < 0)
            {
                _balance = 0;
                _gameLauncher.LaunchMainMenuScreen();
            }
            OnChangeCurrency?.Invoke(_balance);
        }

        //TODO: Вообще это дичь полная, но так как времени нет и в целом уже надо сдать работу оставлю тупо перезапуск игры если проиграл 
        public void SpendCurrency(int amount)
        {
            if (_balance >= amount)
                _balance -= amount;
            else
            {
                _balance = 0;
                _gameLauncher.LaunchMainMenuScreen();
            }

            OnChangeCurrency?.Invoke(_balance);
        }

    }
}