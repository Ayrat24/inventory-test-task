using UnityEngine;

namespace InventorySystem.Scripts.Items
{
    [CreateAssetMenu(menuName = "Inventory/Item Definition", fileName = "ItemDefinition")]
    public class InventoryItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Saving")]
        [Tooltip("Auto-filled (Editor) when this asset is placed under any Resources folder. Used for save/load.")]
        [SerializeField] private string resourcesPath;

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
        public string ResourcesPath => resourcesPath;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = name;

            // Auto-set ResourcesPath when asset is inside a Resources folder.
            // Example: Assets/Resources/Items/Potion.asset => Items/Potion
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            resourcesPath = TryGetResourcesPath(assetPath) ?? resourcesPath;

            maxStackSize = Mathf.Max(1, maxStackSize);
            weight = Mathf.Max(0f, weight);
        }

        private static string TryGetResourcesPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            const string marker = "/Resources/";
            int idx = assetPath.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return null;

            string after = assetPath[(idx + marker.Length)..];
            int dot = after.LastIndexOf('.');
            if (dot >= 0)
                after = after[..dot];

            return string.IsNullOrWhiteSpace(after) ? null : after;
        }
#endif
    }
}
