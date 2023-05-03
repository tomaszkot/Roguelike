using Dungeons.Core;
using Dungeons.TileContainers;
using OuaDII.TileContainers;
using Roguelike.Abilities;
using Roguelike.Generators;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
//using UnityEngine;
using static Dungeons.TileContainers.DungeonNode;

namespace OuaDII.Generators
{
  
  public class TilesGenerator : BaseTilesGenerator
  {
    public const int CampRadius = 15;//TODO 

    List<string> enemyNames = new List<string>
      {
      "spider",
      "skeleton",
      "snake",
      "bear",
      "boar",
      "lynx",
      "bandit1",
      "druid",
      "wolf",
      "wolverine"
      };

    Container container;
    World node;
    Hero hero;
    public TilesGenerator(Container container, Dungeons.TileContainers.DungeonNode node, Hero hero) : base(container)
    {
      this.container = container;
      this.node = node as TileContainers.World;

      this.hero = hero;
    }

    public List<A> Generate2<A, B>(int max, List<Dungeons.Tiles.Tile> emptyTiles)
      where A : Dungeons.Tiles.Tile
      where B : Dungeons.Tiles.Tile, new()
    {
      var res = new List<A>();
      var b = new B();
      return res;
    }

    public List<T> AddToNode<T>(int max, List<Dungeons.Tiles.Tile> emptyTiles) where T : Dungeons.Tiles.Tile
    {
      var res = new List<T>();

      List<string> filteredEnemyNames = null;
      bool enemies = false;
      if (typeof(T) == typeof(Enemy) || typeof(T).IsSubclassOf(typeof(Enemy)))
      {
        emptyTiles = emptyTiles.Where(i => i.DistanceFrom(hero) > CampRadius).ToList();
        filteredEnemyNames = Filter(enemyNames);
        enemies = true;
      }
      var rand = RandHelper.Random;
      var abl = node as AbstractGameLevel;
      if (gm.GameState.CoreInfo.Demo)
      {
        max = max / 8;
      }
      for (int i = 0; i < max; i++)
      {
        var emp = emptyTiles[rand.Next(emptyTiles.Count)];

        if (emp == null)
        {
          container.GetInstance<ILogger>().LogError(" Generate<T> out of empties!");
          break;
        }
        
        T tile = null;
        if (enemies)
          tile = CreateEnemyInstance(container, RandHelper.GetRandomElem(filteredEnemyNames), true, false) as T;
        else
          tile = container.GetInstance<T>();

        //set it inside the node
        if (node.SetTile(tile, emp.point))
        {
          res.Add(tile);
          if (tile is Plant plant)
          {
            if (RandHelper.GetRandomDouble() > 0.7)
              plant.SetKind(PlantKind.Thistle);
            else
              plant.SetKind(PlantKind.Sorrel);
          }

          else if (tile is Mushroom mush)
          {
            mush.MushroomKind = RandHelper.GetRandomEnumValue<MushroomKind>();

          }
          else if (tile is Animal anim)
          {
            anim.tag1 = "deer";
            anim.AnimalKind = AnimalKind.Deer;
          }
          else if (tile is Food food)
          {
            if (RandHelper.GetRandomDouble() > 0.6)
              food.Kind = FoodKind.Apple;
            else
              food.Kind = FoodKind.Plum;
          }
          else if (tile is Barrel bar)
          {
            bar.BarrelKind = RandHelper.GetRandomEnumValue<BarrelKind>();
            if (bar.BarrelKind == BarrelKind.OilBarrel)
              bar.BarrelKind = BarrelKind.Barrel;
          }
          else if (tile is Chest chest)
          {
            if (RandHelper.GetRandomDouble() > 0.5)
              chest.tag1 = "chest_plain2";
          }

          emptyTiles.Remove(emp);
          allEmpty.Remove(emp);
          if (string.IsNullOrEmpty(tile.tag1))
          {
            container.GetInstance<Logger>().LogError("string.IsNullOrEmpty(tile.tag1)! " + tile);
            tile.tag1 = "spider";
          }
        }
      }

      return res;
    }

    protected override List<string> Filter(List<string> enemyNames)
    {
      return enemyNames;
    }


