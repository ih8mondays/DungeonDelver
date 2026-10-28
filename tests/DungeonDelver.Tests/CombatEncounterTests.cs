using DungeonDelver.Core.Combat;
using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Tests;

public class CombatEncounterTests {
    private Player _player;
    private Creature _creature1;
    private Creature _creature2;
    private Creature _creature3;
    private List<Creature> _monsters;

    public CombatEncounterTests() {
        _player = new Player("Hero", 100, 0);
        _creature1 = new Creature("Goblin", 10, 0);
        _creature2 = new Creature("Slime", 20, 0);
        _creature3 = new Creature("Hydra", 999, 0);
        _monsters = [];
    }

    // #############################################################################
    // --------------------- Constructor Tests -------------------------------------
    // #############################################################################
    
    [Fact]
    public void Constructor_ThrowsOnNullPlayer() {
        // Arrange
        _player = null;
        _monsters.Add(_creature1);
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CombatEncounter(_player, _monsters, 100));
    }
    
    [Fact]
    public void Constructor_ThrowsOnDeadPlayer() {
        // Arrange
        _player.Damage(_player.MaxHp);
        _monsters.Add(_creature1);
        
        // Act & Assert
        Assert.Throws<CombatEncounterException>(() => new CombatEncounter(_player, _monsters, 100));
    }

    [Fact]
    public void Constructor_ThrowsOnNullMonsters() {
        // test for single creature
        _creature2 = null;
        _monsters.Add(_creature1);
        _monsters.Add(_creature2);
        _monsters.Add(_creature3);
        Assert.Throws<ArgumentNullException>(() => new CombatEncounter(_player, _monsters, 100));
        
        // test for the whole list
        _monsters = null;
        Assert.Throws<ArgumentNullException>(() => new CombatEncounter(_player, _monsters, 100));
    }
    
    [Fact]
    public void Constructor_ThrowsOnDeadMonsters() {
        //TODO
    }
    
    // #############################################################################
    // --------------------- Step() Tests ------------------------------------------
    // #############################################################################

    [Fact]
    public void Step_ThrowsOnFinishedEncounter() {
        _monsters.Add(_creature1);
        
        
    }
    
    
    [Fact]
    public void Step_AdvancesRoundByOne() {
        
    }
    
    
    
    
}
