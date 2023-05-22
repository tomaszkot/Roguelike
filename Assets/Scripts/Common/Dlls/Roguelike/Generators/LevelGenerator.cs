using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Roguelike.Core.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Generators
{
  public class LevelGenerator : Dungeons.DungeonGenerator
  {
    public ILogger Logger { get; set; }

    public KeyPuzzle KeyPuzzle { get; set; }
    public int LevelIndex { get; set; }
    public EventHandler<DungeonNode> CustomInteriorDecorator;
    public bool StairsUpOnLevel0 { get; set; }
    protected List<string> extraEnemies = new List<string>();
    Difficulty diff;
    Roguelike.Core.Managers.LeverSet leverSet;
    public LevelGenerator(Container container) : base(container)
    {
      Logger = container.GetInstance<ILogger>();
    }

    public LeverSet LeverSet { get => leverSet; protected set => leverSet = value; }

    protected override DungeonNode CreateNode(int w, int h, Dungeons.GenerationInfo gi, int nodeIndex)
    {
      return base.CreateNode(w, h, gi, nodeIndex);
    }
    public override DungeonLevel Generate(int levelIndex, Dungeons.GenerationInfo info = null, LayouterOptions opt = null)
    {
      var revealAllNodes = info != null ? info.RevealAllNodes : false;
      //if (info is Roguelike.Generators.GenerationInfo rgi && MaxLevelIndex == -1)
      //{
      //  MaxLevelIndex = rgi.MaxLevelIndex;
      //}
      this.diff = GenerationInfo.Difficulty;
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

    protected override List<DungeonNode> CreateDungeonNodes(Dungeons.GenerationInfo info = null)
    {
      stairsUp = null;
      var mazeNodes = base.CreateDungeonNodes(info);
      if(info == null || !info.MinimalContent)
        CreateDynamicTiles(mazeNodes);

      return mazeNodes;
    }

    protected override void OnCreate(DungeonNode dungeon, int w, int h, Dungeons.GenerationInfo gi, int nodeIndex)
    {
      dungeon.Create(w, h, gi, nodeIndex);
    }

    protected virtual RoomContentGenerator CreateRoomContentGeneratorForExtraEnemies()
    {
      var roomGen = Container.GetInstance<RoomContentGenerator>();
      roomGen.LevelIndex = LevelIndex;
      return roomGen;
    }

    public GenerationInfo RoguelikeGenInfo
    {
      get => Info as GenerationInfo;
    }

    protected override void CreateDynamicTiles(List<Dungeons.TileContainers.DungeonNode> mazeNodes)
    {
      if (mazeNodes.Any(i => !i.Created))
      {
        Logger.LogError("!i.Created ");
        return;
      }

      if (RoguelikeGenInfo.GenerateEnemies)
      {
        var roomGen = CreateRoomContentGeneratorForExtraEnemies();
        if (roomGen != null)
        {
          foreach (var en in extraEnemies)
          {
            var maze = mazeNodes.GetRandomElem();
            roomGen.AddExtraEnemy(maze, en);
          }
        }
      }
      
      if (ShallGenerateStairsDown())
      {
        var indexWithStairsDown = mazeNodes.Count - 1;
        if (RandHelper.GetRandomDouble() > .5f)
          indexWithStairsDown = mazeNodes.Count - 2;

        if (indexWithStairsDown < 0)
          indexWithStairsDown = 0;

        var maze = mazeNodes[indexWithStairsDown];

        GenerateStairsDown(maze);
      }
    }

    protected virtual bool ShallGenerateStairsDown()
    {
      return LevelIndex < GenerationInfo.DefaultMaxLevelIndex;
    }
    protected virtual void GenerateStairsDown(DungeonNode maze)
    {
      var stairs = new Stairs(Container) { StairsKind = StairsKind.LevelDown, Symbol = '>' };
      //
      var tile = maze.GetRandomEmptyTile(DungeonNode.EmptyCheckContext.DropLoot);//should do job
      if (tile != null)
      {
        var set = maze.SetTile(stairs, tile.point);
        if (stairs.IsFromChildIsland())
        {
          Logger.LogInfo("stairs.IsFromChildIsland! ");
        }
        if(!set)
          Logger.LogError("failed to set stairs down at: " + tile.point);

        Logger.LogInfo("stairs down set at "+ tile.point);
      }
      else
        Logger.LogError("no room for stairs, maze: " + maze);
    }

    protected override void OnChildIslandCreated(ChildIslandCreationInfo e)
    {
      base.OnChildIslandCreated(e);
      var roomGen = Container.GetInstance<RoomContentGenerator>();
      roomGen.Run(e.ChildIslandNode, LevelIndex, e.ChildIslandNode.NodeIndex, 0, e.GenerationInfoIsl as Roguelike.Generators.GenerationInfo);
    }

    protected virtual Stairs CreateStairsUp(int nodeIndex)
    {
      return new Stairs(Container) { StairsKind = StairsKind.LevelUp, Symbol = '<' };
    }

    public override DungeonNode CreateDungeonNodeInstance()
    {
      var node = base.CreateDungeonNodeInstance();
      node.CustomInteriorDecorator = CustomInteriorDecorator;

      return node;
    }

    protected override DungeonNode CreateNode(int nodeIndex, Dungeons.GenerationInfo gi)
    {
      var node = base.CreateNode(nodeIndex, gi);
      if (!node.Created)
        return node;

      if ((LevelIndex > 0 || StairsUpOnLevel0)
        && (!node.Secret && stairsUp == null))
      {
        AddStairsUp(node);
      }

      GenerateRoomContent(nodeIndex, gi, node);

      var barrels = node.GetTiles<Barrel>();
      bool zero1 = barrels.Any(i => i.Level <= 0);
      bool zero2 = node.GetTiles<Chest>().Any(i => i.Level <= 0);
      var rgi = gi as Roguelike.Generators.GenerationInfo;
      DebugHelper.Assert(!rgi.GenerateInteractiveTiles || (!zero1 && !zero2));
      return node;
    }

    Stairs stairsUp;
    protected void AddStairsUp(DungeonNode node)
    {
      var stairs = CreateStairsUp(node.NodeIndex);
      stairsUp = stairs;
      node.SetTile(stairs, node.GetEmptyTiles().First().point);
      OnStairsUpCreated(stairs);
    }

    protected virtual void OnStairsUpCreated(Stairs stairs)
    {
    }

    public override void GenerateRoomContent(int nodeIndex, Dungeons.GenerationInfo gi, DungeonNode node)
    {
      var roomGen = Container.GetInstance<RoomContentGenerator>();
      roomGen.Run(node, LevelIndex, nodeIndex, 0, gi as GenerationInfo);
      
    }

    protected virtual void PopulateDungeonLevel(Roguelike.TileContainers.GameLevel level)
    {
      var lg = new LootGenerator(Container);
      var levelIndex = level.Index;
      var loot = lg.GetRandomEquipment(EquipmentKind.Weapon, levelIndex, null);
      loot.DungeonNodeIndex = levelIndex;
      level.SetTile(loot, level.GetFirstEmptyPoint().Value);
    }

    public virtual void ValidateLevelIndex(int levelIndex)
    {
      
    }

    protected virtual bool CanGenerateOilSpread()
    {
      return true;
    }
    public virtual int MaxLevelIndex
    {
      get => 12;
    }

    protected virtual bool IsLastLevel => LevelIndex == MaxLevelIndex;

    protected virtual Key CreateKey()
    {
      var key = new Key();
      
      return key;
    }

    protected override void DoPostGenerationJobs(DungeonLevel level)
    {
      base.DoPostGenerationJobs(level);

      if (IsLastLevel)
      {
        
      }
     
      if (CanGenerateOilSpread())
      {
        var gl = level as GameLevel;

        var maxSpreads = 2;// 2;
        gl.GenerateOilSpread((int)RandHelper.GetRandomFloatInRange(1, maxSpreads));
      }
    }
  }
}