    protected override void SetILootSourceLevel(ILootSource src, Roguelike.Tiles.LivingEntities.Hero hero)
    {
      if(!allPitStairs.Any())
        allPitStairs = node.GetAllPits();
      node.SetTileStats(src, hero, allPitStairs);
    }

    List<Stairs> allPitStairs = new List<Stairs>();
    public WorldDynamicTiles NewGameDynamicTiles { get; private set; }
    public GenerationInfo GenerationInfo { get => generationInfo; set => generationInfo = value; }

    Roguelike.Managers.GameManager gm;
    OuaDII.Generators.GenerationInfo generationInfo;

    public WorldDynamicTiles GenerateDynamicTiles(Roguelike.Managers.GameManager gm, OuaDII.Generators.GenerationInfo gi)
    {

      if (gi == null)
        gi = new OuaDII.Generators.GenerationInfo();

      GenerationInfo = gi;
      bool testLevel = node.Width < 44;
      if (testLevel)
        GenerationInfo.Counts.SetMin();


      this.gm = gm;
     
      var tilesGenerator = this;

      var newGameDynamicTiles = new WorldDynamicTiles();

      List<Dungeons.Tiles.Tile> campFreeOnes;
      var emptyTiles = GetEmptyTiles(EmptyCheckContext.DropLoot, out campFreeOnes);
      AppendDynamicTiles(gm, gi, testLevel, tilesGenerator, newGameDynamicTiles, emptyTiles, campFreeOnes);

      //GenerateStaticColliders(newGameDynamicTiles, emptyTiles);
      
      //this must be at the end so that enemies are not placed outside the borders
      //newGameDynamicTiles.NullOverrides = tilesGenerator.Generate<Dungeons.Tiles.Tile>(-1, emptyTiles);

      NewGameDynamicTiles = newGameDynamicTiles;

      var enemies = node.GetTiles<Enemy>();
      node.SetEntitiesLevel(hero, enemies, tilesGenerator);

      SetHerds(node, enemies);

      EnsureNoNulls();

      return newGameDynamicTiles;
    }

    

