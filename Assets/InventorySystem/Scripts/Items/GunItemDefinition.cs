using UnityEngine;

namespace InventorySystem.Scripts.Items
{
    [CreateAssetMenu(menuName = "Inventory/Gun Definition", fileName = "Gun")]
    public class GunItemDefinition : InventoryItemDefinition
    {
        [SerializeField] private InventoryItemDefinition ammo;
        [SerializeField] private int damage = 10;
        
        public InventoryItemDefinition Ammo => ammo;
        public int Damage => damage;
    }
}
