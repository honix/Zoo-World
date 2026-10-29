# Architecture patterns

This page lists the patterns Zoo World uses, where to find each one, and why it's there. The second half answers questions that come up when reading the code.

Paths are relative to `Assets/ZooWorld/Scripts/`.

## Patterns in use

### Dependency Injection with a composition root

All wiring happens in one place: `Game/Infrastructure/ZooInstaller.cs`, run by `ZooLifetimeScope` with VContainer. No class looks up its own dependencies. There are no singletons, static managers or `FindObjectOfType` calls.

**Why:** every dependency is visible in one file, and tests can build the same graph with fakes.

### Strategy

Behaviour that varies sits behind an interface, and the installer picks the implementation:

| Interface | Implementation | Decides |
|---|---|---|
| `IFoodChain` | `DietFoodChain` | who eats whom |
| `ITieBreaker` | `CoinFlipTieBreaker` | which predator survives when two meet |
| `ISpawnStrategy<T>` | `WeightedRandomSpawnStrategy<T>` | which animal appears next |
| `IMovement` | `JumpMovement`, `LinearMovement`, `ZigZagMovement` | how an animal moves |

**Why:** a new rule or movement is a new class. Nothing that already exists changes.

### Factory Method

Each movement settings asset creates its own movement: `MovementSettings.CreateMovement(IRandom)` in `Game/Movement/MovementSettings.cs`. `LinearMovementSettings` returns a `LinearMovement`, `JumpMovementSettings` a `JumpMovement`, and so on.

**Why:** the data picks the movement, so no code ever runs a `switch` on the animal type.

### Object Pool

`AnimalFactory` (`Game/Animals/AnimalFactory.cs`) keeps one `ObjectPool<Animal>` per prefab. `TastyLabelPresenter` pools its labels the same way.

**Why:** an animal appears every 1–2 seconds and dies often, so pooling avoids constant `Instantiate`/`Destroy` and the garbage it creates.

### Observer

The logic raises events and views listen to them:

- `Animal.Spawned`, `Ate`, `Died` and `Redirected`
- `IJumpCues.Jumped` and `Landed`
- `CollisionService.Eaten`, `AnimalRegistry.Died` and `DeathStats.Changed`

Views never call back into the logic.

**Why:** you can add, remove or turn off a view (headless runs have none) without touching any rule.

### Model–View–Presenter (Passive View)

The HUD uses MVP:

- **Model:** `DeathStats`, `CollisionService`
- **Presenter:** `DeathCounterPresenter`, `TastyLabelPresenter`
- **View:** `DeathCounterView`, `TastyLabel`

The presenter subscribes to the model, turns its data into what the view needs, and pushes it in, for example `DeathCounterView.Show(int deadPrey, int deadPredators)`. The view doesn't know the model or the presenter.

**Why:** the views stay as simple as possible, and the presenters are plain C# classes that can be tested without a scene.

### Data-driven design (Type Object)

An animal type is data, not a class. An `AnimalConfig` ScriptableObject holds a prefab, tags, a diet and a movement settings asset. An `AnimalTag` asset is a label that diets refer to.

**Why:** this is how "1000 different animals" scales. An animal built from existing parts is a new config and a prefab variant, with no code. `ExtendedWorld`'s spiders and turtles were added this way.

### Layers with ports and adapters

`ZooWorld.Core` holds the rules: food chain, spawning, stats, randomness and time. It has no Unity references, and `ZooWorld.Game` depends on it, never the other way. Unity plugs in through adapters:

- `ITimeProvider` → `UnityTimeProvider`
- `AnimalProfileMapper`: `AnimalConfig` → `AnimalProfile`, and `AnimalTag` → `TagId`

**Why:** the rules can be unit-tested and run headless, and a same-seed run replays exactly (`HeadlessSimulationTests`).

### Composition over inheritance

