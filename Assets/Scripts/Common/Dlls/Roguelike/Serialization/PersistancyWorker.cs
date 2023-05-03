using Roguelike.Managers;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Serialization
{
  public class Serialized
  {
    public string Hero { get; set; }
    public string HeroName { get; set; }

  }
  public class PersistancyWorker
  {
    public void Save(GameManager gm, Action<bool> worldSaver, bool quick, Serialized serialized = null)
    {
      gm.Logger.LogInfo("Save starts!");
      if (!quick)
        gm.PrepareForFullSave();//remove not used data

#if DEBUG
      var heros = gm.CurrentNode.GetTiles<Hero>();

      var heroInNode = heros.SingleOrDefault();
      Dungeons.DebugHelper.Assert(heroInNode != null, " heros.Count : "+ heros.Count);
#endif
      //var nodeName = gm.CurrentNode.Name;
      //Hero is saved in a separate file
      //quick mean we remain in the game!
      if (!quick && !gm.CurrentNode.SetEmptyTile(gm.Hero.point))
        gm.Logger.LogError("failed to reset hero on save");
      if(serialized !=null)
        gm.Persister.SaveHero(serialized.Hero, serialized.HeroName, quick);
      else
        gm.Persister.SaveHero(gm.Hero, quick);

      var alliesStore = new AlliesStore();
      foreach (var ally in gm.AlliesManager.AllAllies)
      {
        alliesStore.Allies.Add(ally);
        //if (!quick)
        {
          var set = gm.CurrentNode.SetEmptyTile(ally.Point);
          if (!set)
            gm.Logger.LogError("failed to reset ally on save " + ally);
        }
      }
        
      gm.Persister.SaveAllies(gm.Hero.Name, alliesStore, quick);

      worldSaver(quick);

      var gameState = gm.PrepareGameStateForSave();
      gameState.QuickSave = quick;
      gm.Persister.SaveGameState(gm.Hero.Name, gameState, quick);

      //restore hero
      gm.CurrentNode.SetTile(gm.Hero, gm.Hero.point);
      //restore ally
      if (quick)
      {
        foreach (var ally in gm.AlliesManager.AllAllies)
        {
          gm.Logger.LogInfo("Save end restoring " + ally);
          var set = gm.CurrentNode.SetTile(ally as Dungeons.Tiles.Tile, ally.Point);
          if (!set)
            gm.Logger.LogError("failed to set ally on save " + ally);
        }
      }

      gm.Logger.LogInfo("!Save ends");
    }

    public AbstractGameLevel Load(string heroName, GameManager gm, bool quick, Func<Hero, GameState, bool, AbstractGameLevel> worldLoader)
    {
      gm.Context.NodeHeroPlacedAfterLoad = null;
      //reset context
      gm.Context.Hero = null;
      gm.Context.CurrentNode = null;
      
      var hero = LoadKeyGameElems(gm, heroName, quick);
      var node = worldLoader(hero, gm.GameState, quick);
      return node;
    }

    public static Hero LoadKeyGameElems(GameManager gm, string heroName, bool quick)
    {
      var hero = gm.Persister.LoadHero(heroName, quick);
      var allies = gm.Persister.LoadAllies(heroName, quick);
      gm.AlliesManager.SetEntities(allies.Allies);

      var gs = gm.Persister.LoadGameState(heroName, quick);
      gm.SetGameState(gs);
      return hero;
    }
  }
}
