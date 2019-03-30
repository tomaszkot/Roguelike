using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Serialization
{
  public class PersistancyWorker
  {
    public void Save(GameManager gm, Action worldSaver)
    {
#if DEBUG
      var heros = gm.CurrentNode.GetTiles<Hero>();
      var heroInNode = heros.SingleOrDefault();
      Debug.Assert(heroInNode != null);
#endif
      var nodeNodeName = gm.CurrentNode.Name;
      //Hero is saved in a separate file, see persister.SaveHero
      if (!gm.CurrentNode.SetEmptyTile(gm.Hero.Point))
        gm.Logger.LogError("failed to reset hero on save");

      gm.Persister.SaveHero(gm.Hero);

      worldSaver();

      var gameState = gm.CreateGameState();
      gm.Persister.SaveGameState(gameState);

      gm.CurrentNode.SetTile(gm.Hero, gm.Hero.Point);
    }

    public void Load(GameManager gm, Func<Hero, GameState, GameNode> worldLoader)
    {
      var hero = gm.Persister.LoadHero();

      var gs = gm.Persister.LoadGameState();

      GameNode node = null;
      //if (worldLoader != null)
        node = worldLoader(hero, gs);
      //else
      //{

      //  //node = gm.Persister.LoadLevel(0);//TODO more levels
      //  //node.SetTile(hero, hero.Point);
      //}
      gm.InitNode(node, true);
      gm.Context.SwitchTo(node, hero, GameContextSwitchKind.GameLoaded);

      gm.PrintHeroStats("load");

      //EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.GameFinished});
    }
  }
}
