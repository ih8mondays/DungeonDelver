namespace DungeonDelver.Model.StatusEffects;

// "rounds" in the json: how long it lasts. 0 = permanent (never expires, carries over between rooms).
// the number fields (bonusDamage ...) default to 0, so an entry only lists what it actually does.

/// Something that sits on a creature for a few rounds. Defined by its numbers, read from data/statuses.json.
/// The hooks are virtual: an effect that needs real logic (stun, thorns ...) can subclass and override them.
/// Live state (RemainingRounds) means: never share one instance between creatures,
/// always build a fresh one from the catalog.
public class StatusEffect(int id, string name, string icon, int rounds,
                          int bonusDamage = 0, int bonusDefense = 0, int bonusHealing = 0, int hpPerRound = 0) {

	public int Id { get; } = id;
	public string Name { get; } = name;
	public string Icon { get; } = icon; // just a string path
	public bool IsPermanent { get; } = rounds == 0;	// rounds == 0 means permanent.
	public int RemainingRounds { get; private set; } = rounds;

	// read by the damage calculation. negative numbers are fine (a curse)
	public virtual int BonusDamage() => bonusDamage;
	public virtual int BonusDefense() => bonusDefense;
	public virtual int BonusHealing() => bonusHealing;
	public virtual int HpChangePerRound() => hpPerRound;  // hp gained (+) or lost (-) at the end of every round.

	// called once per round by the owner, after the round's hp changes have been applied.
	/// returns true = remove it
	public bool AdvanceRound() {
		if (IsPermanent) return false;
		RemainingRounds--;
		return RemainingRounds <= 0;
	}
	
}