    void AppendDynamicTiles(Roguelike.Managers.GameManager gm,
      GenerationInfo gi,
      bool testLevel, 
      TilesGenerator tilesGenerator, 
      WorldDynamicTiles newGameDynamicTiles,
      List<Dungeons.Tiles.Tile> emptyTiles,
      List<Dungeons.Tiles.Tile> campFreeOnes
    )
    {
      var tr = new TimeTracker();

      var animalsCount = 30;
      if (gm.GameState.CoreInfo.Demo)
      {
        animalsCount = 8;
        gi.Counts.OilFlowsCount = 2;
      }
      newGameDynamicTiles.Mushrooms = AddToNode<Mushroom>(gi.Counts.MushroomsCount, emptyTiles);
      newGameDynamicTiles.Food = AddToNode<Food>(gi.Counts.FoodCount, emptyTiles);
      newGameDynamicTiles.MagicDusts = AddToNode<MagicDust>(gi.Counts.MagicDustsCount, emptyTiles);
      newGameDynamicTiles.Plants = AddToNode<Plant>(gi.Counts.PlantCount, emptyTiles);
      newGameDynamicTiles.Animals = AddToNode<Animal>(animalsCount, emptyTiles);

      newGameDynamicTiles.Barrels = AddToNode<Barrel>(gi.Counts.WorldBarrelsCount, emptyTiles);
      newGameDynamicTiles.Chests = AddToNode<Chest>(gi.Counts.WorldChestsCount, emptyTiles);
      newGameDynamicTiles.DeadBodies = AddToNode<DeadBody>(gi.Counts.DeadBodiesCount, emptyTiles);

      //camp stuff!
      {
        campFreeOnes = campFreeOnes.Where(i => node.GetTile(i.point).IsEmpty).ToList();
        newGameDynamicTiles.Chests.AddRange(AddCampChests(gm, campFreeOnes, emptyTiles));
        newGameDynamicTiles.OtherLoot.AddRange(AddCampLoot(gm, campFreeOnes, emptyTiles));
      }
      //gi.Counts.WorldEnemiesCount = 0;
      //gi.Counts.WorldEnemiesPacksCount = 0;
      var ens = AddToNode<Enemy>(gi.Counts.WorldEnemiesCount, emptyTiles);

      var filteredEnemyNames = Filter(enemyNames);
      var worldEnemiesPacksCount = gi.Counts.WorldEnemiesPacksCount;

      if (gm.GameState.CoreInfo.Demo)
      {
        worldEnemiesPacksCount /= 8;
      }

      List<Barrel> oilBarrels = new List<Barrel>();
      if (gi.GenerateEnemies)
      {
        for (int i = 0; i < worldEnemiesPacksCount; i++)
        {
          var enName = RandHelper.GetRandomElem(filteredEnemyNames);
          var packEnemies = CreateEnemiesPack(gi, enName);
          RandHelper.GetRandomElem(packEnemies).SetNonPlain(false);

          ens.AddRange(packEnemies.Cast<Enemy>().ToList());

          var startTile = RandHelper.GetRandomElem(emptyTiles);
          bool triedAddOil = false;
          while (packEnemies.Any())
          {
            var en = packEnemies.First();
            en.Herd = enName + "s";

            node.SetTile(en, startTile.point);
            packEnemies.Remove(en);
            emptyTiles.Remove(startTile);

            if (!triedAddOil)
            {
              if (RandHelper.GetRandomDouble() > 0.85)
                AddOilBarrel(emptyTiles, oilBarrels, en);

              triedAddOil = true;
            }

            startTile = node.GetClosestEmpty(startTile);
          }
        }

        var ensClose = AddToNode<Enemy>(gi.Counts.WorldExtraEnemiesCloseToCampCount, emptyTiles);
        var extEn = new List<Enemy>();
        var emptyTilesClose = GetEmptyNearCenterTiles(emptyTiles).Where(i => i.DistanceFrom(hero) > CampRadius + 2).ToList();
        foreach (var enClose in ensClose)
        {
          AppendToNode(node, enClose, emptyTiles, emptyTilesClose);
        }

        ens.AddRange(ensClose);
        AddOilBarrel(emptyTiles, oilBarrels, ensClose.GetRandomElem());
      }
           

      newGameDynamicTiles.Barrels.AddRange(oilBarrels);

      Log("GenerateDynamicTiles, enemies Count " + ens.Count + " Counts.WorldEnemiesCount: " + gi.Counts.WorldEnemiesCount);

      newGameDynamicTiles.Enemies = ens;
      gm.EnemiesManager.SetEntities(ens.Cast<LivingEntity>().ToList());

      

      Log("[BOOT] AppendDynamicTiles " + tr.TotalSeconds + " ens: "+ newGameDynamicTiles.Enemies.Count);
    }

    private void AddOilBarrel(List<Dungeons.Tiles.Tile> emptyTiles, List<Barrel> oilBarrels, Enemy enemy)
    {
      if (enemy == null)
      {
        Log("[BOOT] AddOilBarrel enemy == null!");
        return;
      }
      var emp = node.GetClosestEmpty(enemy);
      if (emp != null)
      {
        var bars = AddToNode<Barrel>(1, emptyTiles);
        if (bars.Any())
        {
          var bar  = bars.Single();
          bar.BarrelKind = BarrelKind.OilBarrel;
          node.SetTile(bar, emp.point);
          oilBarrels.AddRange(bars);
        }
      }
    }

    private void Log(string log)
    { 
      container.GetInstance<ILogger>().LogInfo(log);
      
    }

    void AppendToNode(World node, Dungeons.Tiles.Tile tile, List<Dungeons.Tiles.Tile> emptyTiles, List<Dungeons.Tiles.Tile> emptyTilesClose)
    {
      var empty = RandHelper.GetRandomElem(emptyTilesClose);
      if (empty != null)
      {
        if (node.SetTile(tile, empty.point))
        {
          emptyTiles.Remove(empty);
          emptyTilesClose.Remove(empty);
        }
      }
    }

    private List<Chest> AddCampChests
    (
      Roguelike.Managers.GameManager gm,
      List<Dungeons.Tiles.Tile> emptyTilesClose,
      List<Dungeons.Tiles.Tile> emptyTiles
    )
    {
      var chests = new List<Chest>();

      for(int i=0;i<4;i++)
      {
        var tile = new Chest(node.Container);
        chests.Add(tile);
        AppendToNode(node, tile, emptyTiles, emptyTilesClose);
      }

      return chests;
    }

