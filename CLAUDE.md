# Zoo World

A small top-down 3D zoo in Unity, built around an extensible animal architecture.

## Docs

The original spec and brief are kept locally in `Docs/Private/`, which is git-ignored: the repo is public, and they must never be committed or quoted. Read both before planning or starting a feature. They are the source of truth; this file doesn't repeat them.
- `Docs/Private/Zoo world_2026.pdf` is the spec with the gameplay requirements. Check every point against it at the end.
- `Docs/Private/zoo-world-prompt.md` is the brief: tech stack, architecture, tests, README and definition of done.

## Guardrails

- No ECS.
- `ZooWorld.Core` has no engine references. Dependencies point one way: Game → Core.
- No singletons, static managers or `FindObjectOfType`. Services come from DI (VContainer).
- Views only react to events (`Animal.Spawned/Ate/Died/Redirected`, `IJumpCues`) and never feed back into logic. Logic never waits for a view; only pooling waits for the death animation.
- The look lives under the prefab's `Visual` child: no colliders there (a contract test enforces it). Headless runs hide it (`ZooInstaller.WithVisuals = false`).
- `DefaultWorld` stays exactly as in the spec (frog and snake). Extra animals go into `ExtendedWorld`.
- Adding an animal from existing parts must stay assets-only (commit type `assets`). If it needs code, the architecture has a gap: say so.

## Unity tooling

The Unity CLI drives the open Editor through `com.unity.pipeline` (verified working). Use the `unity:unity-cli` skill, and pass `--project-path "C:/Users/fyodor.shchukin/Zoo World"` on every call.
- Compile check: `unity recompile --project-path ... --format json`.
- Tests: `unity command test run --mode editor --filter <asm> --filter_type assembly --result-only`. Use this, not `unity test`, which refuses a project that's open in the Editor.
  - PlayMode can't run synchronously (the domain reload drops the request): add `--async_tests true`, then poll `unity command test status`.
  - Contract tests scan assets when they run: don't switch them to `TestCaseSource`, whose cases are cached until a domain reload.
  - Assemblies: `ZooWorld.Core.Tests`, `ZooWorld.Game.Tests` (EditMode), `ZooWorld.Game.PlayModeTests`.
- Build scenes, prefabs and assets in the live Editor, not by hand-editing YAML: use `unity command` or `script eval_file` with a C# script kept outside the repo. Eval wraps code in a method body, so no `using` directives: fully qualify types.
- If a call stalls, check `unity status`. The Editor may be compiling, in Safe Mode, or need focus.
- Play mode freezes when the Editor isn't focused, because the project's Run In Background is off. For checks, `unity command editor.playmode set_run_in_background`, then restore it with `--enable false`. Never commit `runInBackground: 1`.
- Screenshots: `capture screenshot` misses the overlay HUD. In Play mode use `script eval 'UnityEngine.ScreenCapture.CaptureScreenshot("Temp/x.png")'`.

## Unity and package traps

- Never name a private method `Reset`, `Awake`, `OnValidate` or another Unity message by accident. A helper called `Reset()` ran when the component was added in the Editor and zeroed a prefab's scale.
- VContainer overwrites any new file ending in `LifetimeScope.cs` with its template when the `.meta` is created. Rewrite it after the first import.
- PrimeTween: keep the `Sequence`/`Tween` handle and stop that. `Tween.StopAll(target)` errors on tweens nested in a sequence.
- Unity's NUnit is 3.5: no `SetArgDisplayNames` or `Has.Exactly(n).Items`.

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
