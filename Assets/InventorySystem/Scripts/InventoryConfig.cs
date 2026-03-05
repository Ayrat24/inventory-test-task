using UnityEngine;

namespace InventorySystem.Scripts
{
    [CreateAssetMenu(menuName = "Inventory/Inventory Config", fileName = "InventoryConfig")]
    public sealed class InventoryConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int totalSlotCount = 24;
        [SerializeField, Min(0)] private int lockedSlotCount;
        [SerializeField, Min(0)] private int unlockPrice = 50;

        public int TotalSlotCount => totalSlotCount;
        public int LockedSlotCount => Mathf.Clamp(lockedSlotCount, 0, totalSlotCount);
        public int UnlockPrice => unlockPrice;

        public int UnlockedSlotCount => Mathf.Max(0, TotalSlotCount - LockedSlotCount);

        private void OnValidate()
        {
            totalSlotCount = Mathf.Max(1, totalSlotCount);
            lockedSlotCount = Mathf.Clamp(lockedSlotCount, 0, totalSlotCount);
            unlockPrice = Mathf.Max(0, unlockPrice);
        }
    }
}
