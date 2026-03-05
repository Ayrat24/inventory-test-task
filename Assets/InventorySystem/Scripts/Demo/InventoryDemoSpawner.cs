using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace InventorySystem.Scripts.Demo
{
    public sealed class InventoryDemoSpawner : MonoBehaviour
    {
        [SerializeField] private InventoryController inventory;

        [SerializeField] private InventoryItemDefinition[] itemPool;
        [SerializeField] private InventoryItemDefinition[] itemPoolAmmo;

        [SerializeField] private Button spawnButton;
        [SerializeField] private Button ammoButton;
        [SerializeField] private Button fireButton;
        [SerializeField] private Button removeButton;


        private void OnValidate()
        {
            Debug.Assert(inventory != null, $"'{nameof(inventory)}' is not assigned.");
            Debug.Assert(itemPool is { Length: > 0 }, $"'{nameof(itemPool)}' is empty.");
        }

        private void Awake()
        {
            ammoButton.onClick.RemoveListener(AddAmmo);
            ammoButton.onClick.AddListener(AddAmmo);

            spawnButton.onClick.RemoveListener(SpawnRandom);
            spawnButton.onClick.AddListener(SpawnRandom);

            fireButton.onClick.RemoveListener(FireGun);
            fireButton.onClick.AddListener(FireGun);

            removeButton.onClick.RemoveListener(RemoveRandom);
            removeButton.onClick.AddListener(RemoveRandom);
        }

        private void FireGun()
        {
            var model = inventory.Model;

            // Collect gun slot indices.
            int slotCount = model.SlotCount;
            int[] gunSlots = new int[slotCount];
            int gunCount = 0;

            for (int i = 0; i < slotCount; i++)
            {
                var slot = model.GetSlot(i);
                if (slot.IsEmpty) continue;

                if (slot.Item is GunItemDefinition)
                    gunSlots[gunCount++] = i;
            }

            if (gunCount == 0)
            {
                Debug.LogError("Fire failed: no gun in inventory.");
                return;
            }

            int chosenSlotIndex = gunSlots[Random.Range(0, gunCount)];
            var chosenSlot = model.GetSlot(chosenSlotIndex);
            var gun = (GunItemDefinition)chosenSlot.Item;

            var ammo = gun.Ammo;
            bool consumed = inventory.TryConsume(ammo, 1);

            if (consumed)
            {
                Debug.Log(
                    $"Fired '{gun.DisplayName}' (slot {chosenSlotIndex}). Consumed 1 '{ammo.DisplayName}'. Dealt {gun.Damage} damage.");
            }
            else
            {
                Debug.LogError($"Click! '{gun.DisplayName}' (slot {chosenSlotIndex}) is out of '{ammo.DisplayName}'.");
            }
        }

        private void AddAmmo()
        {
            const int ammoAmount = 30;
            foreach (var ammo in itemPoolAmmo)
            {
                inventory.Add(ammo, ammoAmount);
            }
        }

        private void SpawnRandom()
        {
            var item = itemPool[Random.Range(0, itemPool.Length)];
            inventory.Add(item, 1);
        }

        private void RemoveRandom()
        {
            var model = inventory.Model;

            int slotCount = model.SlotCount;
            int[] filledSlots = new int[slotCount];
            int filledCount = 0;

            for (int i = 0; i < slotCount; i++)
            {
                if (!model.GetSlot(i).IsEmpty)
                    filledSlots[filledCount++] = i;
            }

            if (filledCount == 0)
            {
                Debug.LogError("RemoveRandom: inventory is already empty.");
                return;
            }

            int chosenSlotIndex = filledSlots[Random.Range(0, filledCount)];
            bool removed = model.RemoveAtSlot(chosenSlotIndex);

            if (removed)
                Debug.Log($"RemoveRandom: cleared slot {chosenSlotIndex}.");
        }

        private void OnDestroy()
        {
            ammoButton.onClick.RemoveListener(AddAmmo);
            spawnButton.onClick.RemoveListener(SpawnRandom);
            fireButton.onClick.RemoveListener(FireGun);
            removeButton.onClick.RemoveListener(RemoveRandom);
        }
    }
}