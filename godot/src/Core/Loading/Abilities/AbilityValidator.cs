using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Core.Dto.AbilityDto;

namespace DungeonDelver.Core.Loading.Abilities;

/// all the rules an abilities file has to follow. returns every broken rule, empty list = fine
internal static class AbilityValidator
{
    internal static List<string> Validate(IReadOnlyList<AbilityDto> abilities)
    {
        var problems = new List<string>();

        if (abilities == null || abilities.Count == 0)
        {
            problems.Add("File has no abilities.");
            return problems;
        }

        for (var i = 0; i < abilities.Count; i++)
        {
            var a = abilities[i];
            if (a == null)
            {
                problems.Add($"Ability #{i} is null.");
                continue;
            }

            // name used in messages: the id, or the position (#2) if the id is missing
            var name = string.IsNullOrWhiteSpace(a.Id) ? $"#{i}" : $"'{a.Id}'";

            if (string.IsNullOrWhiteSpace(a.Id)) problems.Add($"Ability #{i} has no id.");
            if (string.IsNullOrWhiteSpace(a.Name)) problems.Add($"Ability {name} has no name.");
            if (a.BaseDamage < 0) problems.Add($"Ability {name} has negative BaseDamage.");
            if (a.AttackPowerScalingPercent is < 0 or > 100) problems.Add($"Ability {name} has AttackPowerScalingPercent {a.AttackPowerScalingPercent} (must be 0 to 100).");
            if (a.ProcChancePercent is < 0 or > 100) problems.Add($"Ability {name} has ProcChancePercent {a.ProcChancePercent} (must be 0 to 100).");
            if (string.IsNullOrWhiteSpace(a.IconId)) problems.Add($"Ability {name} has no iconId.");
            if (string.IsNullOrWhiteSpace(a.SpriteId)) problems.Add($"Ability {name} has no spriteId.");
        }

        var real = abilities.Where(a => a != null).ToList();

        foreach (var dup in real.Where(a => !string.IsNullOrWhiteSpace(a.Id)).GroupBy(a => a.Id).Where(g => g.Count() > 1))
            problems.Add($"Ability id '{dup.Key}' is used {dup.Count()} times.");

        // Creature.AddAbility refuses two abilities with the same name, so names must be unique too
        foreach (var dup in real.Where(a => !string.IsNullOrWhiteSpace(a.Name)).GroupBy(a => a.Name).Where(g => g.Count() > 1))
            problems.Add($"Ability name '{dup.Key}' is used {dup.Count()} times.");

        return problems;
    }
}
