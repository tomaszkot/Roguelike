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

namespace RoguelikeUnitTests
{
  internal class SampleGame
  {
    public GameManager GameManager { get; private set; }
    public Container Container { get; set; }
    //int levelIndex = 0;
    LevelGenerator LevelGenerator { get; set; }
    List<DungeonLevel> levels = new List<DungeonLevel>();

    internal SampleGame()
    {
      Container = new ContainerConfigurator().Container;
      GameManager = Container.GetInstance<GameManager>();
      LevelGenerator = Container.GetInstance<LevelGenerator>();

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

    internal DungeonLevel GenerateLevel(int levelIndex)
    {
      if (LevelGenerator.MaxLevelIndex > 0 && levelIndex > LevelGenerator.MaxLevelIndex)
        throw new Exception("levelIndex > LevelGenerator.MaxLevelIndex");
      LevelGenerator.LevelIndex = levelIndex;
      var level = CreateNewDungeon<DungeonLevel>();
      this.levels.Add(level);
      return level;
    }

    internal DungeonLevel Level { get { return GameManager.CurrentNode as DungeonLevel; } }

    internal void SetMaxLevelindex(int maxLevelIndex)
    {
      LevelGenerator.MaxLevelIndex = maxLevelIndex;
    }

    protected Dungeon CreateNewDungeon<Dungeon>() where Dungeon : GameNode
    {
      var gameNode = LevelGenerator.Generate(LevelGenerator.LevelIndex) as Dungeon;
      GameManager.SetContext(gameNode, AddHero(gameNode), Roguelike.GameContextSwitchKind.NewGame);

      return gameNode;
    }

    protected Hero AddHero(GameNode node)
    {
      var hero = Container.GetInstance<Hero>();
      node.SetTile(hero, node.GetFirstEmptyPoint().Value);
      return hero;
    }

    public Hero Hero { get { return GameManager.Hero; } }
  }
}
