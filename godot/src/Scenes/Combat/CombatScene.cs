using System.Collections.Generic;
using DungeonDelver.Model.Creatures;
using DungeonDelver.Model.Creatures.Monster;

namespace DungeonDelver.Scenes.Combat;

public class CombatScene
{
    public Player Player = new Player("Mr. Awesome", 2000, 0);
    public List<Creature> Monsters = [new Goblin("Goblin 1", 10, 0)];
    private Creature CurrentTarget;
    private bool CombatOver = false;
    
    public void ExecuteRound() {
        
    }
    
    
    

}