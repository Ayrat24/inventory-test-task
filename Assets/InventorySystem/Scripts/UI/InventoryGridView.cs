using UnityEngine;

namespace InventorySystem.Scripts.UI
{
    public sealed class InventoryGridView : MonoBehaviour
    {
        [SerializeField] private InventoryController controller;
        [SerializeField] private InventorySlotView slotViewPrefab;
        [SerializeField] private RectTransform contentRoot;

        private InventorySlotView[] views;
        private bool isRebuilding;

        public InventoryController Controller => controller;

        private void OnEnable()
        {
            controller.Model.SlotChanged += OnSlotChanged;
            controller.Model.InventoryChanged += OnInventoryChanged;

            controller.Locks.LockStateChanged += OnLockStateChanged;
            controller.Locks.LockModelChanged += OnLockModelChanged;

            Rebuild(force: true);
        }

        private void OnDisable()
        {
            controller.Model.SlotChanged -= OnSlotChanged;
            controller.Model.InventoryChanged -= OnInventoryChanged;

            controller.Locks.LockStateChanged -= OnLockStateChanged;
            controller.Locks.LockModelChanged -= OnLockModelChanged;
        }

        private void OnValidate()
        {
            Debug.Assert(controller != null, $"[{nameof(InventoryGridView)}] '{nameof(controller)}' is not assigned.",
                this);
            Debug.Assert(slotViewPrefab != null,
                $"[{nameof(InventoryGridView)}] '{nameof(slotViewPrefab)}' is not assigned.", this);
            Debug.Assert(contentRoot != null, $"[{nameof(InventoryGridView)}] '{nameof(contentRoot)}' is not assigned.",
                this);
        }

        private void OnLockModelChanged()
        {
            if (isRebuilding) return;
            // Slot count might have changed or forced refresh is needed.
            RefreshLockVisuals();
        }

        private void OnLockStateChanged(int index)
        {
            if (isRebuilding) return;
            RefreshLockVisual(index);
        }

        private void RefreshLockVisuals()
        {
            if (views == null) return;
            for (int i = 0; i < views.Length; i++)
                RefreshLockVisual(i);
        }

        private void RefreshLockVisual(int index)
        {
            if (views == null) return;
            if ((uint)index >= (uint)views.Length) return;

            var view = views[index];
            if (view == null) return;

            view.SetLocked(controller != null && controller.IsSlotLocked(index));
        }

        private void OnSlotChanged(int index)
        {
            if (views == null) return;
            if ((uint)index >= (uint)views.Length) return;

            views[index].Set(controller.Model.GetSlot(index));
        }

        private void OnInventoryChanged()
        {
            int count = controller.Model.SlotCount;
            if (views == null || views.Length != count)
                Rebuild(force: true);
        }

        private void Rebuild(bool force)
        {
            if (controller == null || controller.Model == null)
                return;

            isRebuilding = true;
            try
            {
                controller.Model.EnsureInitialized();
                int count = controller.Model.SlotCount;

                if (!force && views != null && views.Length == count)
                    return;

                for (int i = contentRoot.childCount - 1; i >= 0; i--)
                    Destroy(contentRoot.GetChild(i).gameObject);

                views = new InventorySlotView[count];
                for (int i = 0; i < count; i++)
                {
                    var view = Instantiate(slotViewPrefab, contentRoot);

                    view.Initialize(this, i);
                    view.Set(controller.Model.GetSlot(i));
                    view.SetLocked(controller.IsSlotLocked(i));
                    views[i] = view;
                }
            }
            finally
            {
                isRebuilding = false;
            }
        }

        public void HandleDrop(int fromIndex, int toIndex, bool splitHalf)
        {
            if (controller.IsSlotLocked(fromIndex) || controller.IsSlotLocked(toIndex))
                return;

            if (!splitHalf)
            {
                controller.Move(fromIndex, toIndex);
                return;
            }

            var from = controller.Model.GetSlot(fromIndex);
            if (from.IsEmpty) return;

            int amount = Mathf.Max(1, from.Quantity / 2);
            controller.Split(fromIndex, toIndex, amount);
        }
    }
}