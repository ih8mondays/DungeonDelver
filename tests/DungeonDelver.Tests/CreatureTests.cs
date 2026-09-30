using DungeonDelver.Model.Abilities;
using DungeonDelver.Model.Creatures;
using Xunit;

namespace DungeonDelver.Tests;

public class CreatureAbilityTests
{
    
    [Fact]
    public void SanityCheck() => Assert.True(true);

    [Fact]
    public void Damage_Creature_TakesDamage() {
        // Arrange
        var creature = new TestCreature();
        
        // Act
        creature.Damage(5);
        
        // Assert
        Assert.Equal(creature.MaxHP - 5, creature.CurrentHP);
    }

    [Fact]
    public void Damage_Creature_NegativeValue_Throws() {
        // Arrange
        var creature = new TestCreature();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Damage(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Damage(-10));
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Damage(-100));
    }

    [Fact]
    public void Damage_Creature_TakesDamage_Dies() {
        // Arrange
        var creatureOne = new TestCreature();
        var creatureTwo = new TestCreature();
        var creatureThree = new TestCreature();
        
        // Act
        creatureOne.Damage(1);
        creatureTwo.Damage(10);
        creatureThree.Damage(100);
        
        // Assert
        Assert.True(creatureOne.IsAlive);
        Assert.False(creatureTwo.IsAlive);
        Assert.False(creatureThree.IsAlive);
    }

    [Fact]
    public void Damage_DeadCreature_Has_NonNegativeHP() {
        // Arrange
        var creatureOne = new TestCreature();
        var creatureTwo = new TestCreature();
        
        // Act
        creatureOne.Damage(10);
        creatureTwo.Damage(100);
        
        // Assert
        Assert.True(creatureOne.CurrentHP >= 0);
        Assert.True(creatureTwo.CurrentHP >= 0);
    }

    [Fact]
    public void Heal_Creature_IsHealed() {
        // Arrange
        var creature = new TestCreature();
        var startHP = creature.CurrentHP;
        
        // Act
        creature.Damage(2);
        creature.Heal(1);

        // Assert
        Assert.Equal(startHP - 1, creature.CurrentHP);
    }

    [Fact]
    public void Heal_DeadCreature_Fails() {
        // Arrange
        var creature = new TestCreature();
        
        // Act
        creature.Damage(creature.CurrentHP);
        creature.Heal(1);

        // Assert
        Assert.Equal(0, creature.CurrentHP);
    }

    [Fact]
    public void Heal_Creature_NegativeValue_Throws() {
        // Arrange
        var creature = new TestCreature();
        
        // Act & Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Heal(-1));
    }

    [Fact]
    public void Heal_Creature_Not_Overhealed() {
        // Arrange
        var creature = new TestCreature();
        var startHP = creature.CurrentHP;
        
        // Act
        creature.Heal(1);
        
        // Assert
        Assert.Equal(startHP, creature.CurrentHP);
        
    }
    
    [Fact]
    public void AddAbility_Success() {
        // Arrange
        var creature = new TestCreature();
        var ability = new TestAbility();

        // Act
        creature.AddAbility(ability);

        // Assert
        Assert.Equal(ability, creature.Abilities[0]);
        Assert.Single(creature.Abilities);
    }

    [Fact]
    public void AddAbility_ExistingAbility_Fails() {
        // Arrange
        var creature = new TestCreature();
        var abilityOne = new TestAbility();
        var abilityTwo = new TestAbility();
        
        // Act
        creature.AddAbility(abilityOne);
        creature.AddAbility(abilityTwo);
        
        // Assert
        Assert.Equal(creature.Abilities.Count(), creature.Abilities.Distinct().Count());
    }

    [Fact]
    public void RemoveAbility_Remove_Existing_Ability() {
        // Arrange
        var creature = new TestCreature();
        var abilityOne = new TestAbility();
        
        // Act
        creature.AddAbility(abilityOne);
        creature.RemoveAbility(abilityOne);
        
        // Assert
        Assert.Empty(creature.Abilities);
    }
    

    // Minimal test doubles, only used by this test class
    private sealed class TestCreature(
        string name = "TestCreature", 
        int maxHP = 10, 
        int attackPower = 1)
        : Creature(name, maxHP, attackPower);

    private sealed class TestAbility(
        string name = "TestAbility",
        int baseDamage = 10,
        string iconId = "TestIcon",
        string spriteId = "TestId") 
        : Ability(name, baseDamage, iconId, spriteId) {
        public override void Use() { }
    }
}