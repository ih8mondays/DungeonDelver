namespace DungeonDelver.Model.StatusEffects;

// Status effect type ids, used as "type" in data/statuses.json:
//   1 = DamageBoost   (+amount damage dealt)
//   2 = DefenseBoost  (+amount defense)
//   3 = Regen         (heal amount per round)
//   4 = Burn          (damage amount per round)
//
// "rounds" in the json: how long it lasts. 0 = permanent (never expires, carries over between rooms).

/// Something that sits on a creature for a few rounds.
/// Every hook does nothing by default, subclasses override only what they need.
/// Live state (RemainingRounds) means: never share one instance between creatures,
/// always build a fresh one from the catalog.
public abstract class StatusEffect(int id, string name, int rounds) {

	public int Id { get; } = id;
	public string Name { get; } = name;
	public bool IsPermanent { get; } = rounds == 0;	// rounds == 0 means permanent.
	public int RemainingRounds { get; private set; } = rounds;

	// read by the damage calculation
	public virtual int BonusDamage() => 0;
	public virtual int BonusDefense() => 0;
	public virtual int BonusHealing() => 0;
	public virtual int HpChangePerRound() => 0;

	// called once per round by the owner, after the round's hp changes have been applied.
	// returns true when the effect has run out and the owner should remove it
	public bool AdvanceRound() {
		if (IsPermanent) return false;
		RemainingRounds--;
		return RemainingRounds <= 0;
	}
	
}
