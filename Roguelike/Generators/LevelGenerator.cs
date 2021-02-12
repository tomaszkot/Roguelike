using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public ILogger Logger { get; set; }
    public int MaxLevelIndex { get; set; }
    public int LevelIndex { get; set; }
    public EventHandler<DungeonNode> CustomInteriorDecorator;
    public bool StairsUpOnLevel0 { get; set; }

    public LevelGenerator(Container container) : base(container)
    {
      Logger = container.GetInstance<ILogger>();
      MaxLevelIndex  = GenerationInfo.MaxLevelIndex;
    }

    public override DungeonLevel Generate(int levelIndex, Dungeons.GenerationInfo info = null, LayouterOptions opt = null)
    {
      var revealAllNodes = info != null ? info.RevealAllNodes : false;
      var options = opt ?? new LayouterOptions() { RevealAllNodes = revealAllNodes };
      LevelIndex = levelIndex;
      //generate level
      var baseLevel = base.Generate(levelIndex, info, options);
      var level = baseLevel as Roguelike.TileContainers.GameLevel;
      level.Index = levelIndex;
      OnLevelGenerated(level);

      // PopulateDungeonLevel(level);
      return level;
    }

    protected virtual void OnLevelGenerated(Roguelike.TileContainers.GameLevel level)
    {
      level.OnGenerationDone();
      //level.GeneratorNodes[0].Reveal(true, true);
    }

    public override List<DungeonNode> CreateDungeonNodes(Dungeons.GenerationInfo info = null)
    {
      var mazeNodes = base.CreateDungeonNodes(info);
      CreateDynamicTiles(mazeNodes);

      return mazeNodes;
    }

    protected override void OnCreate(DungeonNode dungeon, int w, int h, Dungeons.GenerationInfo gi, int nodeIndex)
    {
      dungeon.Create(w, h, gi, nodeIndex);
    }

    protected virtual void CreateDynamicTiles(List<Dungeons.TileContainers.DungeonNode> mazeNodes)
    {
      if (mazeNodes.Any(i => !i.Created))
      {
        Logger.LogError("!i.Created ");
        return;
      }
      if (LevelIndex < MaxLevelIndex)
      {
        var indexWithStairsDown = mazeNodes.Count - 1;
        if (RandHelper.GetRandomDouble() > .5f)
          indexWithStairsDown = mazeNodes.Count - 2;

        if (indexWithStairsDown < 0)
          indexWithStairsDown = 0;

        var stairs = new Stairs() { StairsKind = StairsKind.LevelDown, Symbol = '>' };
        var maze = mazeNodes[indexWithStairsDown];

        var tile = maze.GetRandomEmptyTile();
        if (tile != null)
        {
          maze.SetTile(stairs, tile.Point);
          if (stairs.IsFromChildIsland())
          {
            Logger.LogInfo("stairs.IsFromChildIsland! ");
          }
        }
        else
          Logger.LogError("no room for stairs, maze: " + maze);
        //node.SetTile(stairs, new System.Drawing.Point(3, 1));
      }
    }

    protected override void OnChildIslandCreated(ChildIslandCreationInfo e)
    {
      base.OnChildIslandCreated(e);
      var roomGen = Container.GetInstance<RoomContentGenerator>();
      roomGen.Run(e.ChildIslandNode, LevelIndex, e.ChildIslandNode.NodeIndex, 0, e.GenerationInfoIsl as Roguelike.GenerationInfo, Container);
    }

    protected virtual Stairs CreateStairsUp(int nodeIndex)
    {
      return new Stairs() { StairsKind = StairsKind.LevelUp, Symbol = '<' };
    }

    public override DungeonNode CreateDungeonNodeInstance()
    {
      var node = base.CreateDungeonNodeInstance();
      node.CustomInteriorDecorator = CustomInteriorDecorator;

      return node;
    }

    protected override Dungeons.TileContainers.DungeonNode CreateNode(int nodeIndex, Dungeons.GenerationInfo gi)
    {
      var node = base.CreateNode(nodeIndex, gi);
      if (!node.Created)
        return node;

      if((LevelIndex > 0 || StairsUpOnLevel0)  && nodeIndex == 0)
      {
        AddStairsUp(node);
      }

      GenerateRoomContent(nodeIndex, gi, node);

      var barrels = node.GetTiles<Barrel>();
      bool zero1 = barrels.Any(i => i.Level <= 0);
      bool zero2 = node.GetTiles<Chest>().Any(i => i.Level <= 0);
      Debug.Assert(!zero1 && !zero2);
      return node;
    }

    protected void AddStairsUp(DungeonNode node)
    {
      var stairs = CreateStairsUp(node.NodeIndex);
      node.SetTile(stairs, node.GetEmptyTiles().First().Point);
      OnStairsUpCreated(stairs);
    }

    protected virtual void OnStairsUpCreated(Stairs stairs)
    {
      //throw new NotImplementedException();
    }

    protected virtual void GenerateRoomContent(int nodeIndex, Dungeons.GenerationInfo gi, DungeonNode node)
    {
      var roomGen = Container.GetInstance<RoomContentGenerator>();
      roomGen.Run(node, LevelIndex, nodeIndex, 0, gi as Roguelike.GenerationInfo, Container);
    }

    protected override Dungeons.GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      gi.RevealTiles = false;

      return gi;
    }

    protected virtual void PopulateDungeonLevel(Roguelike.TileContainers.GameLevel level)
    {
      var lg = new LootGenerator(Container);
      var levelIndex = level.Index;
      var loot = lg.GetRandomEquipment(EquipmentKind.Weapon, levelIndex, null);
      loot.DungeonNodeIndex = levelIndex;
      level.SetTile(loot, level.GetFirstEmptyPoint().Value);
    }
  }
}
