using System;
using System.Collections.Generic;
using System.IO;
using InventorySystem.Scripts.Currency;
using UnityEngine;

namespace InventorySystem.Scripts
{
    public sealed class SaveSystem : MonoBehaviour
    {
        [SerializeField] private InventoryController inventory;

        [Serializable]
        private sealed class SaveData
        {
            public int coins;
            public int unlockedSlots;
            public List<Slot> slots = new List<Slot>();

            [Serializable]
            public sealed class Slot
            {
                public int index;
                public string itemId;
                public int quantity;
            }
        }

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private CurrencyWallet Wallet => CurrencyWallet.Instance;

        private void Start()
        {
            Load();
        }

        private static InventoryItemDefinition LoadItem(string itemId)
        {
            return Resources.Load<InventoryItemDefinition>(itemId);
        }

        private void Load()
        {
            if (!File.Exists(SavePath)) return;

            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            Wallet.Set(data.coins);

            inventory.Locks.EnsureSize(inventory.Model.SlotCount);
            inventory.Locks.SetUnlockedSlotCount(data.unlockedSlots);

            int slotCount = inventory.Model.SlotCount;
            for (int i = 0; i < slotCount; i++)
                inventory.Model.SetSlot(i, default);

            foreach (var s in data.slots)
            {
                if ((uint)s.index >= (uint)slotCount) continue;
                if (s.quantity <= 0) continue;

                var def = LoadItem(s.itemId);
                int finalQty = Mathf.Min(def.MaxStackSize, s.quantity);
                inventory.Model.SetSlot(s.index, new InventorySlot { Item = def, Quantity = finalQty });
            }

            inventory.Model.NotifyChanged();
        }

        private void Save()
        {
            inventory.Model.EnsureInitialized();

            var data = new SaveData
            {
                coins = Wallet.Amount,
                unlockedSlots = inventory.Locks.UnlockedSlotCount,
            };

            int slotCount = inventory.Model.SlotCount;
            for (int i = 0; i < slotCount; i++)
            {
                var slot = inventory.Model.GetSlot(i);
                if (slot.IsEmpty) continue;

                var path = slot.Item.ResourcesPath;
                if (string.IsNullOrWhiteSpace(path))
                    path = slot.Item.Id;

                data.slots.Add(new SaveData.Slot
                {
                    index = i,
                    itemId = path,
                    quantity = slot.Quantity
                });
            }

            var json = JsonUtility.ToJson(data);
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(SavePath, json);
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}
