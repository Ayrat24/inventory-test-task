using InventorySystem.Scripts.UI;
using UnityEngine;

namespace InventorySystem.UI
{
    public sealed class InventoryGridView : MonoBehaviour
    {
        [SerializeField] private InventoryController controller;
        [SerializeField] private InventorySlotView slotViewPrefab;
        [SerializeField] private RectTransform contentRoot;

        private InventorySlotView[] views;

        public InventoryController Controller => controller;

        private void OnEnable()
        {
            controller.Model.SlotChanged += OnSlotChanged;
            controller.Model.InventoryChanged += OnInventoryChanged;

            Rebuild(force: true);
        }

        private void OnDisable()
        {
            controller.Model.SlotChanged -= OnSlotChanged;
            controller.Model.InventoryChanged -= OnInventoryChanged;
        }

        private void OnValidate()
        {
            if (contentRoot == null)
                contentRoot = transform as RectTransform;

            Debug.Assert(controller != null, $"[{nameof(InventoryGridView)}] '{nameof(controller)}' is not assigned.",
                this);
            Debug.Assert(slotViewPrefab != null,
                $"[{nameof(InventoryGridView)}] '{nameof(slotViewPrefab)}' is not assigned.", this);
            Debug.Assert(contentRoot != null, $"[{nameof(InventoryGridView)}] '{nameof(contentRoot)}' is not assigned.",
                this);
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
                views[i] = view;
            }
        }

        public void HandleDrop(int fromIndex, int toIndex, bool splitHalf)
        {
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