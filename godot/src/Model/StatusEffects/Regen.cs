namespace DungeonDelver.Model.StatusEffects;

/// Gains amount hp every round.
public class Regen(int id, string name, int amount, int rounds) : StatusEffect(id, name, rounds) {
	public override int HpChangePerRound() => amount;
}
