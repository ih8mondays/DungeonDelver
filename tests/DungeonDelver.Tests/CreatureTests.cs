using DungeonDelver.Model.Abilities;
using DungeonDelver.Model.Creatures;
using Xunit;

namespace DungeonDelver.Tests;

public class CreatureAbilityTests {
    
    [Fact]
    public void Constructor_ThrowsOnWrongInput() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TestCreature(10, 15));
    }
    
    [Fact]
    public void DamageCreature_TakesDamage() {
        // Arrange
        var creature = new TestCreature(10);
        int testDamage = 5;
        
        // Act
        creature.Damage(testDamage);
        
        // Assert
        Assert.Equal(creature.MaxHp - testDamage, creature.CurrentHp);
    }

    [Fact]
    public void DamageCreature_NegativeValueThrows() {
        // Arrange
        var creature = new TestCreature();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Damage(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Damage(-10));
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Damage(-100));
    }

    [Fact]
    public void DamageCreature_TakesDamageAndDies() {
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
    public void DamageDeadCreature() {
        // Arrange
        var creature = new TestCreature();
        creature.Damage(creature.MaxHp);
        
        // Act
        var result = creature.Damage(10);
        
        // Assert
        Assert.Equal(0, result);

    }

    [Fact]
    public void DamageDeadCreature_HasNonNegativeHP() {
        // Arrange
        var creatureOne = new TestCreature();
        var creatureTwo = new TestCreature();
        
        // Act
        creatureOne.Damage(10);
        creatureTwo.Damage(100);
        
        // Assert
        Assert.True(creatureOne.CurrentHp >= 0);
        Assert.True(creatureTwo.CurrentHp >= 0);
    }

    [Fact]
    public void DamageCreature_MaxValue() {
        // Arrange
        var startHp = 15;
        var creature = new TestCreature(startHp);
        
        // Act
        var result = creature.Damage(int.MaxValue);
        
        // Assert
        Assert.Equal(startHp, result);
        Assert.Equal(0, creature.CurrentHp);
    }

    [Fact]
    public void DamageCreature_RaisesEvent() {
        //TODO
    }
    
    [Fact]
    public void DamageDeadCreature_RaisesNoEvents() {
        // Arrange
        var creature = new TestCreature();
        creature.Damage(creature.MaxHp);
        
        var events = new List<string>();
        creature.HpChanged += (_, _) => events.Add("HpChanged");
        creature.Died      += (_, _) => events.Add("Died");
        
        // Act
        var damage = creature.Damage(5);
        
        // Assert
        Assert.Equal(0, damage);
        Assert.Empty(events);
    }

    [Fact]
    public void HealCreature_IsHealed() {
        // Arrange
        var creature = new TestCreature();
        var startHP = creature.CurrentHp;
        var healing = 1;
        
        // Act
        creature.Damage(2);
        var result = creature.Heal(healing);

        // Assert
        Assert.Equal(healing, result);
        Assert.Equal(startHP - healing, creature.CurrentHp);
    }

    [Fact]
    public void HealDeadCreature_Fails() {
        // Arrange
        var creature = new TestCreature();
        
        // Act
        creature.Damage(creature.CurrentHp);
        var result = creature.Heal(1);

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(0, creature.CurrentHp);
    }

    [Fact]
    public void HealCreature_RaisesEvent() {
        //TODO
    }
    
    [Fact]
    public void HealFullHpCreature_RaisesNoEvents() {
        // Arrange
        var creature = new TestCreature();
        var events = new List<string>();
        creature.HpChanged += (_, _) => events.Add("HpChanged");
        creature.Died      += (_, _) => events.Add("Died");
        
        // Act
        var healed = creature.Heal(5);
        
        // Assert
        Assert.Equal(0, healed);
        Assert.Empty(events);
    }
    
    [Fact]
    public void HealDeadCreature_RaisesNoEvents() {
        // Arrange
        var creature = new TestCreature(10);
        creature.Damage(creature.MaxHp);
        
        var events = new List<string>();
        creature.HpChanged += (_, _) => events.Add("HpChanged");
        creature.Died      += (_, _) => events.Add("Died");
        
        // Act
        var healed = creature.Heal(5);
        
        // Assert
        Assert.False(creature.IsAlive);
        Assert.Equal(0, healed);
        Assert.Empty(events);
    }

    [Fact]
    public void HealCreature_NegativeValueThrows() {
        // Arrange
        var creature = new TestCreature();
        
        // Act & Arrange
        Assert.Throws<ArgumentOutOfRangeException>(() => creature.Heal(-1));
    }

    [Fact]
    public void HealCreature_NotOverhealed() {
        // Arrange
        var creature = new TestCreature();
        var startHp = creature.CurrentHp;
        
        // Act
        creature.Heal(1);
        
        // Assert
        Assert.Equal(startHp, creature.CurrentHp);
        
    }

    [Fact]
    public void HealCreature_MaxValue()
    {
        // Arrange
        var creature = new TestCreature(200,1);
        
        // Act
        var result = creature.Heal(int.MaxValue);
        
        // Assert
        Assert.Equal(199, result);
        Assert.Equal(creature.MaxHp, creature.CurrentHp);
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
        var abilityOne = new TestAbility("Beep");
        var abilityTwo = new TestAbility("Boop");
        
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
        var response = creature.RemoveAbility(abilityOne);
        
        // Assert
        Assert.True(response);
        Assert.Empty(creature.Abilities);
    }
    

    // Minimal test doubles, only used by this test class
    private sealed class TestCreature(
        int maxHp = 10,
        int? currentHp = null,
        int attackPower = 1,
        string name = "TestCreature")
        : Creature(name, maxHp, attackPower, currentHp);

    private sealed class TestAbility(
        string name = "TestAbility",
        int baseDamage = 10,
        int attackPowerScalingPercent = 100,
        int procChancePercent = 100,
        string iconId = "TestIcon",
        string spriteId = "TestId")
        : Ability(name, baseDamage, attackPowerScalingPercent, procChancePercent, iconId, spriteId) { }
}