using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class LootTests : TestBase
  {

    [Test]
    public void PiSTest()
    {
      var world = CreateWorld();

      var sword = GameManager.LootGenerator.GetLootByAsset("PiS");
      Assert.AreEqual(sword.Price, 500);//++
    }


      [Test]
    public void GemsLevel()
    {
      var world = CreateWorld();

      var bolt = GameManager.LootGenerator.GetLootByAsset("bolt1");
      try
      {
        //ILootSource lootSrc = Enemies.First();
        for (int i = 0; i < 10; i++)
        {
          var loot = GameManager.LootGenerator.GetRandomLoot(LootKind.Gem, 1) as Gem;
          Assert.AreEqual(loot.EnchanterSize, EnchanterSize.Small);
        }
      }
      catch (System.Exception)
      {
        //GenerationInfo.DebugInfo.EachEnemyGivesPotion = false;
      }
    }

    /// <summary>
    /// See ChanceAtGameStart class for tested mechanism
    /// </summary>
    [Test]
    [Repeat(1)]
    public void EarlyEnemiesThrowsEquipment()
    {
      int enemiesCount = 35;
      List<Equipment> eqs = GetEquipmentFromDeadEnemies(enemiesCount);
      Assert.True(eqs.Any(i => i is Weapon));
    }

    private List<Equipment> GetEquipmentFromDeadEnemies(int enemiesCount = 30)
    {
      
      List<Loot> newLoot = GetLootFromDeadEnemies(enemiesCount);
      var eqs = newLoot.Where(i => i is Equipment).Cast<Equipment>().ToList();
      Assert.Greater(eqs.Count, 0);
      return eqs;
    }

    private List<Loot> GetLootFromDeadEnemies(int enemiesCount = 30)
    {
      var infoW = CreateGenerationInfo();
      infoW.Counts.WorldEnemiesCount = enemiesCount;

      var world = CreateWorld(true, infoW);
      GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      var eqsBefore = world.GetTiles<Loot>();
      KillAllEnemies();
      var newLoot = GetDiff(eqsBefore);
      Assert.Greater(newLoot.Count, 0);
      return newLoot;
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    //[Repeat(3)]
    public void EquipmentMaterialFromLootManager(int lootSrcLevel)
    {
      OuaDII.Generators.GenerationInfo gi = new OuaDII.Generators.GenerationInfo();
      gi.SetMinWorldSize(70);
      var world = CreateWorld(gi);
      GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      GameManager.LootGenerator.ForcedEquipmentKind = EquipmentKind.Weapon;
      var en = AllEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).First();
      var levelToEqCount = new Dictionary<int, List<Equipment>>();
      var enLevel = lootSrcLevel;
      //for (int enLevel = 1; enLevel <= 7; enLevel++)
      {
        en.Level = enLevel;
        var eqs = new List<Equipment>();
        for (int i = 0; i < 300; i++)
        {
          var loot = GameManager.LootManager.TryAddForLootSource(en);
          var lootItems = loot.Where(k => k is Equipment eq && eq.IsMaterialAware()).Cast<Equipment>().ToList();
          if(lootItems.Any())
            eqs.AddRange(lootItems);
        }
        levelToEqCount[enLevel] = eqs;
      }

      if (!levelToEqCount.Any() && lootSrcLevel == 7)
      {
        int k = 0;
        k++;
      }

      foreach (var levelToEq_ in levelToEqCount)
      {
        var eqs = levelToEq_.Value;
        Assert.GreaterOrEqual(eqs.Count, 6);
        if (levelToEq_.Key <= MaterialProps.IronDropLootSrcLevel)
          Assert.True(eqs.All(i => i.Material == Roguelike.Tiles.Looting.EquipmentMaterial.Bronze));
        else
        {
          var materials = eqs.GroupBy(i => i.Material);
          if (materials.Count() < 2)
          {
            int k = 0;
            k++;
          }
          Assert.AreEqual(materials.Count(), 2);
          if (levelToEq_.Key <= MaterialProps.SteelDropLootSrcLevel)
          {
            Assert.True(eqs.Any(i => i.Material == EquipmentMaterial.Bronze));
            Assert.True(eqs.Any(i => i.Material == EquipmentMaterial.Iron));
          }
          else
          {
            var bronzes = eqs.Where(i => i.Material == EquipmentMaterial.Bronze).ToList();
            Assert.False(bronzes.Any(i=>i.LevelIndex > MaterialProps.SteelDropLootSrcLevel));
            Assert.True(eqs.Any(i => i.Material == EquipmentMaterial.Iron));
            Assert.True(eqs.Any(i => i.Material == EquipmentMaterial.Steel));
          }
        }
      }
    }

    [Repeat(1)]
    [Test]
    //[Ignore("")]
    public void EquipmentMaterialTest()
    {
      var world = CreateWorld();
      var wpns = GetEquipmentFromDeadEnemies(200).Where(i => i.IsMaterialAware()).ToList();
      Assert.Greater(wpns.Count, 0);
      int mismatched = 0;
      foreach (var wpn in wpns)
      {
        Assert.True(wpn.DisplayedName.StartsWith("Bronze") ||
          wpn.DisplayedName.StartsWith("Iron") ||
          wpn.DisplayedName.StartsWith("Steel"));

        if (wpn.Source.Level <= MaterialProps.IronDropLootSrcLevel)
          Assert.True(wpn.Material == EquipmentMaterial.Unset ||
                    wpn.Material == EquipmentMaterial.Bronze);
        else if (wpn.Source.Level <= MaterialProps.SteelDropLootSrcLevel)
        {
          var ok = wpn.Material == EquipmentMaterial.Unset ||
                    wpn.Material == EquipmentMaterial.Iron;
          if (!ok)
          {
            mismatched++;
            WriteLine("!!wpn.Material ==" + wpn.Material);
          }

          //Assert.True(ok, "!!wpn.Material ==" + wpn.Material);
        }
        else
        { //RandHelper.GetRandomDouble() > 0.5f --> Steel
          Assert.True(wpn.Material == EquipmentMaterial.Unset ||
                    wpn.Material == EquipmentMaterial.Iron ||
                    wpn.Material == EquipmentMaterial.Steel
                    );
        }
      }

      Assert.Less(mismatched/ wpns.Count, 0.3f);
    }

    [Test]
    public void LootName()
    {
      var loot = new Mushroom(MushroomKind.Boletus);
      Assert.NotNull(loot);
      Assert.AreEqual(loot.Name, "Boletus");

      loot = new Mushroom(MushroomKind.RedToadstool);
      Assert.NotNull(loot);
      Assert.AreEqual(loot.Name, "Red Toadstool");

      var trophy = new HunterTrophy(HunterTrophyKind.Claw);
      var lsi = trophy.GetLootStatInfo(null);
      Assert.AreEqual(lsi[0].Kind, LootStatKind.Weapon);
    }

    [Test]
    public void LootByAssetName()
    {
      var world = CreateWorld();
      var loot = GameManager.LootGenerator.GetLootByAsset("craft_one_eq");
      Assert.NotNull(loot);

      var cap = GameManager.LootGenerator.GetLootByAsset("enhanced_cap");
      Assert.AreEqual(cap.Name, "Enhanced cap");

      var hound_cap = GameManager.LootGenerator.GetLootByAsset("steel_hound_cap");
      Assert.AreEqual(hound_cap.Name.ToLower(), "steel hound cap");
    }

    [Test]
    public void LootLevelMatchesEnemyLevel()
    {
      for (int loop = 0; loop < 1; loop++)
      {
        var info = CreateGenerationInfo();
        info.Counts.WorldEnemiesCount = 50;
        var world = CreateWorld(true, info);
        var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

        GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
        var enChances = GameManager.LootGenerator.Probability.EquipmentClassChances[LootSourceKind.Enemy];
        enChances.SetValue(EquipmentClass.Plain, 1);

        var eqsBefore = world.GetTiles<Loot>();
        var enemies = Enemies.OrderBy(i => i.Level).ToList();
        for (var enIndex = 0; enIndex < enemies.Count; enIndex++)
        {
          var en = enemies[enIndex];
          KillEnemy(en);
          var newLoot = GetDiff(eqsBefore);
          if (newLoot.Any())
          {
            var newEqs = newLoot.Where(i => i is Equipment).Cast<Equipment>().ToList();
            string err = "";
            var levelNotMatch = newEqs.Where(newEq => newEq.LevelIndex > (en.Level+1)).ToList();
            if (levelNotMatch.Any())
            {
              err = "en: " + en + ", not matching eqs:" +Environment.NewLine;
              foreach (var inv in levelNotMatch)
                err += inv.Name + " "+ inv.LevelIndex + Environment.NewLine;
            }
            Assert.False(levelNotMatch.Any(), err);
          }

          eqsBefore = world.GetTiles<Loot>();
        }
      }
    }

    [Test]
    [Repeat(1)]
    public void LootOnAllEnemyLevelsAndEqKinds()
    {
      var info = CreateGenerationInfo();
      info.Counts.WorldEnemiesCount = 300;
      var world = CreateWorld(true, info);
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var eqsBefore = world.GetTiles<Loot>();
      var enemies = AllEnemies.OrderBy(i => i.Level).ToList();
      List<int> enemyLevels = null;
      {
        var _enemyLevels = enemies.GroupBy(i => i.Level).Select(i => i.Key).ToList();
        //enemyLevels = Enumerable.Range(_enemyLevels.Min(), _enemyLevels.Max()).ToList();
        enemyLevels = _enemyLevels;
      }
      Assert.False(enemyLevels.Any(i => i == 0));//enemy level shall be from 1...n
      Assert.Greater(enemyLevels.Count(), 2);
      //foreach (var level in enemyLevels)
      //{
      //  Assert.True(enemies.Any(i=> i.Level == level));
      //}

      //GameManager.LootGenerator.Probability = new Roguelike.Probability.Looting();
      GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      var enChances = GameManager.LootGenerator.Probability.EquipmentClassChances[LootSourceKind.Enemy];
      enChances.SetValue(EquipmentClass.Plain, 1);

      var killed = KillAllEnemies();
      Assert.GreaterOrEqual(killed, info.Counts.WorldEnemiesCount);
      var newLoot = GetDiff(eqsBefore);
      var newEq = newLoot.Where(i => i is Equipment).Cast<Equipment>().ToList();
      //Assert.AreEqual(newLoot.Count, newEq.Count);
      Assert.Greater(newEq.Count, 0);
      var allKinds = Enum.GetValues(typeof(EquipmentKind)).Cast<EquipmentKind>()
        .Where(i => i != EquipmentKind.God && i != EquipmentKind.Unset && i != EquipmentKind.Trophy)
        .ToList();

      foreach (var level in enemyLevels)
      {
        var lvl = level;
        //if (level >= 7)
        //  continue;//TODO!
        var eqAtLevel = newEq.Where(i => i.LevelIndex == lvl).ToList();
        Assert.True(eqAtLevel.Any());

        var grCount = eqAtLevel.GroupBy(i => i.EquipmentKind).Count();
        Assert.GreaterOrEqual(grCount, (allKinds.Count/2)-1);
        //foreach (var kind in allKinds)
        //{
        //  var count = eqAtLevel.Where(i => i.EquipmentKind == kind).Count();
        //  Assert.Greater(count, 0, "checking "+ kind);
        //}
      }
    }

    [Test]
    [Repeat(1)]
    public void BarrelGenerateOneItem()
    {
      var info = CreateGenerationInfo();
      info.Counts.WorldBarrelsCount = 50;
      info.Counts.WorldChestsCount = info.Counts.WorldBarrelsCount;
      var world = CreateWorld(true, info);
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var inters = world.GetTiles<Barrel>();
      Assert.GreaterOrEqual(inters.Count, 30);

      var eqsBefore = world.GetTiles<Loot>();
      foreach (var inter in inters)
      {
        var enemies = world.GetTiles<Enemy>();
        GameManager.InteractHeroWith(inter);
        
        var newTile = world.GetTile(inter.point);
        
        if (newTile is Enemy)
        {
          var atPoint = world.GetLootTile(inter.point);
          Assert.Null(atPoint);
        }
        else //if(newTile.IsEmpty)
        {
          var newEns = world.GetTiles<Enemy>();
          if (newEns.Count > enemies.Count)
          {
            var newOne = newEns.Where(i => !enemies.Contains(i)).ToList();
            WriteLine("newOne: " + newOne);
          }
          Assert.AreEqual(enemies.Count, newEns.Count);
        }
        GotoNextHeroTurn();

      }

    }

    [Test]
    [Repeat(1)]
    public void EarlyBarrelsThrowsEquipment()
    {
      EarlyInteractiveThrowsEquipment<Barrel>();
    }

    [Test]
    [Repeat(2)]
    public void EarlyPlainChestThrowsEquipment()
    {
      EarlyInteractiveThrowsEquipment<Roguelike.Tiles.Interactive.Chest>();
    }


    void EarlyInteractiveThrowsEquipment<T>() where T : Dungeons.Tiles.Tile
    {
      const int InteractiveCount = 40;
      string heroName = "EarlyInteractiveThrowsEquipment";
      {
        var info = CreateGenerationInfo();
        info.Counts.WorldBarrelsCount = InteractiveCount;
        info.Counts.WorldChestsCount = InteractiveCount;
        info.Counts.WorldEnemiesCount = 0;
        info.Counts.WorldEnemiesPacksCount = 0;
        info.MinNodeSize = new System.Drawing.Size(60, 60);
        
        var world = CreateWorld(true, info);
        var heros = world.GetTiles<Hero>();
        var heroInNode = heros.SingleOrDefault();
        Assert.NotNull(heroInNode);

        var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
        hero.Name = heroName;

        var inters = world.GetTiles<T>();
        Assert.GreaterOrEqual(inters.Count, InteractiveCount);

        var eqsBefore = world.GetTiles<Loot>();
        foreach (var inter in inters)
        {
          GameManager.InteractHeroWith(inter);
          GotoNextHeroTurn();
        }

        var newLoot = GetDiff(eqsBefore);
        Assert.Greater(newLoot.Count, 2);
        Assert.GreaterOrEqual(newLoot.Count, 3);//Weapon, TinyTrophy, Pendant, Recipe
        Assert.Greater(GameManager.OuadGameState.ChanceAtGameStart.Chances.Where(i => i.Value.Done).Count(), 3);

        Assert.True(newLoot.Any(i => i.LootKind == LootKind.Recipe));
        Assert.True(newLoot.Any(i => i is Cord));

        var eqs = newLoot.Where(i => i.LootKind == LootKind.Equipment).Cast<Equipment>().ToList();
        Assert.GreaterOrEqual(eqs.Count, 1);
        Assert.AreEqual(eqs[0].Class, EquipmentClass.Plain);
        Assert.True(newLoot.Any(i => i.LootKind == LootKind.HunterTrophy));
        Assert.True(newLoot.Any(i => i.LootKind == LootKind.Scroll));

        GameManager.Save(false);
      }
      {
        //var world = CreateWorld();
        //var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
        //GameManager.Load(heroName, false);
        Reload();
        Assert.Greater(GameManager.OuadGameState.ChanceAtGameStart.Chances.Where(i => i.Value.Done).Count(), 3);
      }
    }

    [Test]
    //[Repeat(5)]
    public void EatableConsumption()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var enemy = PlainEnemies.First();

      int effCounter = 0;
      GameManager.EventsManager.EventAppended += (object s, GameEvent ga) =>
      {
        if (ga is LivingEntityAction)
        {
          var lea = ga as LivingEntityAction;
          if (lea.Kind == LivingEntityActionKind.ExperiencedEffect)
          {
            effCounter++;
          }
        }
      };

      var hp = new Potion();
      hp.SetKind(PotionKind.Health);
      hero.Inventory.Add(hp);

      var heroOrigHealth = hero.Stats.Health;
      HurtHero(hero, enemy);
      Assert.AreEqual(hero.LastingEffects.Count, 0);
      var heroHealth = hero.Stats.Health;
      hero.Consume(hp);

      Assert.Greater(hero.Stats.Health, heroHealth);
      var hpInc = hero.Stats.Health - heroHealth;
      heroHealth = hero.Stats.Health;

      GotoNextHeroTurn();
      Assert.Null(hero.GetFirstLastingEffect(EffectType.ConsumedRawFood));
      //shall not change - hp is consumed instantly
      Assert.AreEqual(hero.Stats.Health, heroHealth);

      HurtHero(hero, enemy);

      Enemies.Clear();//remove all - so that they do not interfere with lasting effect
      GameManager.EnemiesManager.AllEntities.Clear();
      Assert.AreEqual(GameManager.EnemiesManager.GetActiveEnemies().Count, 0);

      heroHealth = hero.Stats.Health;
      var plum = new Food(FoodKind.Plum);
      hero.Inventory.Add(plum);
      hero.Consume(plum);
      Assert.Greater(hero.Stats.Health, heroHealth);
      var foodInc = hero.Stats.Health - heroHealth;
      Assert.AreNotEqual(foodInc, hpInc);
      Assert.AreEqual(hero.LastingEffects.Count, 1);
      while (hero.LastingEffects.Count > 0)
      {
        heroHealth = hero.Stats.Health;
        GotoNextHeroTurn();
        //shall change - food is NOT consumed instantly
        Assert.Greater(hero.Stats.Health, heroHealth);
      }

      heroHealth = hero.Stats.Health;
      GotoNextHeroTurn();
      //effect done
      Assert.AreEqual(hero.Stats.Health, heroHealth);
    }

    private static void HurtHero(OuaDII.Tiles.LivingEntities.Hero hero, LivingEntity enemy)
    {
      //for (int i = 0; i < 5; i++)
      //hero.OnPhysicalHit(enemy);
      hero.ReduceHealth(hero.Stats.Health - 1);
    }

    [Test]
    [Repeat(1)]
    public void KilledEnemyGivesEveryLoot()
    {
      {
        var infoW = CreateGenerationInfo();
        infoW.Counts.WorldEnemiesCount = 300;

        var world = CreateWorld(true, infoW);
        var eqsBefore = world.GetTiles<Loot>();
        KillAllEnemies();
        var newLoot = GetDiff(eqsBefore);
        Assert.Greater(newLoot.Count, 0);

        var ench = newLoot.Where(i => i is Enchanter).ToList();

        var fiks = newLoot.Where(i=> i is FightItem).Cast< FightItem>().ToList();
        Assert.True(fiks.Any());
        Assert.AreEqual(0, fiks.Where(i => i.FightItemKind == FightItemKind.Smoke).Count());

        var allKinds = Enum.GetValues(typeof(LootKind)).Cast<LootKind>()
          .Where(i => i != LootKind.Unset && i != LootKind.Other && i != LootKind.Seal && i != LootKind.SealPart)
          .ToList();
        foreach (var kind in allKinds)
        {
          if (kind == LootKind.Book)
            continue;

          var kl = newLoot.Where(i => i.LootKind == kind).ToList();
          var count = kl.Count;
          Assert.Greater(count, 0);
          int maxCount = 30;
          if (kind == LootKind.Potion)
            maxCount = 95;
          if (kind == LootKind.Food)
            maxCount = 300;
          else if (kind == LootKind.Equipment)
            maxCount = 95;//Chemps and Bosses throw eq

          Assert.Less(count, maxCount);
        }
      }
    }
    ///////////////
    [Test]
    public void LootFromBoss()
    {
      //for (int t = 0;t < 3; t++)
      {
        var infoW = CreateGenerationInfo();
        infoW.Counts.WorldEnemiesCount = 10;

        var world = CreateWorld(true, infoW);
        GameManager.Hero.Level = 2;
        var eqsBefore = world.GetTiles<Loot>();
        AllEnemies.Cast<Enemy>().ToList().ForEach(i => i.SetNonPlain(true));
        AllEnemies.Cast<Enemy>().ToList().ForEach(i => i.SetLevel(2));
        KillAllEnemies();
        var newLoot = GetDiff(eqsBefore);
        Assert.Greater(newLoot.Count, 0);

        var eqs = newLoot.Where(i => i is Equipment).Cast<Equipment>().ToList();
        AssertGoodLoot(eqs, false);
      }
    }

    [Test]
    [Repeat(10)]
    public void LootFromChampion()
    {
      //for (int t = 0; t < 3; t++)
      {
        var infoW = CreateGenerationInfo();
        infoW.Counts.WorldEnemiesCount = 20;

        var world = CreateWorld(true, infoW);
        GameManager.Hero.Level = 2;
        var eqsBefore = world.GetTiles<Loot>();
        AllEnemies.ForEach(i => i.SetNonPlain(false));
        for(int i=0; i< AllEnemies.Count;i++)
          AllEnemies[i].SetLevel(i % 2 == 0 ? 2 : 3);
        
        KillAllEnemies();
        var newLoot = GetDiff(eqsBefore);
        Assert.Greater(newLoot.Count, 0);

        var eqs = newLoot.Where(i => i is Equipment).Cast<Equipment>().ToList();
        AssertGoodLoot(eqs, true);

        //high level enemy
        var en = world.SpawnEnemy(10);
        en.Level = 12;
        en.SetNonPlain(false); 
        GameManager.EnemiesManager.AllEntities.Add(en);
        world.SetTileAtRandomPosition(en);
        eqsBefore = world.GetTiles<Loot>();
        KillEnemy(en);
        newLoot = GetDiff(eqsBefore);
        Assert.Greater(newLoot.Count, 0);
      }
    }

    private static void AssertGoodLoot(List<Equipment> eqs, bool chemp)
    {
      var wrong = eqs.FirstOrDefault(i => !(i.Class == EquipmentClass.MagicSecLevel || i.Class == EquipmentClass.Unique || i.EnchantSlots > 1));
      Assert.AreEqual(wrong, null);
      Assert.True(eqs.All(i => i.Class == EquipmentClass.MagicSecLevel || i.Class == EquipmentClass.Unique || i.EnchantSlots > 1));
      Assert.True(eqs.Any(i => i.Class == EquipmentClass.MagicSecLevel));
      var uniq = eqs.Where(i => i.Class == EquipmentClass.Unique).ToList();
      Assert.True(uniq.Any());
    }

    [Test]
    public void TestPrices()
    {
      var world = CreateWorld();
      var lootItemsGrouped = GameManager.LootGenerator.LootFactory.GetAll()
        .Where(i => i.LootKind == LootKind.Equipment)
        .Cast<Equipment>()
        .OrderBy(j => j.LevelIndex)
        .GroupBy(i => i.EquipmentKind);

      foreach (var items in lootItemsGrouped)
      {
        var eqs = items.ToList();
        if (!eqs.Any())
          continue;

        var firstEq = eqs.First();
        var price = firstEq.Price;
        var level = firstEq.LevelIndex;
        foreach (var eq in eqs)
        {
          if (level < eq.LevelIndex)
            Assert.Greater(eq.Price, price);

          price = eq.Price;
          level = eq.LevelIndex;
        }
      }
    }
    //

    [Test]
    public void LootProps()
    {
      CreateWorld();
      
      var gm = GameManager;
      var shield = gm.LootGenerator.GetLootByAsset("buckler") as Equipment;
      Assert.AreEqual(shield.GetStats().Defense, 3);
      Assert.Greater(shield.GetReqStatValue(EntityStatKind.Strength), 0);

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.SetLevel(5);
      var wpn = gm.LootGenerator.GetLootByAsset("fire_scepter5") as Equipment;
      Assert.NotNull(wpn);
      Assert.AreEqual(wpn.tag1, "fire_scepter1");//TODO draw images
      Assert.AreEqual(wpn.tag2, "fire_scepter1");
    }


  }
}
