using System;
using System.Collections.Generic;
using DungeonDelver.Model.Abilities;
using DungeonDelver.Model.Creatures;
using DungeonDelver.Model.Dto.CreatureDto;

namespace DungeonDelver.Core.Creatures;

/// CreatureDto -> Monster (with its abilities).
/// new monster = new data entry, no code change here.
public static class CreatureFactory
{
    public static Monster Create(CreatureDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var monster = new Monster(dto.Id, dto.Name, dto.MaxHp, dto.AttackPower, dto.Sprite);

        foreach (var ability in CreateAbilities(dto))
            monster.Learn(ability);

        return monster;
    }

    // STUB: hardcoded until abilities are data (AbilityRegistry).
    // later: dto gets an "abilities": ["auto_attack", ...] list, and this looks each id up.
    private static List<Ability> CreateAbilities(CreatureDto dto) => [new AutoAttack()];
}
