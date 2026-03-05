using UnityEngine;

namespace InventorySystem
{
    public sealed class InventoryController : MonoBehaviour
    {
        [Header("Size")]
        [SerializeField] private int columns = 6;
        [SerializeField] private int rows = 4;

        [Header("Data")]
        [SerializeField] private InventoryModel model = new InventoryModel();

        public InventoryModel Model => model;

        private void Awake()
        {
            model.Initialize(columns, rows);
        }

        public int Add(InventoryItemDefinition item, int amount) => model.Add(item, amount);
        public void Move(int fromIndex, int toIndex) => model.Move(fromIndex, toIndex);
        public int Split(int fromIndex, int toIndex, int amount) => model.Split(fromIndex, toIndex, amount);
        public InventorySlot GetSlot(int index) => model.GetSlot(index);
    }
}

