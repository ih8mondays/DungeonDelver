using System.Text.Json;
using System.Text.Json.Serialization;
using DungeonDelver.Model.Dungeon.Loading.Dto;

namespace DungeonDelver.Model.Dungeon.Loading;

/// json text -> DungeonDto (plain data, no real rooms yet).
internal static class DungeonJsonParser
{
    // settings for the .NET json reader
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,              // "exits" in json = Exits in c#
        ReadCommentHandling = JsonCommentHandling.Skip,                 // allow // comments in map files
        AllowTrailingCommas = true,                                     // allow a comma after the last item
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,  // unknown field (typo like "exit") = error
    };

    internal static DungeonDto Parse(string json, string source)
    {
        try
        {
            // ?? = if the left side is null, do the right side
            return JsonSerializer.Deserialize<DungeonDto>(json, JsonOptions)
                   ?? throw new InvalidDungeonException(source, new[] { "File is empty (null)." });
        }
        // broken json -> rethrow as our own exception, so callers only catch one type
        catch (JsonException e)
        {
            throw new InvalidDungeonException(source, new[] { $"Bad JSON: {e.Message}" }, e);
        }
    }
}