There's no `Animal → Predator → Snake` hierarchy. An `Animal` is put together from parts: a movement (`IMovement`), a profile (tags and diet), and views under its `Visual` child (`AnimalView`, `HopView`, `SlitherView`, `TurnView`, `LegGaitView`), each tuned by a preset asset.

**Why:** a new animal mixes existing parts instead of fitting into a class tree.

### Content validation (contract tests)

`Tests/Game/ContentContractTests.cs` tests the content, not the code. Each test searches the project for every `AnimalConfig` or `WorldSettings` asset when it runs and checks all of them, so a new animal is covered as soon as it exists, even in an Editor that's been open the whole time. A failure lists every broken asset at once, for example `"NewAnimal: the Visual must not change physics, but has colliders on Cube"`.

| Check | Verifies |
|---|---|
| `Animals_ConfigsAreValid` | prefab and movement are set, there's at least one tag, no empty slots, the movement's values make sense |
| `Animals_PrefabsFollowTheBaseLayout` | the Rigidbody fits the ground plane, there's a solid collider on the root, a `Visual` child with no colliders, no zero-scale parts |
| `Animals_CanBeSpawned` | the animal spawns alive, in place, with a valid heading |
| `Worlds_SettingsAreValid` | spawn timing makes sense and every animal in the table is valid |
| `DefaultWorld_MatchesTheSpec` | frog and snake only, one spawn every 1–2 s, frog = jumping prey, snake = linear predator |

`ZooSceneSmokeTests` goes one step further: it plays the real scene and checks that a snake really eats a frog.

**Why one test per check, not one per animal:** an earlier version used `[TestCaseSource]` to give each animal its own line in the Test Runner. NUnit builds those cases once, when scripts load, so an animal added in an open Editor wasn't tested until the next script reload. With a thousand animals, a few thousand near-identical rows wouldn't be easier to read anyway. The failure message already names the animal.

**Why:** with "1000 animals", most changes are content made in the Editor, often by designers. These tests catch everyday mistakes, such as a missing tag that makes an animal impossible to eat, or the default collider on a Unity cube left under `Visual`. Adding an animal is data only, and every piece of that data is checked automatically. Studios call this content or asset validation and often run it in CI.

### Smaller ones

- **Registry:** `AnimalRegistry` is the set of living animals. An animal is alive exactly while its id is registered.
- **Value objects:** `AnimalId`, `TagId`, `Meal` and `AnimalDeath` are immutable and compared by value.
- **Seeded random streams:** `RandomStreams` derives separate streams for spawning, movement and tie-breaks from one master seed, so extra random draws in one system never shift another.

## Questions and answers

### Why does `AnimalFactory` have two pool dictionaries?

There's one pool per prefab. The two dictionaries point to the same pools from different directions:

- **`_poolsByPrefab` (prefab → pool)** is used by `Create`. It has a config, so it knows the prefab.
- **`_poolsByInstance` (instance → pool)** is used by `Release`. It only has the live object and needs to know which pool it came from. An instance is added when the pool creates it and removed when the pool destroys it.

The second dictionary is a convenience. `Release` could look up `animal.Config.Prefab` instead, which relies on the config's prefab always being the one the animal came from.

### Why do animals get an `AnimalId`?

1. **Core can't see Unity.** `CollisionService` and `AnimalRegistry` can't hold an `Animal` MonoBehaviour, so they work with an `AnimalId` and a plain `AnimalProfile`. On the Game side, `AnimalPopulation` maps each id back to its `Animal`.
2. **A pooled object gets a new identity every life.** The same GameObject is reused, but each spawn registers a new id. Anything that refers to an old life can't affect the reused object:
   - Unity reports each contact twice. The second report finds the loser's id already dead and is ignored, so two snakes can never eat each other.
   - A death animation still playing can't kill or feed the animal that reuses the object later.
3. **Same seed, same run.** `CollisionService.Report` resolves each pair in id order, so the winner doesn't depend on which collider Unity reported first.

