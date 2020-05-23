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
        var nodeNodeName = gm.CurrentNode.Name;
        //Hero is saved in a separate file
        if (!gm.CurrentNode.SetEmptyTile(gm.Hero.Point))
          gm.Logger.LogError("failed to reset hero on save");

        gm.Persister.SaveHero(gm.Hero);
      }
      worldSaver();

      var gameState = gm.CreateGameState();
      gm.Persister.SaveGameState(gm.Hero.Name,gameState);

      //restore hero
      gm.CurrentNode.SetTile(gm.Hero, gm.Hero.Point);
    }

    public void Load(GameManager gm, Func<Hero, GameState, AbstractGameLevel> worldLoader)
    {
      var hero = gm.Persister.LoadHero(gm.Hero.Name);

      var gs = gm.Persister.LoadGameState(gm.Hero.Name);

      AbstractGameLevel node = null;
      node = worldLoader(hero, gs);
      //gm.InitNode(node, true);
      gm.Context.SwitchTo(node, hero, GameContextSwitchKind.GameLoaded);

      gm.PrintHeroStats("load");

      //EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.GameFinished});
    }
  }
}
