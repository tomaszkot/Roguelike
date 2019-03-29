using Dungeons;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;

namespace Roguelike
{
  public interface IGame
  {
    GameManager GameManager { get; set; }
    Container Container { get; set; }
    Hero Hero { get; }
    DungeonNode GenerateDungeon();
  }

  public abstract class Game : IGame
  {
    public Game(Container container)
    {
      this.Container = container;
      GameManager = container.GetInstance<GameManager>();
    }

    public GameManager GameManager
    {
      get;
      set;
    }
    public Container Container { get; set; }
    public Hero Hero { get { return GameManager.Hero; } }
    public abstract DungeonNode GenerateDungeon();
  }

  //sample game proving it works
  public class RoguelikeGame : Game
  {
    public IDungeonGenerator DungeonGenerator { get; set; }
    public LevelGenerator LevelGenerator { get { return DungeonGenerator as LevelGenerator; } }
    List<TileContainers.DungeonLevel> levels = new List<TileContainers.DungeonLevel>();
    public TileContainers.DungeonLevel Level { get { return GameManager.CurrentNode as TileContainers.DungeonLevel; } }

    public RoguelikeGame(Container container) : base(container)
    {
      Container = container;
      GameManager.DungeonLevelStairsHandler = (int destLevelIndex, Stairs stairs) => {

        if (levels.Count <= destLevelIndex)
        {
          GenerateLevel(destLevelIndex);
        }
        GameManager.SetContext(levels[destLevelIndex], Hero, GameContextSwitchKind.DungeonSwitched, stairs);
        return InteractionResult.ContextSwitched;
      };
      DungeonGenerator = container.GetInstance<IDungeonGenerator>();
    }

    public TileContainers.DungeonLevel GenerateLevel(int levelIndex) 
    {
      TileContainers.DungeonLevel level = null;
      if (LevelGenerator != null)
      {
        if (LevelGenerator.MaxLevelIndex > 0 && levelIndex > LevelGenerator.MaxLevelIndex)
          throw new Exception("levelIndex > LevelGenerator.MaxLevelIndex");
        LevelGenerator.LevelIndex = levelIndex;
        var opt = new LayouterOptions() { RevealAllNodes = false };
        level = LevelGenerator.Generate(levelIndex, null, opt) as TileContainers.DungeonLevel;
        //level.Index = levelIndex;
        this.levels.Add(level);
      }

      if (levelIndex == 0)
        GameManager.SetContext(level, AddHero(level), GameContextSwitchKind.NewGame);
      
      return level;
    }

    public void SetMaxLevelindex(int maxLevelIndex)
    {
      LevelGenerator.MaxLevelIndex = maxLevelIndex;
    }

    //protected T CreateNewDungeon<T>() where T : GameNode
    //{
    //  var gameNode = LevelGenerator.Generate(Container, LevelGenerator.LevelIndex) as T;
    //  return gameNode;
    //}

    protected Hero AddHero(GameNode node)
    {
      var hero = Container.GetInstance<Hero>();
      node.SetTile(hero, node.GetFirstEmptyPoint().Value);
      return hero;
    }

    public override DungeonNode GenerateDungeon()
    {
      if(LevelGenerator!=null)
        return GenerateLevel(0);

      return DungeonGenerator.Generate(0);
    }

    
  }

 
}
