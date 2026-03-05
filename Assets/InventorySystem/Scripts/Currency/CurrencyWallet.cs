using System;

namespace InventorySystem.Scripts.Currency
{
    public sealed class CurrencyWallet
    {
        private static readonly CurrencyWallet InstanceField = new CurrencyWallet();
        public static CurrencyWallet Instance => InstanceField;

        private int amount;

        public int Amount => amount;

        public event Action<int> AmountChanged;

        private CurrencyWallet() { }

        public void Set(int newAmount)
        {
            if (newAmount < 0) newAmount = 0;
            if (newAmount == amount) return;

            amount = newAmount;
            AmountChanged?.Invoke(amount);
        }

        public int Add(int value)
        {
            if (value <= 0) return 0;

            int old = amount;
            try
            {
                checked { amount += value; }
            }
            catch (OverflowException)
            {
                amount = int.MaxValue;
            }

            int delta = amount - old;
            if (delta != 0)
                AmountChanged?.Invoke(amount);

            return delta;
        }

        public bool CanSpend(int value)
        {
            if (value <= 0) return true;
            return amount >= value;
        }

        public bool TrySpend(int value)
        {
            if (value <= 0) return true;
            if (amount < value) return false;

            amount -= value;
            AmountChanged?.Invoke(amount);
            return true;
        }
    }
}