    private List<Roguelike.Tiles.Loot> AddCampLoot
    (
      Roguelike.Managers.GameManager gm,
      List<Dungeons.Tiles.Tile> emptyTilesClose,
      List<Dungeons.Tiles.Tile> emptyTiles
    )
    {
      var lootItems = new List<Roguelike.Tiles.Loot>();

      StackedLoot loot;
      loot = new ProjectileFightItem(FightItemKind.ThrowingKnife);
      AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);

      loot = new ProjectileFightItem(RandHelper.GetRandomDouble() > 0.5 ? FightItemKind.PoisonCocktail : FightItemKind.ExplosiveCocktail);
      AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);

      for (int i = 0; i < 2; i++)
      {
        loot = new ProjectileFightItem(FightItemKind.Stone);
        AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);
      }

      loot = new Food(FoodKind.Apple);
      AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);

      //Sorrel twice
      loot = new Plant(PlantKind.Sorrel);
      AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);
      loot = new Plant(PlantKind.Sorrel);
      AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);

      loot = new Hazel();
      AddLootToNode(emptyTilesClose, emptyTiles, lootItems, loot);

      lootItems.AddRange(AddScrolls(gm, emptyTiles));
      lootItems.AddRange(AddOtherLoot(gm, emptyTiles));

      return lootItems;
    }

    private void AddLootToNode(List<Dungeons.Tiles.Tile> emptyTilesClose, List<Dungeons.Tiles.Tile> emptyTiles, List<Loot> lootItems, StackedLoot loot)
    {
      loot.Count = RandHelper.GetRandomInt(4) + 1;
      AppendToNode(node, loot, emptyTiles, emptyTilesClose);
      lootItems.Add(loot);
    }

    private List<Roguelike.Tiles.Loot> AddScrolls
    (
      Roguelike.Managers.GameManager gm, 
      List<Dungeons.Tiles.Tile> emptyTiles
    )
    {
      var res = new List<Roguelike.Tiles.Loot>();
      var scrolls = new List<string>(){ "identify_scroll", "portal_scroll", "skeleton_scroll"};
      var scrollsGods = new[] { "dziewanna_scroll", "swarog_scroll", "swiatowit_scroll", "perun_scroll" };
      scrolls.Add(RandHelper.GetRandomElem(scrollsGods));

      var emptyTilesClose = GetEmptyNearCenterTiles(emptyTiles);
      foreach (var sc in scrolls)
      {
        var loot = gm.LootGenerator.GetLootByAsset(sc) as Scroll;
        res.Add(loot);
        if(loot.Kind == Roguelike.Spells.SpellKind.Identify)
          loot.Count += RandHelper.GetRandomInt(2) + 1;
        AppendToNode(node, loot, emptyTiles, emptyTilesClose);
      }

      return res;
    }

    private List<Roguelike.Tiles.Loot> AddOtherLoot
    (
      Roguelike.Managers.GameManager gm,
      List<Dungeons.Tiles.Tile> emptyTiles
    )
    {
      var res = new List<Roguelike.Tiles.Loot>();
      var lootNames = new[] { "craft_one_eq" };

      var emptyTilesClose = GetEmptyNearCenterTiles(emptyTiles);
      foreach (var sc in lootNames)
      {
        var loot = gm.LootGenerator.GetLootByAsset(sc);
        res.Add(loot);
        AppendToNode(node, loot, emptyTiles, emptyTilesClose);
      }

      for(int i=0;i<3;i++)
      {
        var loot = new MagicDust();
        res.Add(loot);
        AppendToNode(node, loot, emptyTiles, emptyTilesClose);
      }

      return res;
    }

    public List<Dungeons.Tiles.Wall> GenerateStaticColliders(List<Dungeons.Tiles.Tile> emptyTiles)
    {
      var tr = new TimeTracker();
      string[] tags = new string[] { "tree1", "tree2" };
      Dictionary<Point, Dungeons.Tiles.Tile> et = emptyTiles.ToDictionary(i=>i.point, i=>i);
      var staticColliders = GenerateTilesAtEmptyPlaces<Dungeons.Tiles.Wall>(tags: tags, emptyTiles: et);
      if (staticColliders.Count > 50)
      {
        for (int i = 0; i < staticColliders.Count / 20; i++)
        {
          var el = RandHelper.GetRandomElem(staticColliders);
          el.tag1 = "trunk1";
          var rn = RandHelper.GetRandomDouble();
          var mul = 2;
          if (rn < 0.1 * mul)
            el.tag1 = "trunk2";
          else if (rn < 0.2 * mul)
            el.tag1 = "trunk3";
          else if (rn < 0.3 * mul)
            el.tag1 = "trunk_cut1";
          else if (rn < 0.4 * mul)
            el.tag1 = "trunk_cut2";
          else if (rn < 0.45 * mul)
            el.tag1 = "barrel_broken";
          else if (rn < 0.5 * mul)
            el.tag1 = "wozek_broken";
        }
        var el1 = RandHelper.GetRandomElem(staticColliders);
        el1.tag1 = "wozek_broken";
      }
      staticColliders.ForEach(i => emptyTiles.RemoveAll(et=> et.point == i.point));
      //newGameDynamicTiles.StaticColliders = new List<Dungeons.Tiles.Wall>();
      Log("[BOOT] GenerateStaticColliders " + tr.TotalSeconds);
      return staticColliders;
    }

    List<Dungeons.Tiles.Tile> allEmpty;

    public List<Dungeons.Tiles.Tile> GetEmptyTiles(EmptyCheckContext emptyCheckContext, out List<Dungeons.Tiles.Tile> campTiles)
    {
      allEmpty = node.GetEmptyTiles(emptyCheckContext: emptyCheckContext)
              .Where(i => !node.GetSurfaceKindsUnderPoint(i.point).Any() &&
                      !node.WorldSpecialTiles.GroundPortals.Any(j=> j.point == i.point) &&
                      !node.WorldSpecialTiles.GodGatheringSlots.Any(j => j.point == i.point) &&
                      !node.WorldSpecialTiles.Other.Any(j => j.point == i.point)
                    ).ToList();

      campTiles = allEmpty.Where(i => i.DistanceFrom(hero) < CampRadius-2).ToList();//TODO
      allEmpty = allEmpty.Where(i => string.IsNullOrEmpty(i.mapName) && i.DistanceFrom(hero) > CampRadius).ToList();
      return allEmpty;
    }

    //List<Dungeons.Tiles.Tile> GetEmptyNearCenterTiles()
    //{
    //  return GetEmptyNearCenterTiles(GetEmptyTiles(emptyCheckContext: EmptyCheckContext.DropLoot));
    //}

    public List<Dungeons.Tiles.Tile> GetEmptyNearCenterTiles(List<Dungeons.Tiles.Tile> emptyOnes, int nearOffset = 10)
    {
      return emptyOnes.Where(i => i.DistanceFrom(hero) < CampRadius + nearOffset).ToList();
    }

    public static void AddHiddenEnemy(AbstractGameLevel node, List<Dungeons.Tiles.Tile> emptyTiles, Roguelike.Tiles.LivingEntities.Enemy enemy, 
      string herdName, string mapName)
    {
      var til = node.GetRandomEmptyTile(EmptyCheckContext.Unset);
      enemy.point = til.point;
      emptyTiles.Remove(til);

      AddHiddenEnemy(node, enemy, herdName, mapName);
      
    }

    public static void AddHiddenEnemy(AbstractGameLevel node, Roguelike.Tiles.LivingEntities.Enemy enemy,
      string herdName, string mapName)
    {
      enemy.Herd = herdName;
      var hidden = node.HiddenTiles.Ensure(mapName);
      //var til = node.GetRandomEmptyTile(EmptyCheckContext.Unset);
      //enemy.point = til.point;
      hidden.Add(enemy);
    }

    internal void EnsureNoNulls()
    {
      node.DoGridAction((int col, int row) => {
        if (node.Tiles[row, col] == null)
        {
          var pt = new Point(col, row);
          if (node.GetTileInner(pt) == null)
          {
            var ti = new Dungeons.Tiles.Tile();
            node.SetTile(ti, pt);
          }
        }
      });
    }

    private void SetHerds(World node, List<Enemy> allEnemies)
    {
      var chemps = allEnemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList();
      var plains = allEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList();
      foreach (var chemp in chemps)
      {
        var chempHerdMembers = GetHerdMembers(chemp, allEnemies);
        foreach (var chempHerdMember in chempHerdMembers)
        {
          chempHerdMember.Herd = chemp.Herd;
        }
      }
    }

    public List<Enemy> GetHerdMembers(Enemy chemp, List<Enemy> allEnemies)
    {
      var plains = allEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList();
      var chempHerdMembers = plains.Where(i => i.DistanceFrom(chemp) < 5).ToList();
      return chempHerdMembers;
    }

    public List<T> GenerateTilesAtEmptyPlaces<T>
    (
      Dictionary<Point, Dungeons.Tiles.Tile> emptyTiles,
      double randThreshold = 0.9f,
      string[] tags = null
    ) 
      where T :  Dungeons.Tiles.Tile, new()
    {
      var tr = new TimeTracker();
      var tag1 = "tree1";
      var res = new List<T>();
      
      var emptyOnes = emptyTiles;
      Log("[BOOT] GenerateTilesAtEmptyPlaces " + typeof(T) +" emptyOnes.Count: " + emptyOnes.Count);
      var tagsList = new List<string>();
      if(tags!=null)
        tagsList = tags.ToList();
      if (!tagsList.Any())
        tagsList.Add(tag1);
      var changeTagCounter = emptyOnes.Count / 10;

      int countSkipped1 = 0;
      int counter = 0;
      int tagIndex = 0;
      tag1 = tagsList[tagIndex];
      Dictionary<Point, Dungeons.Tiles.Tile> used = new Dictionary<Point, Dungeons.Tiles.Tile>();

      foreach (var emp in emptyOnes)
      //for(int i=emptyOnes.Count-1; i>=0;i--)
      {
        //var emp = emptyOnes[i];
        if (used.ContainsKey(emp.Key))
          continue;
        var neibs = node.GetNeighborTiles(emp.Value);
        if (neibs.All(i => i.IsEmpty))
        {
          var rand = RandHelper.GetRandomDouble();
          var gen = rand > randThreshold ||
            (countSkipped1 > 15 && rand > 0.6) ||
            countSkipped1 > 20;
          if (gen)
          {
            countSkipped1 = 0;
            AddStaticColl(res, tag1, emp.Value, used);

            var rand1 = RandHelper.GetRandomDouble();
            if (rand1 > .8f)
            {
              var next = neibs.Where(i => i.IsEmpty).First();
              AddStaticColl(res, tag1, next, used);

              next = neibs.Where(i => i.IsEmpty).First();
              AddStaticColl(res, tag1, next, used);
            }
          }
          else
            countSkipped1++;
        }
        
        counter++;
        if (counter > changeTagCounter)
        {
          counter = 0;
          tagIndex++;
          if (tagsList.Count <= tagIndex)
            tagIndex = 0;

          tag1 = tagsList[tagIndex];
        }
      }
      foreach (var pt in used.Keys)
      {
        //var em = emptyOnes.FirstOrDefault(i => i.point == pt);
        //if (em!=null)
        emptyOnes.Remove(pt);
      }
      Log("[BOOT] GenerateTilesAtEmptyPlaces end: " + res.Count + " emptyOnes.Count: "+ emptyOnes.Count + " TotalSeconds: " + tr.TotalSeconds);
      return res;
    }

    private T AddStaticColl<T>
      (
      List<T> res, 
      string tag, 
      Dungeons.Tiles.Tile emp,
      Dictionary<Point, Dungeons.Tiles.Tile> used
      ) where T : Dungeons.Tiles.Tile, new()//tree1
    {
      if (used.ContainsKey(emp.point))
        return null;
            
      var tile = new T();
      used[emp.point] = tile;
      tile.tag1 = tag;
      node.SetTile(tile, emp.point);
      if(tile is Dungeons.Tiles.Wall wall)
        wall.DynamicallyGenerated = true;
      res.Add(tile);
      return tile;
    }
  }
}
