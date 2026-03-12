# MabWorld

2D RPG inspired by Mabinogi, built with Unity 2022 LTS and Universal Render Pipeline (URP 14.0.10). Single-player offline game featuring life skills, crafting, quests, and cultivation-based character progression.

## Project Structure

```
Assets/
  Scripts/
    Core/           # GameManager, DatabaseManager, InputController, StateMachine, etc.
    Database/        # Model base class, ItemModel, SkillModel, NPCModel, QuestModel, etc.
    Item/            # Item, InventoryManager, InventoryBag, LootGenerator, UI_Item
    Skill/           # Skill, SkillManager, SkillSlot, WindowSkill
    Crafting/        # CraftingStation, CraftingRecipe, WindowCrafting
    Quest/           # Quest, QuestStateMachine, WindowQuest
    (root)           # Player, Actor, Mob, NPC, MapResource, HUD, Window base class
  Scenes/            # Base.unity (persistent), Homestead.unity, Fuyang Town Outskirts.unity
  StreamingAssets/   # mabinogi.db (SQLite database)
  Prefabs/           # Reusable game object prefabs
  Sprites/           # 2D sprite assets
  Tiles/             # Tileset assets
  Plugins/           # SharpUI, YamlDotNet
  NavMeshComponents/ # NavMeshPlus for 2D pathfinding
  Addressables/      # Addressable asset references
Saves/               # JSON save files (player.json, gameManager.json)
Packages/            # Unity package manifest
```

## Architecture

- **Singletons**: GameManager, Player, InputController, AudioController (DontDestroyOnLoad)
- **State machines**: GameStateMachine (MenuState/PlayState), PlayerStateMachine, ActorMovementStateMachine, MobStateMachine, QuestStateMachine
- **Event system**: EventManager base with IntManager, FloatManager, StringManager, BoolManager for UI change notifications
- **Manager pattern**: DatabaseManager, InventoryManager, SkillManager, WindowManager
- **Factory**: PrefabFactory for centralized instantiation

## Data Layer

- **SQLite** via Mono.Data.Sqlite stored at `Assets/StreamingAssets/mabinogi.db`
- **Model base class** with reflection-based field mapping (ModelFieldReference) and parameterized SQL queries
- Database uses **snake_case** column names, auto-converted to **camelCase** C# fields
- Models use `fieldMap.Add("column_name", new ModelFieldReference(this, nameof(field)))` pattern
- Write operations are editor-only (`#if UNITY_EDITOR`)
- Sprite/AudioClip assets stored as **Addressable keys** in DB, loaded via `GameManager.Instance.LoadAsset<T>(key)`

## Save System

- Newtonsoft.Json serialization with `PreserveReferencesHandling.Objects`
- `[JsonObject(MemberSerialization.OptIn)]` on serializable classes, `[JsonProperty]` on persisted fields
- Save files: `./Saves/player.json`, `./Saves/gameManager.json`

## Scene Management

- **Base.unity** loads persistently (GameManager, Canvas, HUD, MainMenu)
- Content scenes load additively via `LoadSceneMode.Additive`
- `GameStateMachine` drives transitions: MenuState -> PlayState
- MapTransfer components handle scene-to-scene travel with target point IDs

## Code Style

- **C# conventions**: Allman brace style, 4-space indentation
- **Naming**: PascalCase classes/properties, camelCase private fields, `_prefix` for backing fields
- **Prefixes**: `Window` for UI window classes, `UI_` for UI components, `Manager` suffix for systems
- **Documentation**: XML `///` summary comments on public methods
- **Serialization attributes**: `[JsonProperty]`, `[JsonIgnore]`, `[JsonConstructor]`, `[NonSerialized]` used extensively
- No namespaces — all scripts are in the global namespace

## Key Dependencies

- URP 14.0.10, Addressables 1.21.20, Newtonsoft.Json 3.2.1
- TextMeshPro 3.0.6, AI Navigation 1.1.5 (NavMesh)
- SharpUI (in Assets/Plugins), NavMeshPlus (in Assets/NavMeshComponents)

## Git Workflow

- **main** branch for releases, **v1.1dev** for active development
- No CI/CD pipeline
- Target platform: Windows (1920x1080 default resolution)

## Common Tasks

- **Adding a new Model**: Extend `Model`, define fields with `[JsonProperty]`, populate `fieldMap` in constructor, add SQL table to `mabinogi.db`
- **Adding a new Window**: Extend `Window`, register with `WindowManager`, prefix class name with `Window`
- **Adding a new State**: Extend `State`, use `enterAction`/`exitAction`/`mainAction` delegates
- **Asset references**: Store addressable key strings in DB, load with `GameManager.Instance.LoadAsset<T>(key)`
