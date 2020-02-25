using System.Collections.Generic;
using System.Linq;
using Dungeons;
using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Tiles;
using SimpleInjector;

namespace Roguelike.Generators
{
  public class LevelGenerator : Dungeons.DungeonGenerator
  {
    //public GameNode Dungeon { get; set; }
    public ILogger Logger { get; set; }
    public int MaxLevelIndex { get; set; } = 1000;
    public int LevelIndex { get; set; }

    public LevelGenerator(Container container) : base(container)
    {
      Logger = container.GetInstance<ILogger>();
    }

    public override List<Dungeons.TileContainers.DungeonNode> CreateDungeonNodes(GenerationInfo info = null)
    {
      var mazeNodes = base.CreateDungeonNodes(info);
      CreateDynamicTiles(mazeNodes);

      return mazeNodes;
    }

    protected virtual void CreateDynamicTiles(List<Dungeons.TileContainers.DungeonNode> mazeNodes)
    {
      if (LevelIndex > 0)//1st node shall have stairs up
      {
        var stairs = new Stairs() { StairsKind = StairsKind.LevelUp, Symbol = '<' };
        //mazeNodes[0].SetTile(stairs, new System.Drawing.Point(3, 1));
        mazeNodes[0].SetTile(stairs, mazeNodes[0].GetEmptyTiles().First().Point);
      }

      if (LevelIndex < MaxLevelIndex)
      {
        var indexWithStairsDown = mazeNodes.Count - 1;
        if (RandHelper.GetRandomDouble() > .5f)
          indexWithStairsDown = mazeNodes.Count - 2;

        if (indexWithStairsDown < 0)
          indexWithStairsDown = 0;

        Stairs stairs = new Stairs() { StairsKind = StairsKind.LevelDown, Symbol = '>' };
        mazeNodes[indexWithStairsDown].SetTile(stairs, mazeNodes[indexWithStairsDown].GetEmptyTiles().Last().Point);
        //node.SetTile(stairs, new System.Drawing.Point(3, 1));
      }
    }

    protected override Dungeons.TileContainers.DungeonNode CreateNode(int nodeIndex, GenerationInfo gi)
    {
      var node = base.CreateNode(nodeIndex, gi);
      var enemy = new Enemy();
      node.SetTile(enemy, new System.Drawing.Point(3, 2));// node.GetRandomEmptyTile().Point);

      node.SetTile(container.GetInstance<LootGenerator>().GetRandomLoot(), new System.Drawing.Point(4, 2));// node.GetRandomEmptyTile().Point);

      var barrel = new Barrel();
      barrel.tag = "barrel";
      node.SetTile(barrel, new System.Drawing.Point(2, 2));// node.GetRandomEmptyTile().Point);

      return node;
    }

    protected override GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      gi.RevealTiles = false;

      return gi;
    }

   
    public override Dungeons.TileContainers.DungeonLevel Generate(int levelIndex, GenerationInfo info = null, LayouterOptions opt = null)
    {
      var revealAllNodes = info != null ? info.RevealAllNodes : false;
      var options = opt ?? new LayouterOptions() { RevealAllNodes = revealAllNodes };
      LevelIndex = levelIndex;
      //generate level
      var baseLevel = base.Generate(levelIndex, info, options);
      var level = baseLevel as Roguelike.TileContainers.GameLevel;
      level.Index = levelIndex;
      level.OnGenerationDone();

     // PopulateDungeonLevel(level);
      return level;
    }



    protected virtual void PopulateDungeonLevel(Roguelike.TileContainers.GameLevel level)
    {
      var lg = new LootGenerator();
      var levelIndex = level.Index;
      var loot = lg.GetRandomWeapon();
      loot.DungeonNodeIndex = levelIndex;
      level.SetTile(loot, level.GetFirstEmptyPoint().Value);

      //var enemy = new Enemy();
      //enemy.DungeonNodeIndex = levelIndex;
      //level.SetTile(enemy, level.GetEmptyTiles().Last().Point);
      //int k = 0;
      //k++;
    }

  }
}
