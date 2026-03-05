using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(menuName = "Inventory/Item Definition", fileName = "ItemDefinition")]
    public class InventoryItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;

        [Header("Stacking")]
        [Min(1)]
        [SerializeField] private int maxStackSize = 1;

        [Header("Stats")]
        [Min(0f)]
        [SerializeField] private float weight = 0.01f;

        public string Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite Icon => icon;
        public int MaxStackSize => Mathf.Max(1, maxStackSize);
        public float Weight => Mathf.Max(0f, weight);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = name;

            maxStackSize = Mathf.Max(1, maxStackSize);
            weight = Mathf.Max(0f, weight);
        }
#endif
    }
}
