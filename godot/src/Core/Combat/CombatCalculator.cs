using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Core.Combat;

public class CombatCalculator {
    
    public static int CalcDmg(Creature user, Creature target, int baseDamage, float attackPowerScaling) {
        return (int)(baseDamage + user.AttackPower * attackPowerScaling);
    }
}