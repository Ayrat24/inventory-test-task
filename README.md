# inventory-test-task

## Main scene

Main scene at:

- `Assets/InventorySystem/Scenes/TestInventory.unity`


## Configs

### Inventory config

- `Assets/InventorySystem/Config/InventoryConfig.asset`

This controls things like total slot count, how many start locked, and unlock price.

### Items

Item `ScriptableObject` assets live under:

- `Assets/InventorySystem/Config/Resources/`
  - `Ammo/`
  - `Guns/`
  - `Armor/`

The save system loads items via `Resources.Load(...)`, so item assets must remain under a `Resources` folder.

## Features

- **Drag & drop** inventory items between slots.
- **Slot swapping** when dropping onto an occupied slot.
- **Stack splitting**: hold **Shift** while dropping to split half a stack.
- **Ammo stack replenishment**: the demo UI can add ammo in bulk (replenishes existing stacks first).
- **Wallet / coins**: supports earning coins and spending them to unlock slots.
- **Save/Load**: inventory + wallet are saved to a JSON file on app exit and loaded on app start.
  - Save file path: `Application.persistentDataPath/save.json`
