using System;
using InventorySystem.Scripts.Items;

namespace InventorySystem
{
    [Serializable]
    public struct InventorySlot
    {
        public InventoryItemDefinition Item;
        public int Quantity;

        public bool IsEmpty => Item == null || Quantity <= 0;

        public int MaxStackSize => Item == null ? 0 : Item.MaxStackSize;

        public void Clear()
        {
            Item = null;
            Quantity = 0;
        }

        public int SpaceLeft()
        {
            if (Item == null) return 0;
            return Math.Max(0, Item.MaxStackSize - Quantity);
        }

        public override string ToString()
        {
            return IsEmpty ? "(Empty)" : $"{Item.DisplayName} x{Quantity}";
        }
    }
}
