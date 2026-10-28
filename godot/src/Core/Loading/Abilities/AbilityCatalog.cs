using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DungeonDelver.Core.Dto.AbilityDto;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Core.Loading.Abilities;

/// data/abilities.json -> a lookup from ability id to a ready-made Ability.
/// A map names a creature's abilities by id, the loader asks the catalog to build them.
public sealed class AbilityCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,   // unknown field (typo) = error
    };

    private readonly Dictionary<string, AbilityDto> _byId;

    private AbilityCatalog(Dictionary<string, AbilityDto> byId) => _byId = byId;

    public IReadOnlyCollection<string> Ids => _byId.Keys;

    public bool Contains(string id) => id != null && _byId.ContainsKey(id);

    /// a fresh Ability on every call, so no two creatures share one object
    public Ability Create(string id)
    {
        if (!Contains(id))
            throw new KeyNotFoundException($"Unknown ability id '{id}'.");

        var a = _byId[id];
        return new Ability(a.Name, a.BaseDamage, a.AttackPowerScalingPercent, a.ProcChancePercent, a.IconId, a.SpriteId);
    }

    public static AbilityCatalog FromJson(string json, string source = "(json)")
    {
        try
        {
            var list = JsonSerializer.Deserialize<List<AbilityDto>>(json, JsonOptions)
                       ?? throw new InvalidAbilityCatalogException(source, new[] { "File is empty (null)." });
            return Build(list, source);
        }
        catch (JsonException e)
        {
            throw new InvalidAbilityCatalogException(source, new[] { $"Bad JSON: {e.Message}" }, e);
        }
    }

    public static AbilityCatalog Build(IReadOnlyList<AbilityDto> abilities, string source = "(dto)")
    {
        var problems = AbilityValidator.Validate(abilities);
        if (problems.Count > 0)
            throw new InvalidAbilityCatalogException(source, problems);

        return new AbilityCatalog(abilities.ToDictionary(a => a.Id));
    }
}
