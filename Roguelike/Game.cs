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
    LevelGenerator LevelGenerator { get; set; }
    Hero Hero { get; }
  }

  public class Game : IGame
  {
    public GameManager GameManager { get; set; }
    public Container Container { get; set; }
    public LevelGenerator LevelGenerator { get; set; }
    public Hero Hero { get { return GameManager.Hero; } }

    List<DungeonLevel> levels = new List<DungeonLevel>();
    public DungeonLevel Level { get { return GameManager.CurrentNode as DungeonLevel; } }

    public Game(Container container)
    {
      Container = container;
      GameManager = container.GetInstance<GameManager>();
      LevelGenerator = container.GetInstance<LevelGenerator>();

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
              GenerateLevel(destLevelIndex);
            }
            GameManager.SetContext(levels[destLevelIndex], Hero, GameContextSwitchKind.DungeonSwitched, stairs);
            return InteractionResult.ContextSwitched;
          }
        }
        return InteractionResult.None;
      };
    }

    public DungeonLevel GenerateLevel(int levelIndex)
    {
      if (LevelGenerator.MaxLevelIndex > 0 && levelIndex > LevelGenerator.MaxLevelIndex)
        throw new Exception("levelIndex > LevelGenerator.MaxLevelIndex");
      LevelGenerator.LevelIndex = levelIndex;
      var level = CreateNewDungeon<DungeonLevel>();
      this.levels.Add(level);
      return level;
    }

    public void SetMaxLevelindex(int maxLevelIndex)
    {
      LevelGenerator.MaxLevelIndex = maxLevelIndex;
    }

    protected Dungeon CreateNewDungeon<Dungeon>() where Dungeon : GameNode
    {
      var gameNode = LevelGenerator.Generate(LevelGenerator.LevelIndex) as Dungeon;
      if(LevelGenerator.LevelIndex == 0)
        GameManager.SetContext(gameNode, AddHero(gameNode), GameContextSwitchKind.NewGame);

      return gameNode;
    }

    protected Hero AddHero(GameNode node)
    {
      var hero = Container.GetInstance<Hero>();
      node.SetTile(hero, node.GetFirstEmptyPoint().Value);
      return hero;
    }

    
  }
}
