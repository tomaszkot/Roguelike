﻿using Roguelike.Managers;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Serialization
{
  public class PersistancyWorker
  {
    public void Save(GameManager gm, Action worldSaver)
    {
      gm.PrepareForSave();

      //if (gm.Hero.Dirty)??
      {
#if DEBUG
        var heros = gm.CurrentNode.GetTiles<Hero>();

        var heroInNode = heros.SingleOrDefault();
        Debug.Assert(heroInNode != null);
#endif
        //var nodeName = gm.CurrentNode.Name;
        //Hero is saved in a separate file
        if (!gm.CurrentNode.SetEmptyTile(gm.Hero.point))
          gm.Logger.LogError("failed to reset hero on save");
        gm.Persister.SaveHero(gm.Hero);

        var alliesStore = new AlliesStore();
        foreach (var ally in gm.AlliesManager.AllAllies)
        {
          alliesStore.Allies.Add(ally);
          var set = gm.CurrentNode.SetEmptyTile(ally.Point);
          if (!set)
            gm.Logger.LogError("failed to reset ally on save " + ally);
        }
        gm.Persister.SaveAllies(alliesStore);
      }

      worldSaver();

      var gameState = gm.PrepareGameStateForSave();
      gm.Persister.SaveGameState(gm.Hero.Name, gameState);

      //restore hero
      gm.CurrentNode.SetTile(gm.Hero, gm.Hero.point);
    }

    public void Load(string heroName, GameManager gm, Func<Hero, GameState, AbstractGameLevel> worldLoader)
    {
      gm.Context.NodeHeroPlacedAfterLoad = null;
      //reset context
      gm.Context.Hero = null;
      gm.Context.CurrentNode = null;

      var hero = gm.Persister.LoadHero(heroName);
      var allies = gm.Persister.LoadAllies();
      gm.AlliesManager.SetEntities(allies.Allies);

      var gs = gm.Persister.LoadGameState(heroName);
      gm.SetGameState(gs);

      AbstractGameLevel node = null;
      node = worldLoader(hero, gs);

      gm.SetLoadedContext(node, hero);
      //gm.SetContext(node, hero, GameContextSwitchKind.GameLoaded);

      gm.PrintHeroStats("load");
    }
  }
}
