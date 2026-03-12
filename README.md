# MabWorld

A 2D RPG inspired by Mabinogi, built with **Unity 2022 LTS** and the **Universal Render Pipeline**. MabWorld is a single-player game featuring life skills, crafting, quests, and a cultivation-based character progression system.

## Features

- **Life Skills** — Mining, Botany, Sheep Shearing, Woodcutting, and more
- **Crafting System** — Recipe-based crafting at stations (smelting, weaving, blacksmithing) with skill-dependent success rates
- **Skill Training** — Rank-based progression (F through A) with training methods and XP tracking
- **Quest System** — Multi-condition quests with prerequisites, steps, and rewards
- **Inventory** — Grid-based inventory with drag-and-drop, stack splitting, and item tooltips
- **Equipment** — Equippable gear that modifies character stats
- **Cultivation Stages** — Character progression through cultivation stages and substages
- **Day/Night Cycle** — Dynamic global lighting with dawn, day, dusk, and night phases
- **NPC Dialogue** — Dialogue system with quest-linked conversations
- **Resource Gathering** — Harvestable world resources (rocks, herbs, trees, ore)
- **Save/Load** — Full game state serialization via JSON

## Requirements

- **Unity 2022 LTS** (URP 14.0.10)
- **Windows** (primary target platform)

## Getting Started

1. Clone the repository
   ```
   git clone https://github.com/<your-username>/MabWorld.git
   ```
2. Open the project in Unity 2022 LTS
3. Open `Assets/Scenes/Base.unity` as the starting scene
4. Press Play — the game loads into the main menu, then transitions to the Homestead scene

## Project Structure

```
Assets/
  Scripts/
    Core/           GameManager, DatabaseManager, InputController, StateMachine, etc.
    Database/        Model base class, ItemModel, SkillModel, NPCModel, QuestModel, etc.
    Item/            Item, InventoryManager, InventoryBag, LootGenerator
    Skill/           Skill, SkillManager, training and ranking systems
    Crafting/        CraftingStation, CraftingRecipe, crafting UI
    Quest/           Quest, QuestStateMachine, quest UI
  Scenes/            Base.unity (persistent), Homestead.unity, Fuyang Town Outskirts.unity
  StreamingAssets/   mabinogi.db (SQLite game database)
  Prefabs/           Reusable game object prefabs
  Sprites/           2D sprite assets
  Tiles/             Tileset assets
  Plugins/           SharpUI, YamlDotNet
  NavMeshComponents/ NavMeshPlus for 2D pathfinding
  Addressables/      Addressable asset references
```

## Architecture

- **Singletons** — GameManager, Player, InputController, AudioController (DontDestroyOnLoad)
- **State Machines** — Game states, player actions, movement, mob AI, quest progression
- **Event System** — Typed event managers (IntManager, FloatManager, etc.) for reactive UI updates
- **Data Layer** — SQLite database with a reflection-based ORM (Model base class + ModelFieldReference)
- **Asset Loading** — Addressables for dynamic sprite and audio loading
- **Scene Management** — Persistent base scene with additive content scenes

## Key Dependencies

| Package | Version |
|---------|---------|
| Universal Render Pipeline | 14.0.10 |
| Addressables | 1.21.20 |
| Newtonsoft Json | 3.2.1 |
| TextMeshPro | 3.0.6 |
| AI Navigation | 1.1.5 |
| NavMeshPlus | (in Assets/) |
| SharpUI | (in Assets/) |

## Controls

| Key | Action |
|-----|--------|
| Left Click | Move / Interact |
| I | Inventory |
| C | Character |
| Z | Skills |
| J | Quest Log |
| O | Options |
| M | Minimap |
| Escape | Cancel action / Close window / Main menu |

## License

All rights reserved.
