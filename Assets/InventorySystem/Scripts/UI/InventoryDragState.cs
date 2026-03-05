namespace InventorySystem.UI
{
    public static class InventoryDragState
    {
        public static bool IsDragging { get; private set; }
        public static int FromIndex { get; private set; }
        public static InventoryGridView Grid { get; private set; }

        // True when an OnDrop handler successfully processed the current drag.
        public static bool DropHandled { get; set; }

        public static void BeginDrag(InventoryGridView grid, int fromIndex)
        {
            Grid = grid;
            FromIndex = fromIndex;
            DropHandled = false;
            IsDragging = true;
        }

        public static void EndDrag()
        {
            Grid = null;
            FromIndex = -1;
            DropHandled = false;
            IsDragging = false;
        }
    }
}
