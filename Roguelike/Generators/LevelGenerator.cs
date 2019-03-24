using System;
using System.Collections.Generic;
using System.Linq;
using Dungeons;
using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.TileContainers;
using Roguelike.Tiles;

namespace Roguelike.Generators
{
  public class LevelGenerator : Dungeons.DungeonGenerator
  {
    public GameNode Dungeon { get; set; }
    public ILogger Logger { get; set; }
    public int MaxLevelIndex { get; set; } = 1000;
    public int LevelIndex { get; set; }

    int levelIndex;

    public LevelGenerator(ILogger logger)
    {
      this.Logger = logger;
    }

    public override List<DungeonNode> CreateDungeonNodes()
    {
      var mazeNodes = base.CreateDungeonNodes();
      CreateDynamicTiles(mazeNodes);

      return mazeNodes;
    }

    protected virtual void CreateDynamicTiles(List<DungeonNode> mazeNodes)
    {
      if (levelIndex > 0)//1st node shall have stairs up
      {
        var stairs = new Stairs() { Kind = StairsKind.LevelUp, Symbol = '<' };
        //mazeNodes[0].SetTile(stairs, new System.Drawing.Point(3, 1));
        mazeNodes[0].SetTile(stairs, mazeNodes[0].GetEmptyTiles().First().Point);
      }

      if (levelIndex < MaxLevelIndex)
      {
        var indexWithStairsDown = mazeNodes.Count - 1;
        if (RandHelper.GetRandomDouble() > .5f)
          indexWithStairsDown = mazeNodes.Count - 2;

        if (indexWithStairsDown < 0)
          indexWithStairsDown = 0;

        Stairs stairs = new Stairs() { Kind = StairsKind.LevelDown, Symbol = '>' };
        mazeNodes[indexWithStairsDown].SetTile(stairs, mazeNodes[indexWithStairsDown].GetEmptyTiles().Last().Point);
        //node.SetTile(stairs, new System.Drawing.Point(3, 1));
      }
    }

    protected override DungeonNode CreateNode(int w, int h, GenerationInfo gi, int nodeIndex)
    {
      var node = new GameNode(w, h, gi, nodeIndex);
            
      var enemy = new Enemy();
      node.SetTile(enemy, new System.Drawing.Point(3, 2));// node.GetRandomEmptyTile().Point);
      Logger.LogInfo("added enemy at :" + enemy.Point);
  
      return node;
    }

    protected override GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      gi.RevealTiles = false;

      return gi;
    }

    public override DungeonNode Generate(int levelIndex)
    {
      this.levelIndex = levelIndex;
      LayouterOptions opt = new LayouterOptions();
      opt.RevealAllNodes = false;
      var level = base.Generate<DungeonLevel>(levelIndex, opt);
      level.Index = levelIndex;
      level.OnGenerationDone();
      this.Dungeon = level;

      var sts = level.GetTiles<Stairs>();
      //var level = new DungeonLevel();// new List<TileContainers.DungeonNode> { node });
      // level.AppendMaze(node);
      
      return level;
    }

    
  }
}
