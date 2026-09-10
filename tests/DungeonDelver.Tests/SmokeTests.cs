using System.Reflection;

namespace DungeonDelver.Tests;

/// <summary>
/// Placeholder suite. Exists so the pipeline has something green to run
/// until real tests land. Delete once there is real coverage.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void TestHarness_Runs()
    {
        Assert.True(true);
    }

    [Fact]
    public void GameAssembly_IsReferenced()
    {
        // Proves the test project can see the game assembly, so pure-logic
        // classes from godot/src/ can be tested directly.
        var asm = Assembly.Load("DungeonDelver");
        Assert.Equal("DungeonDelver", asm.GetName().Name);
    }
}
