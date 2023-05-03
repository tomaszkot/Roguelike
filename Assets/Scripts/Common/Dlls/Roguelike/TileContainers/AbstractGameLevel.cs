using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Tiles;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    //public Dictionary<Point, Surface> Surfaces { get; set; } = new Dictionary<Point, Tiles.Surface>();

    public Surfaces SurfaceSets { get; set; } = new Surfaces();

    public Layers Layers { get; set; } = new Layers();

    //public Dictionary<Point, Surface> Smoke { get; set; } = new Dictionary<Point, Tiles.Surface>();
    [JsonIgnore]
    public bool EventsHooked { get; set; }

    [JsonIgnore]
    public ILogger Logger { get; set; }
    public virtual string Name { get; set; } = "";
    List<Type> extraTypesConsideredEmpty = new List<Type>();


    public AbstractGameLevel(Container container)
   : base(container)
    {
      Logger = container.GetInstance<ILogger>();
      Layers.Add(KnownLayer.Smoke);
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
      bool childIsland = false, EntranceSide? entranceSideToSkip = null, Dungeons.TileContainers.DungeonNode prevNode = null, bool makeSureNoNulls = false)
    {
      base.AppendMaze(childMaze, destStartPoint, childMazeMaxSize, childIsland, entranceSideToSkip, prevNode, makeSureNoNulls);
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

    public List<IAlly> GetActiveAllies()
    {
      return GetTiles<LivingEntity>().Where(i => i is IAlly ally && ally.Active).Cast<IAlly>().ToList();
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

    public override Tile GetClosestEmpty(Tile baseTile, bool sameNodeId = false, List<Tile> skip = null, bool incDiagonals = true
      ,Func<Tile, bool> canBeUsed = null)
    {
      return base.GetClosestEmpty(baseTile, sameNodeId, skip, incDiagonals,  canBeUsed);
    }

    bool IsLootTile(Tile tile)
    {
      return Loot.Any(j => j.Value.point == tile.point);
    }

    public List<Tile> GetEmptyNeighborhoodTiles(Tile target, bool incDiagonals, bool excludeLoot, EmptyCheckContext emptyCheckContext = EmptyCheckContext.Unset)
    {
      var neibs = GetNeighborTiles(target, incDiagonals);
      neibs.Shuffle();
      neibs.RemoveAll(i => i == null);
      neibs.RemoveAll(i => (excludeLoot && IsLootTile(i)));
      neibs.RemoveAll(i => (!IsLootTile(i) && !IsTileEmpty(i, emptyCheckContext)));

      return neibs;
    }

    public Tile GetClosestEmpty(Tile baseTile, bool incDiagonals, bool excludeLootPositions)
    {
      var emptyTiles = GetEmptyNeighborhoodTiles(baseTile, incDiagonals, excludeLootPositions);
      if (emptyTiles.Any())
      {
        return emptyTiles.First();
      }

      Func<Tile, bool> canBeUsed = (Tile tile) =>
      {
        if (excludeLootPositions)
        {
          if(Loot.Any(j => j.Value.point == tile.point))
            return false;
        }
        
        return true;
      };

      return base.GetClosestEmpty(baseTile, false, null, incDiagonals, canBeUsed);
    }

    bool CanBeInLayer(SurfaceKind sk, Tile tile)
    {
      if (tile == null)
        return false;

      if (tile is Wall)
        return false;
      if(sk == SurfaceKind.Oil)
        return tile.IsEmpty;

      return true;
    }

    private List<Tile> GetNeibsForLayer(Tile startingTile, int range, SurfaceKind sk)//, ref List<Tile> sides)
    {
      var neibs = GetNeighborTiles(startingTile, true, (int)range)
        .Where(i => CanBeInLayer(sk, i))
        .ToList();
      //CalcSurfaceSetSides(sk, sides, neibs);
      //neibs.AddRange(sides);
      return neibs;
    }

    private List<HitableSurface> CalcSurfaceSetSides(SurfaceKind sk, List<Tile> neibs)
    {
      var sides = new List<HitableSurface>();
      {
        var maxX = neibs.Max(i => i.point.X);
        var neibMaxX = neibs.Where(i => i.point.X == maxX).ToList().GetRandomElem();
        var side = GetNeighborTiles(neibMaxX).Where(i => CanBeInLayer(sk, i) && i.point.X > maxX).FirstOrDefault();
        AddOilSide(sides, side, SurfacePlacementSide.Left);
      }
      {
        var minX = neibs.Min(i => i.point.X);
        var neibMinX = neibs.Where(i => i.point.X == minX).ToList().GetRandomElem();
        var next = GetNeighborTiles(neibMinX).Where(i => CanBeInLayer(sk, i) && i.point.X < minX).FirstOrDefault();
        AddOilSide(sides, next, SurfacePlacementSide.Right);
      }
      {
        var maxY = neibs.Max(i => i.point.Y);
        var neibMax = neibs.Where(i => i.point.Y == maxY).ToList().GetRandomElem();
        var next = GetNeighborTiles(neibMax).Where(i => CanBeInLayer(sk, i) && i.point.Y > maxY).FirstOrDefault();
        AddOilSide(sides, next, SurfacePlacementSide.Top);
      }
      {
        var minY = neibs.Min(i => i.point.Y);
        var neibMinY = neibs.Where(i => i.point.Y == minY).ToList().GetRandomElem();
        var next = GetNeighborTiles(neibMinY).Where(i => CanBeInLayer(sk, i) && i.point.Y < minY).FirstOrDefault();
        AddOilSide(sides, next, SurfacePlacementSide.Bottom);
      }

      return sides;
    }

    private static void AddOilSide(List<HitableSurface> sides, Tile side, SurfacePlacementSide placementSide)
    {
      if (side != null)
      {
        sides.Add(new HitableSurface()
        {
          Kind = SurfaceKind.Oil,
          point = side.point,
          tag1 = RandHelper.GetRandomDouble() > .5 ? "pure_oil_side1" : "pure_oil_side2",
          PlacementSide = placementSide
        }) ;
      }
    }

    public List<Surface> SpreadOil(Tile startingTile, int minRange = 1, int maxRange = 1, List<Dungeons.Tiles.Tile> emptyTilesToUse = null,
      bool reveal = false)
    {
      var range = RandHelper.GetRandomDouble() > .5 ? minRange : maxRange;
      //var sides = new List<Tile>();
      var neibs = GetNeibsForLayer(startingTile, range, SurfaceKind.Oil);
      neibs.Add(startingTile);
      var surfaces = new List<Surface>();
      foreach (var emp in neibs)
      {
        var tag1 = "pure_oil";
        //if (sides.Contains(emp))
        //  tag1 = RandHelper.GetRandomDouble() > .5 ? "pure_oil_side1" : "pure_oil_side2";
        var sutf = new HitableSurface(tag1)
        {
          Kind = SurfaceKind.Oil
        };
        sutf.point = emp.point;
        if (emptyTilesToUse != null)
          emptyTilesToUse.Remove(emp);

        AddSurface(surfaces, sutf, reveal);
      }
      var sides = CalcSurfaceSetSides(SurfaceKind.Oil, neibs);
      foreach (var surSide in sides)
        AddSurface(surfaces, surSide, reveal);
      return surfaces;
    }

    private void AddSurface(List<Surface> surfaces, HitableSurface sutf, bool reveal)
    {
      sutf.Revealed = reveal;
      this.SetTile(sutf, sutf.point);
      surfaces.Add(sutf);
    }

    public List<ProjectileFightItem> AddSmoke(LivingEntity abilityUser, int range, int durab)
    {
      List<Tile> sides = new List<Tile>();
      var neibs = GetNeibsForLayer(abilityUser, range, SurfaceKind.Unset);//, ref sides);

      var smokes = new List<ProjectileFightItem>();
      foreach (var emp in neibs)
      {
        var pfi = new ProjectileFightItem()
        {
          FightItemKind = FightItemKind.Smoke
        };
        pfi.Durability = 4 + durab;
        this.Layers.SetAt<ProjectileFightItem>(KnownLayer.Smoke, pfi, emp.point);
        smokes.Add(pfi);
      }

      return smokes;
    }

    public override bool SetTile
    (
      Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true,
      bool autoSetTileDungeonIndex = true, bool reportError = true
    )
    {
      //Logger.LogInfo("Adding tile "+ tile + " at "+ point);
      
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
          //TODO some UT needed it
          ls.SetLevel(Index + 1, Difficulty.Normal);//TODO Normal, pass Settings here
        }
      }

      if (tile is Hero)
      {
        var tileAtPoint = GetTile(point);
        if (tileAtPoint == tile)
          return true;
        if (tileAtPoint is Stairs)
        {
          DebugHelper.Assert(false);
        }
      }
      else if (tile is Loot loot)
      {
        if (Loot.ContainsKey(point))
        {
          if (Logger != null)
            Logger.LogError("loot already at point: " + Loot[point] + ", trying to add: " + tile + " point:" + point);
          DebugHelper.Assert(false);
          return false;
        }
        //Logger.LogInfo("Adding Loot "+ tile + " at "+ point + " Loot.Count:"+ Loot.Count);
        tile.point = point;
        var baseTile = base.GetTile(point);
        if (baseTile != null)
          loot.tag2 = baseTile.tag2;
        Loot[point] = loot;

        return true;
      }

      else if (tile is Surface sur)
      {
        var surfAtPoint = SurfaceSets.GetKind(sur.Kind);
        if (surfAtPoint.Tiles.ContainsKey(point))
        {
          //var alreadyAtPoint = surfAtPoint.Tiles[point];
          //if (alreadyAtPoint != sur.Kind &&
          //  (!alreadyAtPoint.ToString().Contains("Water") || !sur.ToString().Contains("Water"))//Both water are fine
          //  )
          //{
          //  if (Logger != null)
          //  {
          //    Logger.LogError("Surface already at point: " + Surfaces[point] + ", trying to add: " + tile + " point:" + point);
          //    DebugHelper.Assert(false);
          //    return false;
          //  }
          //}
        }
        //Logger.LogInfo("Adding Loot "+ tile + " at "+ point + " Loot.Count:"+ Loot.Count);
        tile.point = point;
        SurfaceSets.SetAt(point, sur);
        if(sur is HitableSurface hs && hs.Kind == SurfaceKind.Oil)
          HookStartBurning(hs);
        return true;
      }

      Point? prevPos = tile?.point;
      var res = base.SetTile(tile, point, resetOldTile, revealReseted, autoSetTileDungeonIndex, reportError);
      if (res && tile is LivingEntity && prevPos != null)
      {
        (tile as LivingEntity).PrevPoint = prevPos.Value;
      }
      return res;
    }

    private void HookStartBurning(HitableSurface sutf)
    {
      sutf.StartedBurning += (s, o) =>
      {
        var surfs = GetNeighborTiles(sutf, true);
        foreach (var neibSur in surfs)
        {
          var hs = SurfaceSets.GetKind(SurfaceKind.Oil).GetAt(neibSur.point) as HitableSurface;
          if (hs != null && !hs.IsBurning)
          {
            hs.StartBurning();
            if (neibSur is LivingEntity le)
            {
              le.AddFiringFromOil();
            }
          }
        }
      };
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
        var neibs = GetNeighborTiles(door).Where(i=> i!=null).ToList();
        Tile neib = FindTileToRevealRoom(door, hero);

        if (neib == null)
        {
          foreach (var n in neibs)
          {
            neib = FindTileToRevealRoom(n, hero);
            if (neib != null)
              break;
          }
        }

        if (neib != null)
        {
          //var parts = Parts;
          GetNodeFromTile(neib).Reveal(true);
        }
        else
          Container.GetInstance<ILogger>().LogError("neib == null " + door);
      }
      door.Opened = true;
      return true;
    }

    private Tile FindTileToRevealRoom(Tile start, Hero hero)
    {
      try
      {
        var neibs = GetNeighborTiles(start);

        var neib = neibs.Where(i =>
        i != null &&
        i.DungeonNodeIndex != start.DungeonNodeIndex &&
        i != hero &&
        i.DungeonNodeIndex != Dungeons.TileContainers.DungeonNode.DefaultNodeIndex &&
        !GetNodeFromTile(i).Revealed
        ).FirstOrDefault();

        return neib;
      }
      catch (Exception ex)
      {
        Container.GetInstance<ILogger>().LogError("FindTileToRevealRoom ex: " + ex);
      }
      return null;
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
        .Where(
        i => i.Revealed 
        && i.DistanceFrom(tile.point) < 3
        && i.IsCollectable
        ).ToList();
      return res;
    }

    public Tiles.Loot GetLootTile(Point point)
    {
      if (Loot.ContainsKey(point))
        return Loot[point];

      return null;
    }

    private byte[,] InitMatrixBeforePathSearch
    (
      Point from, 
      Point end, 
      bool forHeroAlly, 
      bool canGoOverCrackedStone, 
      bool forEnemyProjectile,
      LivingEntity movingEntity,
      Dictionary<Point, Tile> excludedFromPathFind
    )
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
          var currPt = new Point(col, row);
          var tile = Tiles[row, col];
          if(currPt == end)
            tile = GetTile(currPt);//otherwise hero is not moving when clicking on loot in dungeon
          if (tile is Hero)
          {
            if (forHeroAlly)
              findPathMatrix[row, col] = 0;
            continue;
          }
          else if (tile is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.HunterTrap && pfi.FightItemState == FightItemState.Activated
            && movingEntity != null)
          {
            if (movingEntity.ShallAvoidTrap())
              value = 0;
          }
          else if (tile is IDoor door)
          {
            if (/*&& !EnemyCanPassDoors*/ !door.Opened)
              value = 0;
          }
          else if (tile is IObstacle)
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
          else if (excludedFromPathFind.ContainsKey(tile.point))
            value = 0;
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
                if (tile is Enemy)
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
        //Logger.LogInfo("SetTile done for " + toUse);
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

    Dictionary<Point, Tile> excludedFromPathFind;

    public List<Algorithms.PathFinderNode> FindPath(Point from, Point endPoint, bool forHeroAlly, bool canGoOverCrackedStone,
      bool forEnemyProjectile, LivingEntity movingEntity)
    {
      //Commons.TimeTracker tr = new Commons.TimeTracker();
      if (excludedFromPathFind == null)
      {
        excludedFromPathFind = new Dictionary<Point, Tile>();
        foreach (var lava in SurfaceSets.GetKind(SurfaceKind.Lava).Tiles.Values)
        {
          excludedFromPathFind.Add(lava.point, lava);
        }
      }
      var startPoint = new Algorithms.Point(from.Y, from.X);
      var findPathMatrix = InitMatrixBeforePathSearch(from, endPoint, forHeroAlly, canGoOverCrackedStone, forEnemyProjectile, 
        movingEntity, excludedFromPathFind);

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
      return GetAllStairs(StairsKind.PitDown).Where(i => i.PitName == pitName).SingleOrDefault();
    }

    public Enemy SpawnEnemy(ILootSource lootSource)
    {
      return SpawnEnemy(lootSource.Level);
    }

    public Enemy SpawnEnemy(int level, Difficulty? difficulty = null)
    {
      var enemy = Enemy.Spawn(EnemySymbols.SkeletonSymbol, level, Container, difficulty);
      enemy.Container = Container;
      return enemy;
    }

    public List<IApproachableByHero> ApproachableByHero { get; set; } = new List<IApproachableByHero>();

    [JsonIgnore]
    public bool Inited { get; set; }
    public bool SupportMiniAnimals { get; set; } = true;
    public virtual bool BossLeversSolved()
    {
      return false;
    }

    public void EnsureRevealed(int nodeIndex)
    {
      var notRev = this.GetTiles().Where(i => i.DungeonNodeIndex == nodeIndex && !i.Revealed).ToList();
      if (notRev.Any())
      {
        notRev.ForEach(i => i.Revealed = true);
      }
    }

    public Tile GetHeroStartTile()
    {
      Tile heroStartTile;
      var empOnes = GetEmptyTiles(nodeIndexMustMatch: false).Where(i => i.DungeonNodeIndex > Dungeons.TileContainers.DungeonNode.ChildIslandNodeIndex);
      var secret = Nodes.Where(i => i.Secret).FirstOrDefault();
      if (secret != null)
      {
        empOnes = empOnes.Where(i => i.DungeonNodeIndex != secret.NodeIndex).ToList();
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

    public bool PlaceHeroAtTile(GameContextSwitchKind context, Hero hero, Tile tile)
    {
      ClearOldHeroPosition(context);
      if (SetTile(hero, tile.point, false))
      {
        hero.DungeonNodeIndex = tile.DungeonNodeIndex;
        Logger.LogInfo("PlaceHeroAtTile ok" + tile.point+  " hero: " + GetTiles<Hero>().FirstOrDefault() + " this: "+this);
        return true;
      }
      else
      {
        Logger.LogError("PlaceHeroAtTile failed at: "+tile.point);
        DebugHelper.Assert(false);
      }
      return false;
    }

    public HeroPlacementResult PlaceHeroNextToTile(GameContextSwitchKind context, Hero hero, Tile baseTile)
    {
      var res = new HeroPlacementResult();
      Tile heroStartTile = null;

      try
      {
        if (baseTile != null)
        {
          var emptyOne = GetClosestEmpty(baseTile); //GetEmptyNeighborhoodTiles(baseTile);
          if (emptyOne != null)
            heroStartTile = emptyOne;
          else
            heroStartTile = GetClosestEmpty(baseTile, false);
        }
      }
      catch (Exception exc)
      {
        Logger.LogError(exc);
      }

      if (heroStartTile == null)
      {
        heroStartTile = GetHeroStartTile();
      }

      if (PlaceHeroAtTile(context, hero, heroStartTile))
      {
        res.Point = heroStartTile.point;
        res.Node = this;
      }
      return res;
    }

    public List<SurfaceKind> GetSurfaceKindsUnderLivingEntity(LivingEntity hero)
    {
      return GetSurfaceKindsUnderTile(hero);
    }

    public List<SurfaceKind> GetSurfaceKindsUnderTile(Tile tile)
    {
      return GetSurfaceKindsUnderPoint(tile.point);
    }

    public List<Surface> GetSurfacesUnderTile(Tile tile)
    {
      return GetSurfacesUnderPoint(tile.point);
    }

    public List<Surface> GetSurfacesUnderPoint(Point point)
    {
      return SurfaceSets.GetAt(point);
    }

    public List<SurfaceKind>  GetSurfaceKindsUnderPoint(Point point)
    {
      return GetSurfacesUnderPoint(point).Select(i=>i.Kind).ToList();
    }

    override public Tuple<Point, TileNeighborhood> GetEmptyNeighborhoodPoint
    (
      Tile target,
      TileNeighborhood? prefferedSide = null,
      List<Type> extraTypesConsideredEmpty = null
    )
    {
      return base.GetEmptyNeighborhoodPoint(target, prefferedSide, extraTypesConsideredEmpty);
    }

    List<LivingEntity> deadOnes = new List<LivingEntity>();
    public List<Enemy> GetDeadEnemies()
    {
      var de = deadOnes.Where(i => i is Enemy).Cast<Enemy>().ToList();
      return de;
    }

    public void AppendDead(LivingEntity dead)
    {
      deadOnes.Add(dead);
    }

    public void RemoveDead(LivingEntity dead)
    {
      deadOnes.Remove(dead);
    }

    public override bool IsTileEmpty(Tile tile, EmptyCheckContext emptyCheckContext)
    {
      var emp = base.IsTileEmpty(tile, emptyCheckContext);

      if (tile != null && emptyCheckContext == EmptyCheckContext.DropLoot)
      {
        var t1 = GetTile(tile.point);
        if (t1 is Loot)
          return false;
        if (tile is LivingEntity)
          return true;
      }
      return emp;
    }

    public virtual Roguelike.Tiles.Interactive.InteractiveTile GetCamp()
    {
      return null;
    }

    public virtual Dungeons.Tiles.Tile GetEmptyNextToCamp()
    {
      return null;
    }

    public override List<Tile> GetAllTiles()
    {
      var tiles = base.GetAllTiles();
      tiles.AddRange(this.Loot.Values.ToList());
      return tiles;
    }

    public virtual void OnLoadDone()
    { 
    }

    public bool IsAtSmoke(Dungeons.Tiles.Tile tile)
    {
      return Layers.ContainsAt(KnownLayer.Smoke, tile.point);
    }

    protected virtual bool RevealOilOnGeneration()
    {
      return false;
    }

    public void GenerateOilSpread(int oilSpreadsCount, List<Dungeons.Tiles.Tile> emptyTilesToUse = null)
    {
      var gl = this;
      int oilSpreads = oilSpreadsCount;// ;
      for (int i = 0; i < oilSpreads; i++)
      {
        var emptyOnes = emptyTilesToUse ?? gl.GetEmptyTiles();
        for (int attempt = 0; attempt < 10; attempt++)
        {
          var randTile = gl.GetRandomEmptyTile(emptyOnes);
          if (gl.GetEmptyNeighborhoodTiles(randTile).Count > 1)
          {
            gl.SpreadOil(randTile, emptyTilesToUse: emptyOnes, reveal: RevealOilOnGeneration());
            break;
          }
        }
      }
    }

    public void RevealAllNodes()
    {
      this.Nodes.ForEach(i => {
        i.Reveal(true, true);
        i.ChildIslands.ForEach(i => i.Reveal(true, true));
        }
      );
    }
  }
}
