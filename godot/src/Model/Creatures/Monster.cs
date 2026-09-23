namespace DungeonDelver.Model.Creatures;

// every enemy is a Monster. goblin, slime, bosses = different data, same class.
// only make a new class when a monster needs different BEHAVIOUR, not different numbers.
public class Monster(string id, string name, int maxHp, int attackPower, string spriteId = null)
    : Creature(name, maxHp, attackPower, spriteId) {

    // which template it was built from, e.g. "goblin"
    public string Id { get; } = id;
}
