﻿using System.Collections.Generic;
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

    public override List<DungeonNode> CreateDungeonNodes(GenerationInfo info = null)
    {
      var mazeNodes = base.CreateDungeonNodes(info);
      CreateDynamicTiles(mazeNodes);

      return mazeNodes;
    }

    protected virtual void CreateDynamicTiles(List<DungeonNode> mazeNodes)
    {
      if (LevelIndex > 0)//1st node shall have stairs up
      {
        var stairs = new Stairs() { StairsKindValue = StairsKind.LevelUp, Symbol = '<' };
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

        Stairs stairs = new Stairs() { StairsKindValue = StairsKind.LevelDown, Symbol = '>' };
        mazeNodes[indexWithStairsDown].SetTile(stairs, mazeNodes[indexWithStairsDown].GetEmptyTiles().Last().Point);
        //node.SetTile(stairs, new System.Drawing.Point(3, 1));
      }
    }

    protected override DungeonNode CreateNode(int nodeIndex, GenerationInfo gi)
    {
      var node = base.CreateNode(nodeIndex, gi);
      var enemy = new Enemy();
      node.SetTile(enemy, new System.Drawing.Point(3, 2));// node.GetRandomEmptyTile().Point);s

      var loot = new Loot();
      node.SetTile(loot, new System.Drawing.Point(4, 2));// node.GetRandomEmptyTile().Point);s
      return node;
    }

    protected override GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      gi.RevealTiles = false;

      return gi;
    }

    public override Dungeons.DungeonLevel Generate(int levelIndex, GenerationInfo info = null, LayouterOptions opt = null)
    {
      var level = base.Generate(levelIndex, info, opt) as Roguelike.TileContainers.DungeonLevel;
      level.Index = levelIndex;
      level.OnGenerationDone();//TODO
      return level;
    }

  }
}
