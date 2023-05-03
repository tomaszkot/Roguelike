using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using Newtonsoft.Json;
using OuaDII.Generators;
using OuaDII.Tiles.Interactive;
using Roguelike;
using Roguelike.Serialization;
using Roguelike.Settings;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace OuaDII.TileContainers
{
  public class DynamicLevel<T>
  {
    public T Dungeon { get; set; }
    public List<Tile> DynamicTiles { get; set; }
  }

  public class WorldSpecialTiles
  {
    public List<GroundPortal> GroundPortals { get; set; } = new List<GroundPortal>();
    public List<GodGatheringSlot> GodGatheringSlots = new List<GodGatheringSlot>();
    public List<Tile> Other = new List<Tile>();

    public void Append(WorldSpecialTiles other)
    {
      GroundPortals.AddRange(other.GroundPortals);
      if(other.GodGatheringSlots.Any())
        GodGatheringSlots.AddRange(other.GodGatheringSlots);
      Other.AddRange(other.Other);
    }
  }

  //a giant node like 500x500 tiles
  public class World : AbstractGameLevel, IPersistable
  {
    public override string Name { get; set; } = "";
    public WorldSpecialTiles WorldSpecialTiles { get; set; } = new WorldSpecialTiles();

    [JsonIgnore]//saved by GameInfo
    public List<DungeonPit> Pits { get; set; } = new List<DungeonPit>();

    List<Stairs> pitsStairs = new List<Stairs>();
    const int defaultWidth = 15;
    const int defaultHeight = defaultWidth;

    //'World' must be a non-abstract type with a public parameterless constructor in order to use it as parameter 'T' in the generic type or method
    public World() : this(ContainerConfigurator.LastOne)
    {

    }

    public World(Container container) : base(container)
    {
      //AddStairsWithPit must be used to create  a pit
      //EnsurePits();
      Name = "Outer World";
      SupportMiniAnimals = false;
    }

    public Generators.LevelGenerator CreatePitGenerator(DungeonPit pit)
    {
      return new OuaDII.Generators.LevelGenerator(Container, pit.Name, pit.QuestKind, pit.StartEnemiesLevel, Logger);
    }

    public void EnsurePitsGenerators()
    {
      var pitStairs = GetAllStairs(StairsKind.PitDown).ToList();
      foreach (var pitStair in pitStairs)
      {
        var pit = GetPit(pitStair);
        if (pit.Name == OuaDII.Managers.GameManager.GameOnePitDown)
          continue;

        if (pit.LevelGenerator == null)
        {
          var gen = new OuaDII.Generators.LevelGenerator(Container, pit.Name, pit.QuestKind, pit.StartEnemiesLevel, Logger);
          pit.LevelGenerator = gen;
        }
      }
    }

    public override string ToString()
    {
      return GetType().Name + " " + "[" + Width + ":" + Height + "]" + +GetHashCode();// + " " + base.ToString();
    }

    public DungeonPit GetPit(string pitName)
    {
      //AddStairsWithPit must be used to create  a pit
      //var pit = EnsurePit(pitName);
      return Pits.Where(i => i.Name == pitName).FirstOrDefault();
    }

    public DungeonPit GetPit(Stairs stairs)
    {
      return Pits.Where(i => i.Name == stairs.PitName).FirstOrDefault();
    }

    public DungeonPit AddPit(string pitName)
    {
      var existingPit = Pits.FirstOrDefault(i => i.Name == pitName);
      if (existingPit != null)
      {
        Logger.LogError("existingPit != null " + pitName + ", creating uniq name.... ");
        pitName += Guid.NewGuid();
      }

      DungeonPit pit = new DungeonPit();
      pit.Name = pitName;
      Pits.Add(pit);
      return pit;
    }
        
    public override List<Dungeons.TileContainers.DungeonNode> GeneratorNodes
    {
      get { return null; }
    }

    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true, bool autoSetTileDungeonIndex = true,
      bool reportError = true)
    {
      var tileAtPoint = GetTile(point);
      if (tileAtPoint is GroundPortal)
      {
        return true;
      }
      if (tile != null)
        tile.Revealed = true;//whole world is revealed

      if (tile is GroundPortal)
      {
        tile.point = point;//TODO tk
        if (!WorldSpecialTiles.GroundPortals.Contains(tile))
          WorldSpecialTiles.GroundPortals.Add(tile as GroundPortal);
        return true;
      }


      var set = base.SetTile(tile, point, resetOldTile, revealReseted, autoSetTileDungeonIndex, reportError);
      return set;
    }

    /// <summary>
    /// Creates a new world by merging worldStatic with this instance's dynamic tiles
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="worldStatic"></param>
    /// <returns></returns>
    public DynamicLevel<World> Merge(World worldStatic)
    {
      Logger.LogInfo("World Merge, worldStatic size: " + worldStatic.Width + ":" + worldStatic.Height);
      return CreateDynamicWorld<World>(this, worldStatic, Container, Logger);
    }

    public static DynamicLevel<T> CreateDynamicWorld<T>
    (
      DungeonNode dynamic,
      DungeonNode worldStatic,
      Container container,
      ILogger logger
    )
    where T : Dungeons.TileContainers.DungeonNode
    {
      var destNode = Activator.CreateInstance(typeof(T), new object[] { container }) as T;

      if (worldStatic == null)//roguelike mode?
      {
        worldStatic = new World();
        worldStatic.Create(dynamic.Width, dynamic.Height);
        var staticOnes = dynamic.GetTiles().Where(i => !i.IsDynamic()).ToList();
        staticOnes.ForEach(i => worldStatic.SetTile(i, i.point));
      }
      var gi = new OuaDII.Generators.GenerationInfo();

      destNode.Create(worldStatic.Width, worldStatic.Height, generateContent: false);

      //otherwise barrel in StonesMine are not visible 
      destNode.NodeIndex = dynamic.NodeIndex;

      var allowNulls =  gi.allowNulls;
      if (!(dynamic is World))//otherwise there were tiles outside smith room
      {
        allowNulls = true;
      }
      destNode.AppendMaze(worldStatic, new Point(0, 0), allowNulls: allowNulls);
      destNode.RestoreBkg = worldStatic.RestoreBkg;
      var dynTiles = new List<Tile>();
      dynamic.DoGridAction((int col, int row) =>
      {
        var tile = dynamic.GetTile(new Point(col, row));
        if (tile.IsDynamic())
        {
          dynTiles.Add(tile);
        }
      });

      var dt = dynTiles.Count;

      foreach (var tile in dynTiles)
      {
        if (!destNode.SetTile(tile, tile.point))
        {
          logger.LogError("CreateDynamicWorld failed to set tile " + tile);
        }
      }
      return new DynamicLevel<T>() { Dungeon = destNode, DynamicTiles = dynTiles };
    }

    public Stairs CreateStairsWithPit(string pitName)
    {
      var stairs = new Stairs(Container, StairsKind.PitDown);
      var pit = AddPit(pitName);

      stairs.PitName = pit.Name;

      return stairs;
    }

    public Stairs AddStairsWithPit(string pitName, Point point)
    {
      var stairs = CreateStairsWithPit(pitName);
      SetTile(stairs, point);
      return stairs;
    }

    public override string Description
    {
      get
      {
        var desc = GetType() + " " + Name + " " + GetHashCode();

        return desc;

      }
    }

    public bool Dirty { get; set; }

    public Stairs GetSavedPitStairs(Roguelike.State.GameState gs)
    {
      Stairs pitStairs = null;
      pitStairs = GetAllStairs(StairsKind.PitDown).Where(i => GetPit(i).Name == gs.HeroPath.Pit).SingleOrDefault();
      return pitStairs;
    }

    internal AbstractGameLevel PlaceLoadedHero(Hero hero, Roguelike.State.GameState gs)
    {
      AbstractGameLevel node = this;
      Tile tile = null;

      if (Options.Instance.Serialization.RestoreHeroToDungeon &&
          !string.IsNullOrEmpty(gs.HeroPath.Pit)
          && Options.Instance.Serialization.RestoreHeroToSafePointAfterLoad//if false Hero will be exactly at save place (potentially dangerous)
          )
      {
        var pit = Pits.Where(i => i.Name == gs.HeroPath.Pit).SingleOrDefault();

        if (pit.Levels.Count > gs.HeroPath.LevelIndex)
        {
          node = pit.Levels[gs.HeroPath.LevelIndex];
          var stairs = node.GetStairs(StairsKind.LevelUp);
          if (stairs == null)
            stairs = node.GetStairs(StairsKind.PitUp);
          if (stairs != null)
          {
            tile = node.GetClosestEmpty(stairs, false);
            if (tile == null)
              node = this;
          }
        }
        else
        {
          Logger.LogError(pit + " pit.Levels.Count: " + pit.Levels.Count + ", but gs.HeroPathValue.LevelIndex: " + gs.HeroPath.LevelIndex);
        }
      }

      if (node == this)
      {
        if (Options.Instance.Serialization.RestoreHeroToSafePointAfterLoad)
        {
          if (gs.HeroInitGamePosition != new Point().Invalid())
          {
            tile = node.GetTile(gs.HeroInitGamePosition);
            if (tile == null && node is World world)
            {
              var groundPortals = world.WorldSpecialTiles.GroundPortals;
              var camp = groundPortals.FirstOrDefault(i => i.GroundPortalKind == GroundPortalKind.Camp);
              if(camp!=null)
                tile = node.GetClosestEmpty(camp);
            }
          }
        }
      }
      
      node.PlaceHeroAtTile(GameContextSwitchKind.GameLoaded, hero, tile);
      return node;

    }

    public override bool RevealRoom(Roguelike.Tiles.Interactive.Door door, Roguelike.Tiles.LivingEntities.Hero hero)
    {
      door.Opened = true;
      return true;
    }

    public override void Create(int width = 10, int height = 10, Dungeons.GenerationInfo info = null, int nodeIndex = 999, DungeonNode parent = null, bool generateContent = true)
    {
      base.Create(width, height, info, nodeIndex, parent, generateContent);
    }

    public void SetEntitiesLevel(Hero hero, List<Enemy> enemies, Generators.TilesGenerator tilesGenerator)
    {
      SetPitsStartLevel(hero);

      tilesGenerator.SetILootSourceLevel(this, hero);
      var pits = GetAllPits();
      foreach (var key in this.HiddenTiles.GetKeys())
      {
        var hiddenOnes = this.HiddenTiles.Get(key).Tiles.Where(i => i is ILootSource).Cast<ILootSource>().ToList();
        hiddenOnes.ForEach(i =>
        {
          if (i is Enemy en)
            en.Container = this.Container;//TODO!
          SetTileStats(i, hero, pits);
        });
      }

      CreateWorldChempions(hero, enemies);

      CreateWounded(hero, enemies);
      //if(enemiesClose.Count() == 1)
      //  enemiesClose.ElementAt(0).SetIsWounded(true);
    }

    private static void CreateWorldChempions(Hero hero, List<Enemy> enemies)
    {
      var maxEnemies = enemies.Count;
      var enemiesFar = enemies.Where(i => i.DistanceFrom(hero) > TilesGenerator.CampRadius+10).ToList();
      var champsMaxCount = enemiesFar.Count / 3;
      var champsCount = 0;
      for (int i = 0; i < champsMaxCount; i++)
      {
        var en = RandHelper.GetRandomElem<Enemy>(enemiesFar);
        if (en.tag1.Contains("tree_monster"))
          continue;
        if (en.PowerKind == EnemyPowerKind.Plain && !en.Herd.Any())
        {
          en.SetNonPlain(false, true);
          enemiesFar.Remove(en);
          champsCount++;
          if (champsCount > maxEnemies / 10)
            break;
        }
      }
    }

    //private static void TempFakeEnemyLevelInc(Hero hero, List<Enemy> enemies)
    //{
    //  var enemiesFarWeak = enemies.Where(i => i.DistanceFrom(hero) > TilesGenerator.CampRadius+10 && i.Level == 1).ToList();//8
    //  foreach (var en in enemiesFarWeak)
    //  {
    //    if (en.StatsIncreased.ContainsKey(IncreaseStatsKind.Level))
    //      en.StatsIncreased[IncreaseStatsKind.Level] = false;
    //    if (en.StatsIncreased.ContainsKey(IncreaseStatsKind.Difficulty))//TODO
    //      en.StatsIncreased[IncreaseStatsKind.Difficulty] = false;
    //    en.SetLevel(2);
    //  }
    //}

    private void CreateWounded(Hero hero, List<Enemy> enemies)
    {
      var enemiesClose = enemies.OrderBy(i => i.DistanceFrom(hero)).Take(10);
      foreach (var en in enemiesClose)
      {
        if (RandHelper.GetRandomDouble() > 0.4 &&
          en.tag1 != "demon"//TODO
           )
          en.SetIsWounded(true);
      }
    }

    public void SetPitsStartLevel(Hero hero)
    {
      var pitStairs = GetAllStairs(StairsKind.PitDown).OrderBy(i => i.DistanceFrom(hero)).ToList();
      int startEnemiesLevel = 1;
      foreach (var pitStair in pitStairs)
      {
        if (pitStair.tag1 == OuaDII.Managers.GameManager.GameOnePitDown)
          continue;
        var pit = GetPit(pitStair);
        
        pit.StartEnemiesLevel = startEnemiesLevel;
        pit.LevelGenerator = CreatePitGenerator(pit);
        startEnemiesLevel += LevelGenerator.GetMaxLevelIndex(pit);

        //var log = "pit: " + pit.Name + ", assigned startEnemiesLevel: " + startEnemiesLevel;
        //log += " end Level:" + startEnemiesLevel;
        //Logger.LogInfo(log);
      }
    }

    public List<Stairs> GetAllPits()
    {
      return GetAllStairs(Roguelike.Tiles.Interactive.StairsKind.PitDown).Where(i => i.tag1 != OuaDII.Managers.GameManager.GameOnePitDown).ToList();
    }

    public void SetTileStats(ILootSource lootSrc, Hero hero, List<Stairs> allPitStairs)//lootSrc enemy or interactive
    {
      var tile = lootSrc as Tile;

      if (hero != null)
      {
        var distFromHero = tile.DistanceFrom(hero);
        if (distFromHero < TilesGenerator.CampRadius + 7)
        { 
          lootSrc.SetLevel(1);
          //Logger.LogInfo("set lev 1  for "+ lootSrc);
          return;
        }
        if (distFromHero < TilesGenerator.CampRadius + 13)
        {
          lootSrc.SetLevel(2);
          //Logger.LogInfo("set lev 1  for "+ lootSrc);
          return;
        }
      }

      var pitStairs = allPitStairs;
      if (pitStairs.Any())
      {
        var minDist = pitStairs.Min(i => i.DistanceFrom(tile));
        var stairs = pitStairs.Where(i => i.DistanceFrom(tile) == minDist).First();
        var pit = Pits.Where(i => i.Name == stairs.PitName).Single();

        if (pit.StartEnemiesLevel == 7)
        {
          int k = 0;
          k++;
        }
        lootSrc.SetLevel(pit.StartEnemiesLevel);
      }
      else
      {
#if UNITY_EDITOR
        lootSrc.SetLevel(1);
#endif
      }
    }

    public override List<Tile> GetEmptyTiles
    (
      GenerationConstraints constraints = null,
      bool canBeNextToDoors = true,
      bool levelIndexMustMatch = false//allows skipping childIsland tiles,
      , EmptyCheckContext emptyCheckContext = EmptyCheckContext.Unset
    )
    {
      //TODO slow - better not use it!
      return base.GetEmptyTiles(constraints, canBeNextToDoors, levelIndexMustMatch, emptyCheckContext);

    }

    public override Tile GetClosestEmpty(Tile baseTile, bool sameNodeId = false, List<Tile> skip = null, bool incDiag = true,
      Func<Tile, bool> canBeUsed = null)
    {
      return base.GetClosestEmpty(baseTile, sameNodeId, skip, incDiag, canBeUsed);
    }

    protected override Tile GetClosestEmptyLastChance(Tile baseTile, bool sameNodeId, List<Tile> skip)
    {
      Log("GetClosestEmptyLastChance - failed to find empty fast way in the World!", true);
      return null;//too slow for a world!
    }

    protected override void ReportInvalidPoint(Tile tile, Point point)
    {
      if (tile is Wall && (point.X < 0 || point.Y < 0))
        Logger.LogError("SetTile failed for point: " + point + " tile: " + tile, false);
      else
        base.ReportInvalidPoint(tile, point);
    }

    [JsonIgnore]
    public Generators.TilesGenerator TilesGenerator { get; set; }

    public WorldDynamicTiles GenerateDynamicTiles(Roguelike.Managers.GameManager gm, Roguelike.Tiles.LivingEntities.Hero hero, OuaDII.Generators.GenerationInfo gi)
    {
      TilesGenerator = new TilesGenerator(Container, this, hero);
      TilesGenerator.GenerateDynamicTiles(gm, gi);

      return TilesGenerator.NewGameDynamicTiles;
    }

    public override Dungeons.Tiles.Tile GetEmptyNextToCamp()
    {
      var camp = GetCamp();
      var skip = new List<Tile>();
      skip.Add(GetTile(camp.point));
      return GetClosestEmpty(camp, false, skip);
    }

    public override Roguelike.Tiles.Interactive.InteractiveTile GetCamp()
    {
      var groundPortals = WorldSpecialTiles.GroundPortals;
      var camp = groundPortals.FirstOrDefault(i => i.GroundPortalKind == GroundPortalKind.Camp);
      return camp;
    }

    protected override bool RevealOilOnGeneration()
    {
      return true;
    }
  }
}
