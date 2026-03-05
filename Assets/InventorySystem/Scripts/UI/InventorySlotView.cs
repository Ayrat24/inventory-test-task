using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem.UI
{
    public sealed class InventorySlotView : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Image highlight;

        private InventoryGridView grid;
        private int index;

        private Canvas rootCanvas;
        private RectTransform iconRect;
        private Transform iconOriginalParent;
        private Vector2 iconOriginalAnchoredPos;

        public int Index => index;

        private void OnValidate()
        {
            Debug.Assert(iconImage != null, $"[{nameof(InventorySlotView)}] '{nameof(iconImage)}' is not assigned.", this);
            Debug.Assert(quantityText != null, $"[{nameof(InventorySlotView)}] '{nameof(quantityText)}' is not assigned.", this);
            Debug.Assert(highlight != null, $"[{nameof(InventorySlotView)}] '{nameof(highlight)}' is not assigned.", this);
        }

        public void Initialize(InventoryGridView owner, int slotIndex)
        {
            grid = owner;
            index = slotIndex;

            rootCanvas = GetComponentInParent<Canvas>();
            Debug.Assert(rootCanvas != null, $"[{nameof(InventorySlotView)}] No parent Canvas found.", this);

            iconRect = iconImage.rectTransform;
            highlight.enabled = false;
        }

        public void Set(InventorySlot slot)
        {
            bool empty = slot.IsEmpty;

            iconImage.enabled = !empty;
            iconImage.sprite = empty ? null : slot.Item.Icon;

            if (empty || slot.Item.MaxStackSize <= 1)
                quantityText.text = string.Empty;
            else
                quantityText.text = slot.Quantity.ToString();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (grid == null) return;

            var slot = grid.Controller.Model.GetSlot(index);
            if (slot.IsEmpty) return;

            iconOriginalParent = iconRect.parent;
            iconOriginalAnchoredPos = iconRect.anchoredPosition;
            iconRect.SetParent(rootCanvas.transform, true);

            highlight.enabled = true;
            InventoryDragState.BeginDrag(grid, index);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!InventoryDragState.IsDragging) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)rootCanvas.transform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPos);

            iconRect.anchoredPosition = localPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!InventoryDragState.IsDragging) return;

            iconRect.SetParent(iconOriginalParent, true);
            iconRect.anchoredPosition = iconOriginalAnchoredPos;

            highlight.enabled = false;
            InventoryDragState.EndDrag();
        }

        private void OnDisable()
        {
            if (InventoryDragState.IsDragging && InventoryDragState.FromIndex == index)
                InventoryDragState.EndDrag();

            highlight.enabled = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (grid == null) return;
            if (!InventoryDragState.IsDragging) return;

            int from = InventoryDragState.FromIndex;
            int to = index;

            bool splitHalf = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            grid.HandleDrop(from, to, splitHalf);
        }
    }
}
