using DungeonDelver.Model.Dungeon;
using DungeonDelver.Core.Loading;
using DungeonDelver.Model.Dto.DungeonDto;

namespace DungeonDelver.Tests;

public class DungeonLoaderTests {
    private static RoomDto R(string id, string type, params string[] exits) =>
        new(id,type,exits.ToList());

    private static DungeonDto Map(params RoomDto[] rooms) =>
        new(DungeonLoader.CurrentVersion, "Test", rooms.ToList());

    private static DungeonDto Line() => Map(
        R("a", "combat", "b"),
        R("b", "shop", "c"),
        R("c", "combat"));

    private static IReadOnlyList<string> ProblemsOf(DungeonDto data) =>
        Assert.Throws<InvalidDungeonException>(() => DungeonLoader.Build(data)).Problems;

    private static string MapsFolder() {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "godot", "maps")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "godot", "maps");
    }

    // A correct map loads: right room types, right start room, last room has no exits, exits point to the right rooms.
    [Fact]
    public void ValidMap_BuildsRightRoomTypes_AndFindsStart() {
        var map = DungeonLoader.Build(Line());

        Assert.Equal("a", map.Start.Id);
        Assert.IsType<CombatRoom>(map.Get("a"));
        Assert.IsType<ShopRoom>(map.Get("b"));
        Assert.True(map.Get("c").IsEnd);
        Assert.Equal(new[] { "b" }, map.ExitsOf(map.Start).Select(r => r.Id));
    }

    // Map files may contain // comments and an extra comma at the end of a list.
    [Fact]
    public void Json_AllowsCommentsAndTrailingCommas() {
        const string json = """
            {
              // hand-written map
              "version": 1,
              "name": "Tiny",
              "rooms": [
                { "id": "start", "type": "combat", "exits": ["end"], },
                { "id": "end", "type": "shop", "exits": [] },
              ],
            }
            """;

        Assert.Equal("start", DungeonLoader.FromJson(json).Start.Id);
    }

    // Every real .json map in godot/maps loads without errors.
    [Fact]
    public void AllMapFilesInGodotMapsFolder_AreValid() {
        var files = Directory.GetFiles(MapsFolder(), "*.json");
        Assert.NotEmpty(files);

        foreach (var file in files)
            DungeonLoader.FromJson(File.ReadAllText(file), Path.GetFileName(file));
    }

    // Broken or wrong JSON is rejected, and the error message names the file.
    // Cases: not JSON at all, "rooms" missing, an unknown field, a typo ("exit" instead of "exits").
    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{ "version": 1, "name": "x" }""")]
    [InlineData("""{ "version": 1, "name": "x", "rooms": [], "extra": 1 }""")]
    [InlineData("""{ "version": 1, "name": "x", "rooms": [ { "id": "a", "type": "shop", "exit": [] } ] }""")]
    public void BadJson_Throws(string json) {
        var ex = Assert.Throws<InvalidDungeonException>(() => DungeonLoader.FromJson(json, "test.json"));
        Assert.Contains("test.json", ex.Message);
    }

    // A map with an unknown version number is rejected.
    [Fact]
    public void WrongVersion_IsRejected() {
        var data = new DungeonDto(99,"Test",new List<RoomDto>()); 
        Assert.Contains(ProblemsOf(data), p => p.Contains("version"));
    }

    // A map with no rooms is rejected.
    [Fact]
    public void NoRooms_IsRejected() =>
        Assert.Contains(ProblemsOf(Map()), p => p.Contains("no rooms"));

    // A room type we don't have (e.g. "boss") is rejected.
    [Fact]
    public void UnknownType_IsRejected() =>
        Assert.Contains(ProblemsOf(Map(R("a", "boss", "b"), R("b", "shop"))), p => p.Contains("unknown type 'boss'"));

    // Two rooms with the same id is rejected.
    [Fact]
    public void DuplicateId_IsRejected() =>
        Assert.Contains(ProblemsOf(Map(R("a", "combat", "b"), R("b", "shop"), R("b", "shop"))),
            p => p.Contains("'b' is used 2 times"));

    // An exit pointing to a room that doesn't exist is rejected.
    [Fact]
    public void ExitToMissingRoom_IsRejected() =>
        Assert.Contains(ProblemsOf(Map(R("a", "combat", "nowhere"), R("b", "shop"))),
            p => p.Contains("'nowhere'"));

    // A room with an exit back to itself is rejected.
    [Fact]
    public void ExitToSelf_IsRejected() =>
        Assert.Contains(ProblemsOf(Map(R("a", "combat", "a", "b"), R("b", "shop"))),
            p => p.Contains("exit to itself"));

    // A room with more than 3 exits is rejected.
    [Fact]
    public void TooManyExits_IsRejected() =>
        Assert.Contains(
            ProblemsOf(Map(R("a", "combat", "b", "c", "d", "e"), R("b", "shop"), R("c", "shop"), R("d", "shop"), R("e", "shop"))),
            p => p.Contains("max 3"));

    // A map must have exactly one room with no exits (the end), so two is rejected.
    [Fact]
    public void TwoEndRooms_IsRejected() =>
        Assert.Contains(ProblemsOf(Map(R("a", "combat", "b", "c"), R("b", "shop"), R("c", "shop"))),
            p => p.Contains("exactly 1 end room"));

    // A room nothing leads to ("lost") is rejected, because the map would then have two start rooms.
    [Fact]
    public void UnreachableRoom_IsRejected()
    {
        var data = Line();
        data.Rooms.Add(R("lost", "shop", "c"));
        Assert.Contains(ProblemsOf(data), p => p.Contains("exactly 1 start room") && p.Contains("lost"));
    }

    // Rooms that lead back to each other (b -> c -> b) are rejected, so the player can't walk in circles.
    [Fact]
    public void Loop_IsRejected() {
        var data = Map(
            R("a", "combat", "b"),
            R("b", "combat", "c", "end"),
            R("c", "combat", "b"),
            R("end", "shop"));
        Assert.Contains(ProblemsOf(data), p => p.Contains("loop") && p.Contains("b") && p.Contains("c"));
    }

    // A map with several mistakes reports all of them at once, not just the first.
    [Fact]
    public void SeveralProblems_AreAllReported() {
        var data = new DungeonDto(999, "", null);
        Assert.True(ProblemsOf(data).Count >= 3);
    }
}
