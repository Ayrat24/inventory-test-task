namespace InventorySystem.UI
{
    public static class InventoryDragState
    {
        public static bool IsDragging { get; private set; }
        public static int FromIndex { get; private set; }
        public static InventoryGridView Grid { get; private set; }

        public static void BeginDrag(InventoryGridView grid, int fromIndex)
        {
            Grid = grid;
            FromIndex = fromIndex;
            IsDragging = true;
        }

        public static void EndDrag()
        {
            Grid = null;
            FromIndex = -1;
            IsDragging = false;
        }
    }
}

