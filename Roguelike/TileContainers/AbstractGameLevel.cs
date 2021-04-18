using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.TileContainers
{
  //a single room - might be:
  //1. a DungeonLevel - result of composition  of many DungeonNodes and have size like 100x100
  //2. a World - big one dungeon like 500x500 tiles
  public abstract class AbstractGameLevel : Dungeons.TileContainers.DungeonLevel
  {
    int index;//index of the level or world. DungeonPit can have levels with indexes of 0-[n]
    public int Index
    {
      get { return index; }
      set
      {
        index = value;
        NodeIndex = value;//this field shall not be used in AbstractGameLevel or derived ones, but setting it to index shall be a safe move.
      }
    }

    public static Tile EmptyTile = new Tile(symbol: Constants.SymbolBackground);
    public Dictionary<Point, Loot> Loot { get; set; } = new Dictionary<Point, Tiles.Loot>();
    public Dictionary<Point, Surface> Surfaces { get; set; } = new Dictionary<Point, Tiles.Surface>();

    [JsonIgnore]
    public ILogger Logger { get; set; }
    public virtual string Name { get; set; } = "";
    List<Type> extraTypesConsideredEmpty = new List<Type>();


    public AbstractGameLevel(Container container)
   : base(container)
    {
      Logger = Container.GetInstance<ILogger>();
      AddExtraTypesConsideredEmpty(typeof(Loot));
      AddExtraTypesConsideredEmpty(typeof(Surface));
    }

    public Tuple<Point, TileNeighborhood> GetEmptyNeighborhoodPoint(Tile target, EmptyNeighborhoodCallContext context, TileNeighborhood? prefferedSide = null)
    {
      return GetEmptyNeighborhoodPoint(target, prefferedSide, context == EmptyNeighborhoodCallContext.Move ? extraTypesConsideredEmpty : null);
    }

    public void AddExtraTypesConsideredEmpty(Type type)
    {
      extraTypesConsideredEmpty.Add(type);
    }

    public List<Type> GetExtraTypesConsideredEmpty()
    {
      return extraTypesConsideredEmpty;
    }

    protected override void SetDungeonNodeIndex(Tile tile)
    {
      //after load hero on level 1, he was getting wrong dungeonNodeIndex when moved
      //tile.DungeonNodeIndex = this.NodeIndex;
    }

    public override void AppendMaze(Dungeons.TileContainers.DungeonNode childMaze, Point? destStartPoint = null, Point? childMazeMaxSize = null,
      bool childIsland = false, EntranceSide? entranceSideToSkip = null, Dungeons.TileContainers.DungeonNode prevNode = null)
    {
      base.AppendMaze(childMaze, destStartPoint, childMazeMaxSize, childIsland, entranceSideToSkip, prevNode);
    }

    public List<Tile> GetTiles(bool includeLoot) 
    {
      var res = GetTiles<Tile>();
      if (includeLoot)
      {
        res.AddRange(Loot.Values);
      }
      return res;
    }

    public List<Ally> GetActiveAllies()
    {
      return GetTiles<LivingEntity>().Where(i => i is Ally ally && ally.Active).Cast<Ally>().ToList();
    }

    public override List<T> GetTiles<T>()
    {
      var res = base.GetTiles<T>();
      if (IsLoot<T>())
      {
        var lootItems = Loot.Values.Where(i => i is T).Cast<T>().ToList();
        //var res1 = base.GetTiles<Loot>();
        //if (res1.Count != lootItems.Count)
        //{
        //  int k = 0;
        //  k++;
        //}
        res.AddRange(lootItems);
      }

      return res;
    }

    private bool IsLoot<T>() where T : class
    {
      return IsTypeMatching(typeof(Loot), typeof(T));
    }

    public List<Tile> GetClosestEmpties(Point pt)
    {
      return GetEmptyNeighborhoodTiles(GetTile(pt), true);
    }

    public List<Tile> GetClosestEmpties(Tile baseTile)
    {
      return GetEmptyNeighborhoodTiles(baseTile, true);
    }

    public override Tile GetClosestEmpty(Tile baseTile, bool sameNodeId = false, List<Tile> skip = null, bool incDiagonals = true)
    {
      var empties = GetEmptyNeighborhoodTiles(baseTile, incDiagonals);
      if (empties.Any())
        return empties.First();

      //hmm, TODO! maybe that method shall be marked as trowing NotSupportedException/deprecated? 
      //1(Loot is not considered here)
      //2 veeeery slow!
      return base.GetClosestEmpty(baseTile, sameNodeId, skip);
    }

    bool IsLootTile(Tile tile)
    {
      return Loot.Any(j => j.Value.point == tile.point);
    }

    void RemoveLoot(List<Tile> tiles)
    {
      int removed = tiles.RemoveAll(i => IsLootTile(i));
      Logger.LogInfo("removed " + removed);
    }

    public List<Tile> GetEmptyNeighborhoodTiles(Tile target, bool incDiagonals, bool excludeLoot)
    {
      var neibs = GetNeighborTiles(target, incDiagonals);
      neibs.Shuffle();
      neibs.RemoveAll(i => (excludeLoot && IsLootTile(i)));
      neibs.RemoveAll(i => (!IsLootTile(i) && !IsTileEmpty(i)));

      return neibs;
    }

    public Tile GetClosestEmpty(Tile baseTile, bool incDiagonals, bool excludeLootPositions)
    {
      var emptyTiles = GetEmptyNeighborhoodTiles(baseTile, incDiagonals, excludeLootPositions);
      if (emptyTiles.Any())
      {
        return emptyTiles.First();
      }

      //TODO slow, first check neibs of neibs!!!
      emptyTiles = GetEmptyTiles();
      if (excludeLootPositions)
      {
        int removed = emptyTiles.RemoveAll(i => Loot.Any(j => j.Value.point == i.point));
        Logger.LogInfo("removed " + removed);
      }
      return GetClosestEmpty(baseTile, emptyTiles);
    }

    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true,
      bool autoSetTileDungeonIndex = true, bool reportError = true)
    {
      if (tile is IApproachableByHero)
      {
        var abh = tile as IApproachableByHero;
        if (!ApproachableByHero.Contains(abh))
          ApproachableByHero.Add(abh);
      }

      if (tile is ILootSource)
      {
        var ls = tile as ILootSource;
        if (ls.Level <= 0)
        {
          ls.SetLevel(Index+1);//some UT needed it
        }
      }

      if (tile is Hero)
      {
        var tileAtPoint = GetTile(point);
        if (tileAtPoint == tile)
          return true;
        if (tileAtPoint is Stairs)
        {
          Debug.Assert(false);
        }
      }
      else if (tile is Loot loot)
      {
        if (Loot.ContainsKey(point))
        {
          if (Logger != null)
            Logger.LogError("loot already at point: " + Loot[point] + ", trying to add: " + tile + " point:"+ point);
          Debug.Assert(false);
          return false;
        }
        //Logger.LogInfo("Adding Loot "+ tile + " at "+ point + " Loot.Count:"+ Loot.Count);
        tile.point = point;
        Loot[point] = loot;

        return true;
      }

      else if (tile is Surface sur)
      {
        if (Surfaces.ContainsKey(point) && Surfaces[point].Kind != sur.Kind)
        {
          if (Logger != null)
          {
            Logger.LogError("Surface already at point: " + Surfaces[point] + ", trying to add: " + tile + " point:" + point);
            Debug.Assert(false);
            return false;
          }
        }
        //Logger.LogInfo("Adding Loot "+ tile + " at "+ point + " Loot.Count:"+ Loot.Count);
        tile.point = point;
        Surfaces[point] = sur;

        return true;
      }

      Point? prevPos = tile?.point;
      var res =  base.SetTile(tile, point, resetOldTile, revealReseted, autoSetTileDungeonIndex, reportError);
      if (res && tile is LivingEntity && prevPos!=null)
      {
        (tile as LivingEntity).PrevPoint = prevPos.Value;
      }
      return res;
    }

    public virtual void OnGenerationDone()
    {
      
    }

    public bool RemoveLoot(Point point)
    {
      if (Loot.ContainsKey(point))
      {
        Loot.Remove(point);
        return true;
      }

      return false;
    }

    List<Dungeons.TileContainers.DungeonNode> emptyNodes = new List<Dungeons.TileContainers.DungeonNode>();
    [JsonIgnore]
    public virtual List<Dungeons.TileContainers.DungeonNode> GeneratorNodes
    {
      get
      {
        if (!Parts.Any())
          return emptyNodes;
        return Parts[0].Parts.Cast<Dungeons.TileContainers.DungeonNode>().ToList();
        //return Parts[0].Parts.Cast<Dungeons.TileContainers.DungeonNode>().ToList();
      }
    }

    public virtual void OnHeroPlaced(Hero hero)
    {
      
    }

    public Dungeons.TileContainers.DungeonNode GetNodeFromTile(Tile tile)
    {
      var node = GeneratorNodes.Where(i => i.NodeIndex == tile.DungeonNodeIndex).SingleOrDefault();
      return node as Dungeons.TileContainers.DungeonNode;
    }

    Dungeons.TileContainers.DungeonNode GetChildIslandFromTile(Tile tile)
    {
      foreach (var node in GeneratorNodes)
      {
        var isl = node.ChildIslands.FirstOrDefault(i => i.NodeIndex == tile.DungeonNodeIndex);
        if (isl != null)
          return isl as Dungeons.TileContainers.DungeonNode;
      }

      return null;
    }

    public virtual bool RevealRoom(Tiles.Interactive.Door door, Hero hero)
    {
      if (door.IsFromChildIsland)
      {
        var node = GetChildIslandFromTile(door);
        //var parts = Parts;
        node.Reveal(true);
      }
      else
      {
        var neib = GetNeighborTiles(door).Where(i => 
        i !=null && 
        i.DungeonNodeIndex != door.DungeonNodeIndex && 
        i != hero && 
        i.DungeonNodeIndex != Dungeons.TileContainers.DungeonNode.DefaultNodeIndex).FirstOrDefault();

        if (neib != null)
        {
          //var parts = Parts;
          GetNodeFromTile(neib).Reveal(true);
        }
        else
          Container.GetInstance<ILogger>().LogError("neib == null "+ door);
      }
      door.Opened = true;
      return true;
    }

    public override Tile GetTile(Point point)
    {
      var tile = base.GetTile(point);
      if (tile == null || tile.IsEmpty)
      {
        var lootTile = GetLootTile(point);
        return lootTile != null ? lootTile : tile;
      }

      return tile;
    }

    public List<Tiles.Loot> GetLootTilesNearby(Tile tile)
    {
      List<Tiles.Loot> res = Loot.Values
        .Where(i=> i.Revealed && i.DistanceFrom(tile.point) < 3
        //&& i.DungeonNodeIndex == tile.DungeonNodeIndex TODO
        )
        .ToList();//TODO 
      return res;
    }

    public Tiles.Loot GetLootTile(Point point)
    {
      if (Loot.ContainsKey(point))
        return Loot[point];

      return null;
    }

    private byte[,] InitMatrixBeforePathSearch(Point from, Point end, bool forHeroAlly, bool canGoOverCrackedStone, bool forEnemyProjectile)
    {
      var findPathMatrix = new byte[Height, Width];
      int width = Width;
      int height = Height;
      for (int col = 0; col < width; col++)
      {
        for (int row = 0; row < height; row++)
        {
          byte value = 1;
          findPathMatrix[row, col] = value;

          var tile = Tiles[row, col];

          if (tile is Hero)
          {
            if (forHeroAlly)
              findPathMatrix[row, col] = 0;
            continue;
          }
          else if (tile is IDoor door)
          {
            if (/*&& !EnemyCanPassDoors*/ !door.Opened)
              value = 0;
          }
          else if (tile is Dungeons.Tiles.IObstacle)
          {
            if (forHeroAlly && tile is LivingEntity)
            {
              int k = 0;
              k++;
            }
            else
              value = 0;//0
          }
          else if (tile is Wall)
            value = 0;//0
          else if (tile == null)
            value = 0;//0 mean can not move
          else
          {
            if (tile.point.X == from.X && tile.point.Y == from.Y)
            {
              continue;
            }
            else if (tile.point.X == end.X && tile.point.Y == end.Y)
            {
              continue;
            }
            else if (tile is LivingEntity && !forHeroAlly)
            {
              value = 0;
              if (forEnemyProjectile)
              { 
                if(tile is Enemy)
                  value = 1;//outside code must get a straight line to the target
              }
            }
          }

          findPathMatrix[row, col] = value;

        }
      }

      return findPathMatrix;
    }

    public Tile ReplaceTile(Tile replacer, Point point)
    {
      Tile toUse = replacer;
      if (replacer == null)
        toUse = new Tile();
      var prev = GetTile(point);
      toUse.Revealed = true;
      if (SetTile(toUse, point))
      {
        Logger.LogInfo("SetTile done for "+ toUse);
        if (replacer is Loot)
        {
          var tile = new Tile(point);//reset old one, as loot is not hold in Tiles table
          tile.Revealed = true;
          Tiles[point.Y, point.X] = tile;
        }
        return prev;
      }
      return null;
    }

    public List<Algorithms.PathFinderNode> FindPath(Point from, Point endPoint, bool forHeroAlly, bool canGoOverCrackedStone,
      bool forEnemyProjectile)
    {
      //Commons.TimeTracker tr = new Commons.TimeTracker();

      var startPoint = new Algorithms.Point(from.Y, from.X);
      var findPathMatrix = InitMatrixBeforePathSearch(from, endPoint, forHeroAlly, canGoOverCrackedStone, forEnemyProjectile);

      var mPathFinder = new Algorithms.PathFinder(findPathMatrix);
      mPathFinder.Diagonals = false;

      var path = mPathFinder.FindPath(startPoint, new Algorithms.Point(endPoint.Y, endPoint.X));
      if (path != null)
      {
        //System.Diagnostics.//Debug.WriteLine("FindPathTest len = " + path.Count);
      }
      //if (worstPathFind < tr.TotalMiliseconds)
      //    worstPathFind = tr.TotalMiliseconds;
      //Log.AddInfo("FindPathTest end time : " + tr.TotalMiliseconds + ", worstPathFind = " + worstPathFind);
      return path;
    }

    public override string ToString()
    {
      return GetType().Name + " " + GetHashCode() + " " + base.ToString();
    }

    public Stairs GetStairs(StairsKind kind)
    {
      return GetAllStairs(kind).FirstOrDefault();
    }

    public IEnumerable<Stairs> GetAllStairs(StairsKind kind)
    {
      return GetTiles<Stairs>().Where(s => s.StairsKind == kind);
    }

    public Stairs GetPitStairs(string pitName)
    {
      return GetAllStairs(StairsKind.PitDown).Where(i=> i.PitName == pitName).SingleOrDefault();
    }

    public Enemy SpawnEnemy(ILootSource lootSource)
    {
      return SpawnEnemy(lootSource.Level);
    }

    public Enemy SpawnEnemy(int level)
    {
      var enemy = Enemy.Spawn(EnemySymbols.SkeletonSymbol, level);
      enemy.Container = Container;
      return enemy;
    }

    public List<IApproachableByHero> ApproachableByHero { get; set; } = new List<IApproachableByHero>();

    [JsonIgnore]
    public bool Inited { get; set; }

    public void EnsureRevealed(int nodeIndex)
    {
      var notRev = this.GetTiles().Where(i => i.DungeonNodeIndex == nodeIndex && !i.Revealed).ToList();
      if (notRev.Any())
      {
        notRev.ForEach(i => i.Revealed = true);//TODO, that sucks
      }
    }

    public Tile GetHeroStartTile()
    {
      Tile heroStartTile;
      var empOnes = GetEmptyTiles(nodeIndexMustMatch: false).Where(i=>i.DungeonNodeIndex > Dungeons.TileContainers.DungeonNode.ChildIslandNodeIndex);
      var secret = Nodes.Where(i => i.Secret).FirstOrDefault();
      if (secret != null)
      {
        empOnes = empOnes.Where(i => i.dungeonNodeIndex != secret.NodeIndex).ToList();
      }
      var emp = empOnes.FirstOrDefault();
      if (emp == null)
      {
        Logger.LogError("GetHeroStartTile failed!");
      }
      heroStartTile = emp;
      return heroStartTile;
    }

    public void ClearOldHeroPosition(GameContextSwitchKind context)
    {
      var heros = GetTiles<Hero>();
      var heroInNode = heros.SingleOrDefault();

      //if (heroInNode == null && context == GameContextSwitchKind.DungeonSwitched)
      //  Logger.LogError("SwitchTo heros.Count = " + heros.Count);

      if (heroInNode != null)
        SetEmptyTile(heroInNode.point);//Hero is going to be placed in the node, remove it from the old one (CurrentNode)
    }

    public void PlaceHeroAtTile(GameContextSwitchKind context, Hero hero, Tile tile)
    {
      ClearOldHeroPosition(context);
      if (SetTile(hero, tile.point, false))
        hero.DungeonNodeIndex = tile.DungeonNodeIndex;
    }

    public void PlaceHeroNextToTile(GameContextSwitchKind context, Hero hero, HeroPlacementResult res, Tile baseTile)
    {
      Tile heroStartTile = null;
      if (baseTile != null)
      {
        var emptyOnes = GetEmptyNeighborhoodTiles(baseTile);
        if (emptyOnes.Any())
          heroStartTile = emptyOnes.First();
        else
          heroStartTile = GetClosestEmpty(baseTile, false);
      }

      if (heroStartTile == null)
        heroStartTile = GetHeroStartTile();

      heroStartTile.DungeonNodeIndex = heroStartTile.DungeonNodeIndex;
      res.Tile = heroStartTile;
      PlaceHeroAtTile(context, hero, heroStartTile);
    }

    public SurfaceKind GetSurfaceKindUnderHero(Hero hero)
    {
      return GetSurfaceKindUnderTile(hero);
    }

    public SurfaceKind GetSurfaceKindUnderTile(Tile tile)
    {
      return GetSurfaceKindUnderPoint(tile.point);
    }

    public SurfaceKind GetSurfaceKindUnderPoint(Point point)
    {
      SurfaceKind kind = SurfaceKind.Empty;
      if (Surfaces.Any(i => i.Key == point))
        return Surfaces.First(i => i.Key == point).Value.Kind;

      return kind;
    }
  }
}
