namespace DungeonDelver.Model.StatusEffects;

/// Loses amount hp every round.
public class Burn(int id, string name, int amount, int rounds) : StatusEffect(id, name, rounds) {
	public override int HpChangePerRound() => -amount;
}
