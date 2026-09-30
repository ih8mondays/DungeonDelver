namespace DungeonDelver.Model.StatusEffects;

/// +amount damage on every hit while active.
public class DamageBoost(int id, string name, int amount, int rounds) : StatusEffect(id, name, rounds) {
	public override int BonusDamage() => amount;
}
