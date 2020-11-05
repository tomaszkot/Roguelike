using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Serialization
{
  public class PersistancyWorker
  {
    public void Save(GameManager gm, Action worldSaver)
    {
      if (gm.Hero.Dirty)
      {
#if DEBUG
        var heros = gm.CurrentNode.GetTiles<Hero>();

        var heroInNode = heros.SingleOrDefault();
        Debug.Assert(heroInNode != null);
#endif
        //var nodeName = gm.CurrentNode.Name;
        //Hero is saved in a separate file
        if (!gm.CurrentNode.SetEmptyTile(gm.Hero.Point))
          gm.Logger.LogError("failed to reset hero on save");

        gm.Persister.SaveHero(gm.Hero);
      }
  
      worldSaver();

      var gameState = gm.PrepareGameStateForSave();
      gm.Persister.SaveGameState(gm.Hero.Name,gameState);

      //restore hero
      gm.CurrentNode.SetTile(gm.Hero, gm.Hero.Point);
    }

    public void Load(string heroName, GameManager gm, Func<Hero, GameState, AbstractGameLevel> worldLoader)
    {
      gm.Context.NodeHeroPlacedAfterLoad = null;
      //reset context
      gm.Context.Hero = null;
      gm.Context.CurrentNode = null;

      var hero = gm.Persister.LoadHero(heroName);
      var gs = gm.Persister.LoadGameState(heroName);
      gm.SetGameState(gs);

      AbstractGameLevel node = null;
      node = worldLoader(hero, gs);

      gm.Context.SwitchTo(node, hero, gs, GameContextSwitchKind.GameLoaded);

      gm.PrintHeroStats("load");

      //EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.GameFinished});
    }
  }
}
