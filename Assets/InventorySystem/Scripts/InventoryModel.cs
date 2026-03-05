using System;
using UnityEngine;

namespace InventorySystem.Scripts
{
    [Serializable]
    public sealed class InventoryModel
    {
        [SerializeField] private int columns = 6;
        [SerializeField] private int rows = 4;
        [SerializeField] private InventorySlot[] slots;

        public int Columns => Mathf.Max(1, columns);
        public int Rows => Mathf.Max(1, rows);
        public int SlotCount => Columns * Rows;

        public event Action<int> SlotChanged;
        public event Action InventoryChanged;

        public void Initialize(int cols, int rws)
        {
            columns = Mathf.Max(1, cols);
            rows = Mathf.Max(1, rws);
            slots = new InventorySlot[SlotCount];
            RaiseAllChanged();
        }

        public void EnsureInitialized()
        {
            if (slots == null || slots.Length != SlotCount)
            {
                var old = slots;
                slots = new InventorySlot[SlotCount];

                if (old != null)
                {
                    for (int i = 0; i < Mathf.Min(old.Length, slots.Length); i++)
                        slots[i] = old[i];
                }

                RaiseAllChanged();
            }
        }

        public InventorySlot GetSlot(int index)
        {
            EnsureInitialized();
            ValidateIndex(index);
            return slots[index];
        }

        public void SetSlot(int index, InventorySlot value)
        {
            EnsureInitialized();
            ValidateIndex(index);
            slots[index] = value;
            RaiseSlotChanged(index);
        }

        /// <summary>
        /// Empties the slot at <paramref name="index"/>. Returns true if a non-empty slot was cleared.
        /// </summary>
        public bool RemoveAtSlot(int index)
        {
            EnsureInitialized();
            ValidateIndex(index);

            var s = slots[index];
            if (s.IsEmpty)
                return false;

            s.Clear();
            slots[index] = s;

            RaiseSlotChanged(index);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool CanStack(InventoryItemDefinition a, InventoryItemDefinition b)
        {
            return a != null && a == b;
        }

        /// <summary>
        /// Adds up to <paramref name="amount"/> of <paramref name="item"/>. Returns the amount actually added.
        /// </summary>
        public int Add(InventoryItemDefinition item, int amount)
        {
            EnsureInitialized();
            if (item == null) return 0;
            if (amount <= 0) return 0;

            int remaining = amount;

            // 1) Fill existing stacks.
            if (item.MaxStackSize > 1)
            {
                for (int i = 0; i < slots.Length && remaining > 0; i++)
                {
                    if (slots[i].IsEmpty) continue;
                    if (!CanStack(slots[i].Item, item)) continue;

                    int space = slots[i].SpaceLeft();
                    if (space <= 0) continue;

                    int toAdd = Math.Min(space, remaining);
                    var s = slots[i];
                    s.Quantity += toAdd;
                    slots[i] = s;
                    remaining -= toAdd;

                    Debug.Log($"Add: '{item.DisplayName}' slot {i} +{toAdd} (now {s.Quantity}).");
                    RaiseSlotChanged(i);
                }
            }

            // 2) Put into empty slots.
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty) continue;

                int toAdd = Math.Min(item.MaxStackSize, remaining);
                slots[i] = new InventorySlot { Item = item, Quantity = toAdd };
                remaining -= toAdd;

                Debug.Log($"Add: '{item.DisplayName}' +{toAdd} slot={i} (now {toAdd}).");
                RaiseSlotChanged(i);
            }

            int added = amount - remaining;

            if (added > 0)
                InventoryChanged?.Invoke();

            if (added < amount)
                Debug.LogError($"Add failed: '{item.DisplayName}' requested {amount}, added {added}.");

            return added;
        }

