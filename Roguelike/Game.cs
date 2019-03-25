using Dungeons;
using Dungeons.Tiles;
using Roguelike;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public interface IGame
  {
    GameManager GameManager { get; set; }
    Container Container { get; set; }
    Hero Hero { get; }
    DungeonNode GenerateDungeon();
    void SetAutoHandleStairs(bool v);
  }

  public class Game : IGame
  {
    public GameManager GameManager { get; set; }
    public Container Container { get; set; }
    public IDungeonGenerator DungeonGenerator { get; set; }
    public LevelGenerator LevelGenerator { get { return DungeonGenerator as LevelGenerator; } }
    public Hero Hero { get { return GameManager.Hero; } }

    List<DungeonLevel> levels = new List<DungeonLevel>();
    public DungeonLevel Level { get { return GameManager.CurrentNode as DungeonLevel; } }

    public Game(Container container)//, bool autoHandleStairs = false)
    {
      Container = container;
      GameManager = container.GetInstance<GameManager>();
      DungeonGenerator = container.GetInstance<IDungeonGenerator>();

     
    }

    public T GenerateLevel<T>(int levelIndex) where T : GameNode
    {
      T level = null;
      if (LevelGenerator != null)
      {
        if (LevelGenerator.MaxLevelIndex > 0 && levelIndex > LevelGenerator.MaxLevelIndex)
          throw new Exception("levelIndex > LevelGenerator.MaxLevelIndex");
        LevelGenerator.LevelIndex = levelIndex;
        level = CreateNewDungeon<DungeonLevel>() as T;
        this.levels.Add(level as DungeonLevel);
      }
      else
        level = DungeonGenerator.Generate(levelIndex) as T;

      if (levelIndex == 0)
        GameManager.SetContext(level, AddHero(level), GameContextSwitchKind.NewGame);


      return level;
    }

    public void SetMaxLevelindex(int maxLevelIndex)
    {
      LevelGenerator.MaxLevelIndex = maxLevelIndex;
    }

    protected T CreateNewDungeon<T>() where T : GameNode
    {
      var gameNode = LevelGenerator.Generate(LevelGenerator.LevelIndex) as T;
      //if(LevelGenerator.LevelIndex == 0)
      //  GameManager.SetContext(gameNode, AddHero(gameNode), GameContextSwitchKind.NewGame);

      return gameNode;
    }

    protected Hero AddHero(GameNode node)
    {
      var hero = Container.GetInstance<Hero>();
      node.SetTile(hero, node.GetFirstEmptyPoint().Value);
      return hero;
    }

    public DungeonNode GenerateDungeon()
    {
      if(LevelGenerator!=null)
        return GenerateLevel< DungeonLevel>(0);

      return DungeonGenerator.Generate(0);
    }

    public void SetAutoHandleStairs(bool on)
    {
      if (on)
      {
        GameManager.Interact = (Tile tile) =>
        {
          if (tile is Stairs)
          {
            var stairs = tile as Stairs;
            var destLevelIndex = -1;
            if (stairs.Kind == StairsKind.LevelDown ||
            stairs.Kind == StairsKind.LevelUp)
            {
              if (stairs.Kind == StairsKind.LevelDown)
              {
                destLevelIndex = Level.Index + 1;
              }
              else if (stairs.Kind == StairsKind.LevelUp)
              {
                destLevelIndex = Level.Index - 1;
              }
              if (levels.Count <= destLevelIndex)
              {
                GenerateLevel<DungeonLevel>(destLevelIndex);
              }
              GameManager.SetContext(levels[destLevelIndex], Hero, GameContextSwitchKind.DungeonSwitched, stairs);
              return InteractionResult.ContextSwitched;
            }
          }
          return InteractionResult.None;
        };
      }
      else
        GameManager.Interact = null;
    }
  }
}
