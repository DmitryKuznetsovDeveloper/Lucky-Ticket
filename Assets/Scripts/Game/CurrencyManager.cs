
namespace Game
{
    public sealed class CurrencyManager
    {
        public int Balance { get; private set; }

        public CurrencyManager(int initialBalance) => Balance = initialBalance;

        public void AddCurrency(int amount) => Balance += amount;
    
        public bool SpendCurrency(int amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }
    }
}

