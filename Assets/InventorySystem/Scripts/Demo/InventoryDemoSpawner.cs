using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.Scripts.Demo
{
    public sealed class InventoryDemoSpawner : MonoBehaviour
    {
        [SerializeField] private InventoryController inventory;

        [SerializeField] private InventoryItemDefinition[] itemPool;

        [Min(1)] [SerializeField] private int minAmount = 1;

        [Min(1)] [SerializeField] private int maxAmount = 1;

        [SerializeField] private Button spawnButton;


        private void OnValidate()
        {
            Debug.Assert(inventory != null, $"[{nameof(InventoryDemoSpawner)}] '{nameof(inventory)}' is not assigned.",
                this);
            Debug.Assert(itemPool != null && itemPool.Length > 0,
                $"[{nameof(InventoryDemoSpawner)}] '{nameof(itemPool)}' is empty.", this);

            if (maxAmount < minAmount)
                maxAmount = minAmount;
        }

        private void Awake()
        {
            if (spawnButton != null)
            {
                spawnButton.onClick.RemoveListener(SpawnRandom);
                spawnButton.onClick.AddListener(SpawnRandom);
            }
        }

        private void SpawnRandom()
        {
            var item = itemPool[Random.Range(0, itemPool.Length)];
            int amount = Random.Range(minAmount, maxAmount + 1);

            int added = inventory.Add(item, amount);

            Debug.Log($"[{nameof(InventoryDemoSpawner)}] Spawned {item.DisplayName} x{amount}. Added: {added}.", this);
        }
    }
}