This is also why logic never waits for a view. `Kill` removes the id at once, and the object returns to the pool after its death animation.

### How does an `AnimalConfig` become an `AnimalProfile`?

`AnimalProfileMapper.GetProfile(config)` does it when `AnimalFactory.Create` spawns an animal:

1. Each `AnimalTag` asset is given a `TagId`: the next integer, plus the asset's name. The same asset always gets the same id.
2. The config's tags and diet become lists of those ids: `new AnimalProfile(config.name, tags, diet)`.
3. The profile is cached per config, so all frogs share one profile.

With the shipped configs:

| Config | Tags | Diet | Role |
|---|---|---|---|
| Frog | Animal, Amphibian | – | prey (empty diet) |
| Snake | Animal, Reptile | Animal | predator |

`snake.CanEat(frog)` checks whether any id in the snake's diet is among the frog's tags. `DietFoodChain` checks both directions: one side eats, the tie-breaker picks between two that can eat each other, and two that can't both bounce. There's no "predator" flag and no `if (animal is Snake)`. A spider that eats frogs but not snakes is just a config with tags `Animal, Arachnid` and diet `Amphibian`.

Ids are assigned in the order tags are first seen, so they can differ between runs. That's fine because they only live in memory. If tags ever needed to be saved or sent over a network, they'd need stable ids such as the asset GUID.

### Why do tag ids carry a name?

At first the debugger showed a snake's diet as `{Tag#1}`, which meant nothing without the mapper's dictionary. `TagId` now has an optional name, set from the tag asset and used only by `ToString()`, so it shows `{Animal}`. Equality still compares only the number, so the food chain doesn't depend on names, and two assets with the same name stay different tags.

### What does the presenter do in MVP?

It holds the model and the view and pushes the model's data into the view. For `DeathCounterPresenter`, it also:

- **Decides when:** it subscribes in `Start()`, pushes once straight away so the view starts correct, and unsubscribes in `Dispose()`.
- **Translates:** the view only takes two ints. Any formatting or "show X only if Y" rule would live in the presenter.

Full MVP also goes the other way: the view raises user input, and the presenter updates the model. Zoo World's HUD only displays, so it has just the model → view direction.

### MVC, MVP or MVVM?

| | View knows | Who updates the view | Cost |
|---|---|---|---|
| **MVC** | the model | the view reads the model | logic leaks into views, which in Unity are MonoBehaviours and hard to test |
| **MVP (Passive View)** | nothing | the presenter pushes values | a push method for every field |
| **MVVM** | the ViewModel | the view binds to observable properties | binding plumbing, and declarative bindings can fail silently at runtime |

- **MVP** has the lightest view: it's just methods like `Show(int, int)`. The work moves into the presenter, which has to push every change itself.
- **MVVM reverses the dependency.** The ViewModel exposes its state, for example `ReactiveProperty<int> DeadPrey`, and never sees a view, while the view subscribes to it. Binding can be plain code (R3 or UniRx reactive properties) or declared in markup (UI Toolkit runtime binding). Only the markup version uses "magic": reflection or generated code.

**Choice:** MVP. The HUD is two numbers and a pooled label, so explicit pushes are simpler and easier to follow. A large, form-heavy UI would be a better fit for MVVM with binding.

### Which tests use the real content, and which use their own?

It depends on what a test protects.

- **Code tests build their own data.** Food chain, movement, mapper, play area, wizard and headless simulation tests create their tags, configs and worlds in code (`TestAssets`, the Core `Zoo` fakes) or as temporary assets. Renaming, moving or retuning the shipped content can't break them. They share only `AnimalBase.prefab`, which is the physics every animal really has.
- **Content tests use the real content on purpose.** `ContentContractTests` and `ZooSceneSmokeTests` exist to catch broken content, so they must look at what actually ships.

Tuning values, such as a faster snake or a shorter frog jump, fail no test. Tests check rules, not exact numbers, so designers can tune freely.
