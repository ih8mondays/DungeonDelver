# Tests

`DungeonDelver.Tests` — xUnit, `net8.0`, runs headless via `dotnet test`.

## Run locally

```
dotnet test tests/DungeonDelver.Tests/DungeonDelver.Tests.csproj
```

## What can be tested here

These tests run **outside** the Godot runtime. The engine is never
initialised, so anything that constructs a `Node`, loads a scene, or calls
into Godot's native side will fail at runtime (a compile-time reference to
Godot types is fine — `GodotSharp` comes from NuGet).

Keep game rules, generation, and state logic in plain C# classes that take
their data as arguments and return results; wire those into `Node` scripts
in `godot/src/`. Those plain classes are what gets tested here.

If scene- or node-level tests become necessary later, that needs an
in-engine runner (e.g. GdUnit4) and a Godot binary in CI — a separate job.
