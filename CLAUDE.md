# Zoo World

A small top-down 3D zoo in Unity, built around an extensible animal architecture.

## Docs

- `Docs/Zoo world_2026.pdf` is the spec and the source of truth. Check every requirement against it at the end.
- `Docs/Prompts/zoo-world-prompt.md` is the full brief: architecture, tests, README, definition of done. Read it before planning or starting a feature.

## Task in short

Top-down 3D zoo with primitive graphics. Every 1–2 s an animal spawns and moves randomly, and off-screen animals turn back.
- **Frog**: prey, jumps a fixed distance every X seconds.
- **Snake**: predator, moves linearly at a fixed speed.
- **Collisions**: prey vs prey bounce apart by physics; a predator eats prey; predator vs predator leaves one survivor (deterministic rule).
- **"Tasty!"** label appears under a predator when it eats.
- **UI**: top-right uGUI counter of dead prey and dead predators.

Main criterion: an animal architecture that is clear and easy to extend to 1000 more animals. **No ECS.**

## Tech

Unity 6000.3.27f1, URP, built-in physics, VContainer (DI), uGUI + TextMeshPro, Unity Test Framework (EditMode + PlayMode). Keep the package list short. No Addressables, and no extra frameworks beyond DI and one tween library later.

## Architecture rules

- Composition, not inheritance. No `Animal → Predator → Snake` hierarchy.
- `AnimalConfig` ScriptableObject: a new animal from existing parts is a new config + prefab variant, with no code changes.
- Movement is a strategy, using a `MovementSettings` ScriptableObject subclass per type. No enums or switches to edit.
- Food chain is data: `AnimalTag` assets plus a diet list. One service resolves collisions, and each collision pair is processed once.
- Assemblies: `ZooWorld.Core` has no engine references (pure rules), and `ZooWorld.Game` references Core only. Dependencies point one way: Game → Core.
- Thin MonoBehaviours, logic in plain C#. Time and randomness go behind interfaces. Services come from DI, with no singletons or `FindObjectOfType`.
- Logic emits events and views react. Logic never waits for visuals.

## Workflow

- **Plan first**: show the structure, main classes and test plan, then wait for approval before writing code.
- Work in small steps. Commit after each meaningful step with a clear message. Avoid giant commits.
- Run the tests after each step.
- If the spec is ambiguous, pick the simplest reasonable option, tell the user, and record it under "Decisions" in the README.
- Prefer clarity over cleverness. The deadline is short (2.5 days), so don't over-engineer.
- Code, comments and the README are in English.

## Commit messages

Conventional Commits format:

```
<type>: <Short summary in imperative mood>

<Optional body: what and why, not how>
```

- Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `assets`.
- Summary: imperative mood ("Add", not "Added"), about 50 characters, capitalized, no trailing period.
- Each commit does one logical thing. Commit `.meta` files with their assets.
- Never use messages like `wip`, `fix`, or `update`.
- Keep the default Claude Code `Co-Authored-By` trailer.
