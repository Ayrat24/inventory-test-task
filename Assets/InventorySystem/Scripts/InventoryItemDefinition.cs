using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(menuName = "Inventory/Item Definition", fileName = "ItemDefinition")]
    public sealed class InventoryItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;

        [Header("Stacking")]
        [Min(1)]
        [SerializeField] private int maxStackSize = 1;

        public string Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite Icon => icon;
        public int MaxStackSize => Mathf.Max(1, maxStackSize);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = name;

            maxStackSize = Mathf.Max(1, maxStackSize);
        }
#endif
    }
}

