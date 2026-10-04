using System.Collections.Generic;
using DungeonDelver.Core.Dto.CreatureDto;

namespace DungeonDelver.Core.Loading.Creatures;

internal static class CreatureValidator {
    internal static List<string> Validate(CreatureDto dto) {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name)) problems.Add("Creature has no name.");
        if (dto.MaxHp <= 0) problems.Add($"Creature '{dto.Name}' has MaxHp {dto.MaxHp} (must be > 0).");
        if (dto.AttackPower < 0) problems.Add($"Creature '{dto.Name}' has negative AttackPower.");
        return problems;
    }
}