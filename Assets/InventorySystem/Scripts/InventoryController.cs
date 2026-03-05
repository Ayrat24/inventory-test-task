using InventorySystem.Scripts.Currency;
using InventorySystem.Scripts.Items;
using UnityEngine;

namespace InventorySystem.Scripts
{
    public sealed class InventoryController : MonoBehaviour
    {
        [SerializeField] private InventoryConfig config;

        private readonly InventoryModel model = new InventoryModel();
        private readonly InventoryLockModel locks = new InventoryLockModel();

        public InventoryModel Model => model;
        public InventoryLockModel Locks => locks;
        public InventoryConfig Config => config;

        private void OnValidate()
        {
            Debug.Assert(config != null, $"[{nameof(InventoryController)}] '{nameof(config)}' is not assigned.", this);
        }

        private void Awake()
        {
            int total = Mathf.Max(1, config.TotalSlotCount);
            model.Initialize(total, 1);

            locks.InitializeFromConfig(config);
            locks.EnsureSize(model.SlotCount);
        }

        public bool IsSlotLocked(int index)
        {
            if (locks.TotalSlotCount != model.SlotCount)
                locks.EnsureSize(model.SlotCount);

            return locks.IsSlotLocked(index);
        }

        public bool TryUnlockNextSlot()
        {
            int nextLocked = locks.GetNextLockedIndex();
            if (nextLocked < 0) return false; // none locked

            int price = Mathf.Max(0, config.UnlockPrice);
            if (!CurrencyWallet.Instance.TrySpend(price))
                return false;

            return locks.TryUnlockNext();
        }

        public int Add(InventoryItemDefinition item, int amount)
        {
            // Only allow adding into unlocked slots.
            return model.Add(item, amount, isSlotAvailable: i => !IsSlotLocked(i));
        }

        public bool TryConsume(InventoryItemDefinition item, int amount) => model.TryConsume(item, amount);

        public void Move(int fromIndex, int toIndex)
        {
            if (IsSlotLocked(fromIndex) || IsSlotLocked(toIndex))
                return;
            model.Move(fromIndex, toIndex);
        }

        public int Split(int fromIndex, int toIndex, int amount)
        {
            if (IsSlotLocked(fromIndex) || IsSlotLocked(toIndex))
                return 0;
            return model.Split(fromIndex, toIndex, amount);
        }

        public InventorySlot GetSlot(int index) => model.GetSlot(index);
    }
}
