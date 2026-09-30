using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Model.Abilities;

public class AutoAttack(string name, int baseDamage, float attackPowerScaling, float procChance, string iconId, string spriteId) 
	: Ability(name, baseDamage, attackPowerScaling, procChance, iconId, spriteId) {
	
	
	public override void Use(Creature user, Creature target) {
		int damage = CalculateDamage(user);
		target.Damage(damage);
	}
	
	
}
