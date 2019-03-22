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
  class PersistancyWorker
  {
    public void Save(GameManager gm)
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
      gm.Persister.SaveLevel(gm.CurrentNode as DungeonLevel);

      var gameState = gm.CreateGameState();
      gm.Persister.SaveGameState(gameState);

      gm.CurrentNode.SetTile(gm.Hero, gm.Hero.Point);
    }

    public void Load(GameManager gm)
    {
      var hero = gm.Persister.LoadHero();

      var gs = gm.Persister.LoadGameState();

      var level = gm.Persister.LoadLevel(0);//TODO more levels
      level.SetTile(hero, hero.Point);
      gm.InitNode(level, true);
      gm.Context.SwitchTo(level, hero, GameContextSwitchKind.GameLoaded);

      gm.PrintHeroStats("load");

      //EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.GameFinished});
    }
  }
}
