using System;
using UnityEngine;

namespace InventorySystem.Scripts
{
    [Serializable]
    public sealed class InventoryLockModel
    {
        [SerializeField] private int totalSlotCount = 24;
        [SerializeField] private int unlockedSlotCount = 24;

        public int TotalSlotCount => Mathf.Max(1, totalSlotCount);
        public int UnlockedSlotCount => Mathf.Clamp(unlockedSlotCount, 0, TotalSlotCount);

        public event Action<int> LockStateChanged; // index
        public event Action LockModelChanged;

        public void InitializeFromConfig(InventoryConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            totalSlotCount = Mathf.Max(1, config.TotalSlotCount);
            unlockedSlotCount = Mathf.Clamp(config.UnlockedSlotCount, 0, totalSlotCount);
            RaiseAllChanged();
        }

        public void EnsureSize(int total)
        {
            totalSlotCount = Mathf.Max(1, total);
            unlockedSlotCount = Mathf.Clamp(unlockedSlotCount, 0, totalSlotCount);
            RaiseAllChanged();
        }

        public bool IsSlotLocked(int index)
        {
            ValidateIndex(index);
            return index >= UnlockedSlotCount;
        }

        public int GetNextLockedIndex()
        {
            if (UnlockedSlotCount >= TotalSlotCount) return -1;
            return UnlockedSlotCount;
        }

        public bool TryUnlockNext()
        {
            if (UnlockedSlotCount >= TotalSlotCount) return false;
            int unlockedIndex = unlockedSlotCount;
            unlockedSlotCount++;
            LockStateChanged?.Invoke(unlockedIndex);
            LockModelChanged?.Invoke();
            return true;
        }

        public void ForceUnlockAll()
        {
            if (unlockedSlotCount == TotalSlotCount) return;
            unlockedSlotCount = TotalSlotCount;
            RaiseAllChanged();
        }

        public void SetUnlockedSlotCount(int value)
        {
            int clamped = Mathf.Clamp(value, 0, TotalSlotCount);
            if (clamped == unlockedSlotCount) return;

            unlockedSlotCount = clamped;
            RaiseAllChanged();
        }

        private void RaiseAllChanged()
        {
            LockModelChanged?.Invoke();
            for (int i = 0; i < TotalSlotCount; i++)
                LockStateChanged?.Invoke(i);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= TotalSlotCount)
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
