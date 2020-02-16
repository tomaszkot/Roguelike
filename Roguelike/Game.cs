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
  public enum Difficulty { Easy, Normal, Hard };

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

      GameManager.WorldLoader = (Hero hero, GameState gs) =>
        {
          levels.Clear();
          TileContainers.DungeonLevel lvl = null;
          var maxLevel = gs.HeroPathValue.LevelIndex;//TODO gs shall have maxLevel, hero might have go upper. Maybe just count level files in dir ?
          for (var i = 0; i <= maxLevel; i++)
          {
            var level = GameManager.LoadLevel(i);
            levels.Add(level);
          }
          lvl = levels[gs.HeroPathValue.LevelIndex];
          return lvl;
        };

      GameManager.WorldSaver = () =>
      {
        for (var i = 0; i < levels.Count; i++)
        {
          GameManager.Persister.SaveLevel(levels[i]);
        }
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
        
        level = LevelGenerator.Generate(levelIndex, new GenerationInfo()) as TileContainers.DungeonLevel;
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
