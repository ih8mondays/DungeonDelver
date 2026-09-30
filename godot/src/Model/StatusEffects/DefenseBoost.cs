namespace DungeonDelver.Model.StatusEffects;

/// +amount defense against every hit while active.
public class DefenseBoost(int id, string name, int amount, int rounds) : StatusEffect(id, name, rounds) {
	public override int BonusDefense() => amount;
}
