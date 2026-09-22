# DungeonDelver

A small dungeon-crawler built in Godot 4.7 with C# / .NET.

Dungeons are defined as JSON map files (`maps/`) and loaded into a room graph
at runtime — rooms are nodes, exits are directed edges, and combat rooms have
to be cleared before you can move on.

![Map loader showing the Crypt of the Ember King room graph](src/Resources/readme/Loader_2.png)

## Layout

- `maps/` — dungeon map files (JSON)
- `src/Model/Dungeon/` — `Room`, `CombatRoom`, `ShopRoom`, `DungeonMap` and the loader
- `src/Model/Creatures/` — player and monsters, with their data classes
- `src/Model/Abilities/` — abilities
- `src/Scenes/` — Godot scenes
- `src/Simulation/` — simulation / test harness

## Running

Open the project in Godot 4.7 (with .NET support) or Rider and run it.
