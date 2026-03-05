using TMPro;
using UnityEngine;

namespace InventorySystem.Scripts.Currency
{
    public class CurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountText;

        private CurrencyWallet Wallet => CurrencyWallet.Instance;

        private void OnValidate()
        {
            UpdateView();
        }

        private void Awake()
        {
            Wallet.AmountChanged += OnAmountChanged;
            UpdateView();
        }

        private void OnDestroy()
        {
            Wallet.AmountChanged -= OnAmountChanged;
        }

        private void OnAmountChanged(int newAmount)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            amountText.text = $"Coins: {Wallet.Amount}";
        }
    }
}