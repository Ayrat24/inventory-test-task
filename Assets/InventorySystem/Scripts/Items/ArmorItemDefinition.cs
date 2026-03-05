using System;
using UnityEngine;

namespace InventorySystem.Scripts.Items
{
    [CreateAssetMenu(menuName = "Inventory/Armor Definition", fileName = "Armor")]
    public class ArmorItemDefinition : InventoryItemDefinition
    {
        [SerializeField] private BodyPart equipBodyPart;
        [SerializeField] private DefenceStats[] defenceStats;

        [Serializable]
        private class DefenceStats
        {
            public BodyPart bodyPart;
            public int defence;
        }

        private enum BodyPart
        {
            Head,
            Chest
        }
    }
}