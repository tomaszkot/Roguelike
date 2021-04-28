using Dungeons;
using Roguelike.Abstract;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike
{
  public enum Difficulty { Easy, Normal, Hard };

  //sample game proving it works
  public class RoguelikeGame : Game
  {
    public IDungeonGenerator DungeonGenerator { get; set; }
    public LevelGenerator LevelGenerator { get { return DungeonGenerator as LevelGenerator; } }
    List<TileContainers.GameLevel> levels = new List<TileContainers.GameLevel>();
    public TileContainers.GameLevel Level { get { return GameManager.CurrentNode as TileContainers.GameLevel; } }

    public RoguelikeGame(Container container) : base(container)
    {
      Container = container;
      GameManager.DungeonLevelStairsHandler = (int destLevelIndex, Stairs stairs) =>
      {

        if (levels.Count <= destLevelIndex)
        {
          GenerateLevel(destLevelIndex, null, true);
          GameManager.Assert(levels[destLevelIndex].Index == destLevelIndex);
        }
        GameManager.SetContext(levels[destLevelIndex], Hero, GameContextSwitchKind.DungeonSwitched, stairs);
        return InteractionResult.ContextSwitched;
      };

      GameManager.WorldLoader = (Hero hero, GameState gs) =>
      {
        levels.Clear();
        GameLevel lvl = null;
        var maxLevel = gs.HeroPath.LevelIndex;//TODO gs shall have maxLevel, hero might have go upper. Maybe just count level files in dir ?
        for (var i = 0; i <= maxLevel; i++)
        {
          GameLevel nextLvl = null;
          if (gs.Settings.CoreInfo.RegenerateLevelsOnLoad)
          {
            nextLvl = GenerateLevel(i, null, false);
          }
          else
          {
            nextLvl = GameManager.LoadLevel(hero.Name, i);
            levels.Add(nextLvl);
          }
        }
        for (var i = 0; i < gs.HeroPath.LevelIndex; i++)
        {
          levels[i].Reveal(true);//TODO, due to bugs with reveal of rooms it's better to do it for whole level
        }
        lvl = levels[gs.HeroPath.LevelIndex];
        return lvl;
      };

      GameManager.WorldSaver = () =>
      {
        for (var i = 0; i < levels.Count; i++)
        {
          GameManager.Persister.SaveLevel(GameManager.Hero.Name, levels[i]);
        }
      };

      DungeonGenerator = container.GetInstance<IDungeonGenerator>();
    }

    public TileContainers.GameLevel GenerateLevel(int levelIndex, Dungeons.GenerationInfo gi = null, bool canSetActive = true)
    {
      TileContainers.GameLevel level = null;
      if (LevelGenerator != null)
      {
        if (LevelGenerator.MaxLevelIndex > 0 && levelIndex > LevelGenerator.MaxLevelIndex)
          throw new Exception("levelIndex > LevelGenerator.MaxLevelIndex");
        LevelGenerator.LevelIndex = levelIndex;

        var generInfo = gi ?? new Generators.GenerationInfo();
        level = LevelGenerator.Generate(levelIndex, generInfo) as TileContainers.GameLevel;

        var merch = this.Container.GetInstance<Merchant>();
        level.SetTileAtRandomPosition(merch);

        this.levels.Add(level);
      }

      if (canSetActive && levelIndex == 0)
        GameManager.SetContext(level, AddHero(level), GameContextSwitchKind.NewGame);

      return level;
    }

    public void SetMaxLevelIndex(int maxLevelIndex)
    {
      LevelGenerator.MaxLevelIndex = maxLevelIndex;
    }

    protected Hero AddHero(AbstractGameLevel node)
    {
      var hero = Container.GetInstance<Hero>();

      var empOnes = node.GetEmptyTiles(nodeIndexMustMatch: false);
      ////TODO PitUp here?
      //Dungeons.Tiles.Tile emptyForHero = node
      //  .GetTiles<Stairs>()
      //  .FirstOrDefault(i => 
      //    i.StairsKind == StairsKind.LevelUp ||
      //    i.StairsKind == StairsKind.PitUp);


      //var secret = node.Nodes.Where(i => i.Secret).FirstOrDefault();
      //if (secret != null)
      //{
      //  empOnes = empOnes.Where(i => i.dungeonNodeIndex != secret.NodeIndex).ToList();
      //}
      //if (emptyForHero == null)
      //{
      //  if (LevelGenerator.LevelIndex != 0)
      //  {
      //    Container.GetInstance<ILogger>().LogError("emptyForHero == null " + LevelGenerator.LevelIndex);
      //  }

      //  emptyForHero = empOnes.First();
      //}
      //var empty = node.GetClosestEmpty(emptyForHero, empOnes);
      var empty = empOnes.First();
      node.SetTile(hero, empty.point);
      return hero;
    }

    public override Dungeons.TileContainers.DungeonNode GenerateDungeon()
    {
      levels.Clear();
      var level = GenerateLevel(0, null, true);
      return level;
    }


  }


}
