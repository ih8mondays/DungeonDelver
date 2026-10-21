using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace DungeonDelver.Scenes.Combat;

// Events fire all at once inside Step(); this plays their animations one after another.
// Each animation must contain at least one tween step, or Finished never fires and Drain() hangs.
public class AnimationQueue {
    private readonly Queue<Func<Tween>> _pending = new();

    public void Enqueue(Func<Tween> animation) => _pending.Enqueue(animation);

    // The future loop awaits this after every Step().
    public async Task Drain() {
        while (_pending.Count > 0) {
            var tween = _pending.Dequeue()();
            await tween.ToSignal(tween, Tween.SignalName.Finished);
        }
    }
}
