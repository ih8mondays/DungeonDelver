using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Core.Dto.CreatureDto;
using DungeonDelver.Core.Loading.Abilities;

namespace DungeonDelver.Core.Loading.Creatures;

internal static class CreatureValidator {
    // catalog = where the ability ids get looked up. null = no catalog given, so listing abilities is a problem
    internal static List<string> Validate(CreatureDto dto, AbilityCatalog abilities = null) {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name)) problems.Add("Creature has no name.");
        if (dto.MaxHp <= 0) problems.Add($"Creature '{dto.Name}' has MaxHp {dto.MaxHp} (must be > 0).");
        if (dto.AttackPower < 0) problems.Add($"Creature '{dto.Name}' has negative AttackPower.");

        if (dto.Abilities is { Count: > 0 }) {
            if (abilities == null)
                problems.Add($"Creature '{dto.Name}' lists abilities, but no ability catalog was given to look them up in.");
            else
                foreach (var id in dto.Abilities.Where(x => !abilities.Contains(x)))
                    problems.Add($"Creature '{dto.Name}' uses unknown ability '{id}'.");

            foreach (var dup in dto.Abilities.Where(id => id != null).GroupBy(id => id).Where(g => g.Count() > 1))
                problems.Add($"Creature '{dto.Name}' lists ability '{dup.Key}' more than once.");
        }

        return problems;
    }
}
