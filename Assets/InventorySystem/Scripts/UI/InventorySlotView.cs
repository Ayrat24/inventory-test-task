using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem.Scripts.UI
{
    public sealed class InventorySlotView : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IPointerClickHandler
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Image highlight;

        [Header("Lock UI")]
        [SerializeField] private GameObject lockGroup;
        [SerializeField] private TMP_Text lockText;

        private InventoryGridView grid;
        private int index;
        private bool isLocked;

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

        public void SetLocked(bool locked)
        {
            isLocked = locked;

            lockGroup.SetActive(locked);

            if (quantityText != null)
                quantityText.alpha = locked ? 0.25f : 1f;

            if (locked)
            {
                Set(default);
                
                int price = Mathf.Max(0, grid.Controller.Config.UnlockPrice);
                lockText.text = $"Locked\n{price} coins";
            }
        }

        public void Set(InventorySlot slot)
        {
            if (isLocked)
            {
                // Locked slots never show contents.
                iconImage.enabled = false;
                iconImage.sprite = null;
                quantityText.text = string.Empty;
                return;
            }

            bool empty = slot.IsEmpty;

            iconImage.enabled = !empty;
            iconImage.sprite = empty ? null : slot.Item.Icon;

            if (empty || slot.Item.MaxStackSize <= 1)
                quantityText.text = string.Empty;
            else
                quantityText.text = slot.Quantity.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(isLocked)
            {
                grid.Controller.TryUnlockNextSlot();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isLocked) return;
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
            // Only the source slot view should restore the icon.
            if (!InventoryDragState.IsDragging) return;
            if (InventoryDragState.FromIndex != index) return;

            // Always restore visuals (whether cancelled or successfully dropped).
            iconRect.SetParent(iconOriginalParent, true);
            iconRect.anchoredPosition = iconOriginalAnchoredPos;
            highlight.enabled = false;

            // If no drop handler ran, treat it as cancel; otherwise the model was already updated.
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
            if (isLocked) return;
            if (!InventoryDragState.IsDragging) return;

            int from = InventoryDragState.FromIndex;
            int to = index;

            // Ignore self-drop.
            if (from == to) return;

            bool splitHalf = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            grid.HandleDrop(from, to, splitHalf);
        }
    }
}
