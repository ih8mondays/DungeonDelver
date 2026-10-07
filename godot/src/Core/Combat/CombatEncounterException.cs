using System;

namespace DungeonDelver.Core.Combat;

public class CombatEncounterException(string message) : ArgumentException(message);