        /// <summary>
        /// Move/stack/swap from one slot to another.
        /// </summary>
        public void Move(int fromIndex, int toIndex)
        {
            EnsureInitialized();
            if (fromIndex == toIndex) return;
            ValidateIndex(fromIndex);
            ValidateIndex(toIndex);

            var from = slots[fromIndex];
            var to = slots[toIndex];

            if (from.IsEmpty) return;

            // Stack if possible
            if (!to.IsEmpty && CanStack(from.Item, to.Item) && to.Quantity < to.MaxStackSize)
            {
                int space = to.SpaceLeft();
                int moved = Math.Min(space, from.Quantity);

                to.Quantity += moved;
                from.Quantity -= moved;

                if (from.Quantity <= 0) from.Clear();

                slots[toIndex] = to;
                slots[fromIndex] = from;

                RaiseSlotChanged(fromIndex);
                RaiseSlotChanged(toIndex);
                InventoryChanged?.Invoke();
                return;
            }

            // Otherwise swap
            slots[toIndex] = from;
            slots[fromIndex] = to;
            RaiseSlotChanged(fromIndex);
            RaiseSlotChanged(toIndex);
            InventoryChanged?.Invoke();
        }

        /// <summary>
        /// Splits a stack into target slot (empty or stackable). Returns how many were moved.
        /// </summary>
        public int Split(int fromIndex, int toIndex, int amount)
        {
            EnsureInitialized();
            if (amount <= 0) return 0;
            if (fromIndex == toIndex) return 0;
            ValidateIndex(fromIndex);
            ValidateIndex(toIndex);

            var from = slots[fromIndex];
            var to = slots[toIndex];
            if (from.IsEmpty) return 0;
            if (from.Quantity <= 1) return 0;

            amount = Math.Min(amount, from.Quantity);

            if (to.IsEmpty)
            {
                int toMove = Math.Min(amount, from.Item.MaxStackSize);
                from.Quantity -= toMove;
                slots[fromIndex] = from;

                slots[toIndex] = new InventorySlot { Item = from.Item, Quantity = toMove };
                RaiseSlotChanged(fromIndex);
                RaiseSlotChanged(toIndex);
                InventoryChanged?.Invoke();
                return toMove;
            }

            if (CanStack(from.Item, to.Item))
            {
                int space = to.SpaceLeft();
                if (space <= 0) return 0;

                int toMove = Math.Min(space, amount);
                to.Quantity += toMove;
                from.Quantity -= toMove;
                if (from.Quantity <= 0) from.Clear();

                slots[fromIndex] = from;
                slots[toIndex] = to;
                RaiseSlotChanged(fromIndex);
                RaiseSlotChanged(toIndex);
                InventoryChanged?.Invoke();
                return toMove;
            }

            return 0;
        }

        /// <summary>
        /// Tries to consume (remove) up to <paramref name="amount"/> of <paramref name="item"/>.
        /// Returns true if the full amount was consumed.
        /// </summary>
        public bool TryConsume(InventoryItemDefinition item, int amount)
        {
            EnsureInitialized();
            if (item == null) return false;
            if (amount <= 0) return true;

            // First pass: count total available.
            int available = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) continue;
                if (!CanStack(slots[i].Item, item)) continue;
                available += slots[i].Quantity;
                if (available >= amount) break;
            }

            if (available < amount)
                return false;

            // Second pass: actually consume.
            int remaining = amount;
            bool anyChanged = false;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                var s = slots[i];
                if (s.IsEmpty) continue;
                if (!CanStack(s.Item, item)) continue;

                int toRemove = Math.Min(s.Quantity, remaining);
                s.Quantity -= toRemove;
                remaining -= toRemove;

                if (s.Quantity <= 0)
                    s.Clear();

                slots[i] = s;
                RaiseSlotChanged(i);
                anyChanged = true;
            }

            if (anyChanged)
                InventoryChanged?.Invoke();

            return true;
        }

        private void RaiseAllChanged()
        {
            InventoryChanged?.Invoke();
            if (slots == null) return;
            for (int i = 0; i < slots.Length; i++)
                SlotChanged?.Invoke(i);
        }

        private void RaiseSlotChanged(int index)
        {
            SlotChanged?.Invoke(index);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= SlotCount)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
