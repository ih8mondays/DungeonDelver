namespace DungeonDelver.Model.Creatures;

public class Player(string name, int maxHp, int attackPower, string spriteId = null)
    : Creature(name, maxHp, attackPower, spriteId) { 

}