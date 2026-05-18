using System;
using Game.Core.Prototypes;

var loop = new HeIsComingPrototypeLoop();
for (int build = 0; build < 3; build++)
{
    var wins = 0;
    var hpLeft = 0;
    for (int run = 0; run < 1; run++)
    {
        var state = loop.CreateInitialState();
        while (!state.IsGameOver && !state.IsVictory)
        {
            var encounter = loop.ResolveEncounter(state);
            state = encounter.NextState;
            if (state.IsGameOver || state.IsVictory)
            {
                break;
            }
            state = loop.ApplyReward(state, build);
        }
        if (state.IsVictory) wins++;
        hpLeft += state.PlayerHp;
    }
    Console.WriteLine($"build {build}: wins={wins}, hp={hpLeft}");
}
