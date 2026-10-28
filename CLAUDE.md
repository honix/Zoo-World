# Zoo World

A small top-down 3D zoo in Unity, built around an extensible animal architecture.

## Docs

Read both before planning or starting a feature. They are the source of truth; this file doesn't repeat them.
- `Docs/Zoo world_2026.pdf` is the spec with the gameplay requirements. Check every point against it at the end.
- `Docs/Prompts/zoo-world-prompt.md` is the brief: tech stack, architecture, tests, README and definition of done.

## Guardrails

- No ECS.
- `ZooWorld.Core` has no engine references. Dependencies point one way: Game → Core.
- No singletons, static managers or `FindObjectOfType`. Services come from DI (VContainer).

## Unity tooling

The Unity CLI drives the open Editor through `com.unity.pipeline` (verified working). Use the `unity:unity-cli` skill, and pass `--project-path "C:/Users/fyodor.shchukin/Zoo World"` on every call.
- Compile check: `unity recompile --project-path ... --format json`.
- Tests: `unity command test run --mode editor|playmode --filter <asm> --filter_type assembly --result-only`. Use this, not `unity test`, which refuses a project that's open in the Editor.
- Build scenes, prefabs and assets with `unity command` (e.g. `scene get_hierarchy`, `prefab create_variant`, `asset create`), not by hand-editing YAML.
- If a call stalls, check `unity status`. The Editor may be compiling, in Safe Mode, or need focus.

## Workflow

- **Plan first**: show the structure, main classes and test plan, then wait for approval before writing code.
- Work in small steps. Commit after each meaningful step with a clear message. Avoid giant commits.
- Run the tests after each step.
- If the spec is ambiguous, pick the simplest reasonable option, tell the user, and record it under "Decisions" in the README.

## Commit messages

Conventional Commits: `<type>: <Short summary in imperative mood>`, with an optional body that explains what and why.
- Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `assets`.
- Summary: imperative mood ("Add", not "Added"), about 50 characters, capitalized, no trailing period.
- Each commit does one logical thing. Commit `.meta` files with their assets.
- Keep the default Claude Code `Co-Authored-By` trailer.
