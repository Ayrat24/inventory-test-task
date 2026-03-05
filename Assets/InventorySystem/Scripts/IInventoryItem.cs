using InventorySystem.Scripts.Items;

namespace InventorySystem
{
    /// <summary>
    /// Optional extensibility point. The current system uses <see cref="InventoryItemDefinition"/>
    /// as the item identity, but you can implement this interface for richer runtime item instances.
    /// </summary>
    public interface IInventoryItem
    {
        InventoryItemDefinition Definition { get; }
    }
}
