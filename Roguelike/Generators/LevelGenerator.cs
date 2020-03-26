using System;
using System.Collections.Generic;
using System.Linq;
using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Roguelike.Abstract;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
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

    public override List<Dungeons.TileContainers.DungeonNode> CreateDungeonNodes(Dungeons.GenerationInfo info = null)
    {
      
      var mazeNodes = base.CreateDungeonNodes(info);
      CreateDynamicTiles(mazeNodes);

      return mazeNodes;
    }

    protected virtual void CreateDynamicTiles(List<Dungeons.TileContainers.DungeonNode> mazeNodes)
    {
      


      if (LevelIndex < MaxLevelIndex)
      {
        var indexWithStairsDown = mazeNodes.Count - 1;
        if (RandHelper.GetRandomDouble() > .5f)
          indexWithStairsDown = mazeNodes.Count - 2;

        if (indexWithStairsDown < 0)
          indexWithStairsDown = 0;

        Stairs stairs = new Stairs() { StairsKind = StairsKind.LevelDown, Symbol = '>' };
        var maze = mazeNodes[indexWithStairsDown];

        var tile = maze.GetEmptyTiles().LastOrDefault();
        if (tile != null)
          maze.SetTile(stairs, tile.Point);
        else
          Logger.LogError("no room for stairs, maze: " + maze);
        //node.SetTile(stairs, new System.Drawing.Point(3, 1));
      }
    }

    protected override void OnChildIslandCreated(ChildIslandCreationInfo e)
    {
      base.OnChildIslandCreated(e);
      var roomGen = container.GetInstance<RoomContentGenerator>();
      roomGen.Run(e.Child, LevelIndex, e.Child.NodeIndex, e.GenerationInfoIsl as Roguelike.GenerationInfo, container);
    }

    protected override Dungeons.TileContainers.DungeonNode CreateNode(int nodeIndex, Dungeons.GenerationInfo gi)
    {
      var node = base.CreateNode(nodeIndex, gi);

      if (
        //LevelIndex > 0 &&
        nodeIndex==0)//1st node shall have stairs up
      {
        var stairs = new Stairs() { StairsKind = StairsKind.LevelUp, Symbol = '<' };
        //mazeNodes[0].SetTile(stairs, new System.Drawing.Point(3, 1));
        node.SetTile(stairs, node.GetEmptyTiles().First().Point);
        OnStairsUpCreated(stairs);
      }

      GenerateRoomContent(nodeIndex, gi, node);

      //var lpt = node.GetEmptyTiles().First().Point;// new System.Drawing.Point(4, 2);
      //node.SetTile(container.GetInstance<LootGenerator>().GetRandomLoot(), lpt);

      //var barrel = new Barrel();
      //barrel.tag = "barrel";
      //var pt = node.GetEmptyTiles().First().Point;//new System.Drawing.Point(2, 2));
      //node.SetTile(barrel, pt);

      return node;
    }

    protected virtual void OnStairsUpCreated(Stairs stairs)
    {
      //throw new NotImplementedException();
    }

    protected virtual void GenerateRoomContent(int nodeIndex, Dungeons.GenerationInfo gi, DungeonNode node)
    {
      var roomGen = container.GetInstance<RoomContentGenerator>();
      roomGen.Run(node, LevelIndex, nodeIndex, gi as Roguelike.GenerationInfo, container);
    }

    protected override Dungeons.GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      gi.RevealTiles = false;

      return gi;
    }

   
    public override Dungeons.TileContainers.DungeonLevel Generate(int levelIndex, Dungeons.GenerationInfo info = null, LayouterOptions opt = null)
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

    }

  }
}
