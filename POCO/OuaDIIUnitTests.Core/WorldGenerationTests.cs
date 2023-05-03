using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using NUnit.Framework;
using OuaDII.Generators;
using OuaDII.Quests;
using OuaDII.Serialization;
using OuaDII.TileContainers;
using OuaDII.Tiles.LivingEntities;
using OuaDII.Tiles.Looting;
using Roguelike;
using Roguelike.Settings;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class WorldGenerationTests : TestBase
  {

    [Test]
    public void TestWorldMerge()
    {
      int wallsCount = 10;
      var _info = CreateGenerationInfo();
      World staticWorld = CreateStaticWorld(wallsCount);//simulate unity maps

      int barrelsCount = 10;
      World dynamicWorld = CreateDynamicWorld(staticWorld, barrelsCount);

      //Merge
      var worldData = dynamicWorld.Merge(staticWorld);

      Assert.AreEqual(worldData.Dungeon.GetTiles<Barrel>().Count, barrelsCount);
      Assert.AreEqual(worldData.Dungeon.GetTiles<Wall>().Count, wallsCount);
      Assert.AreEqual(worldData.DynamicTiles.Count, barrelsCount);//these are barrels

      worldData.Dungeon.DoGridAction((int col, int row) =>
      {
        var pt = new Point(col, row);
        if (worldData.Dungeon.GetTile(pt) == null)
        {
          Assert.False(true);
        }
        else if (worldData.Dungeon.Tiles[row, col] == null)
          Assert.False(true);
      });

    }

    private World CreateDynamicWorld(World staticWorld, int barrelsCount)
    {
      World dynamicWorld = new World(CreateContainer());
      dynamicWorld.Create(10, 10);
      dynamicWorld.DoGridAction((int col, int row) =>
      {
        dynamicWorld.SetTile(new Tile(), new Point(col, row));
      });
      dynamicWorld.NullTilesAllowed = true;
      Assert.True(dynamicWorld.SetTile(null, new Point(1, 3)));


      for (int i = 0; i < barrelsCount; i++)
      {
        var emp = dynamicWorld.GetRandomEmptyTile(Dungeons.TileContainers.DungeonNode.EmptyCheckContext.Unset);
        while (staticWorld.GetTile(emp.point) == null || !staticWorld.GetTile(emp.point).IsEmpty)//is it empty on static world ?
          emp = dynamicWorld.GetRandomEmptyTile(Dungeons.TileContainers.DungeonNode.EmptyCheckContext.Unset);
        Assert.True(dynamicWorld.SetTile(new Barrel(Container), emp.point));
      }

      return dynamicWorld;
    }

    private World CreateStaticWorld(int wallsCount)
    {
      World staticWorld = new World(CreateContainer());
      staticWorld.Create(10, 10);
      staticWorld.DoGridAction((int col, int row) =>
      {
        if (RandHelper.GetRandomDouble() > 0.2f)
          staticWorld.SetTile(new Tile(), new Point(col, row));
      });

      //at least one null
      staticWorld.NullTilesAllowed = true;
      Assert.True(staticWorld.SetTile(null, new Point(0, 0)));

      for (int i = 0; i < wallsCount; i++)
      {
        var emp = staticWorld.GetRandomEmptyTile(Dungeons.TileContainers.DungeonNode.EmptyCheckContext.Unset);
        Assert.True(staticWorld.SetTile(new Wall(), emp.point));
      }

      Assert.Greater(staticWorld.GetTiles().Where(i => i.IsEmpty).Count(), 10);

      return staticWorld;
    }

    [Test]
    public void EventsManagerInstTest()
    {
      CreateWorld();
      var em1 = GameManager.EventsManager;
      var oldWorld = GameManager.World;

      Reload(GameManager.Hero.Name);
      var em2 = GameManager.EventsManager;
      //Assert.AreEqual(em1, em2);
      Assert.AreNotEqual(oldWorld, GameManager.World);
    }

    [Test]
    [Repeat(1)]
    public void TestDifficultyWorld()
    {
      int plainChecks = 0;
      int chempChecks = 0;

      for (int level = 1; level < 6; level++)
      {
        var _info = CreateGenerationInfo();
        _info.SetMinWorldSize(80);
        _info.Counts.WorldEnemiesCount = 250;

        Roguelike.Generators.GenerationInfo.Difficulty = Roguelike.Difficulty.Easy;
        var worldEasy = CreateWorld(info: _info);
        Assert.AreEqual(GameManager.GameState.CoreInfo.Difficulty, Roguelike.Difficulty.Easy);

        var plainEasyEnemies = PlainEnemies.Where(i => i.Level == level && !i.IsStrongerThanAve).ToList();
        var plainEasyEnemy = plainEasyEnemies.First();
        var chempEasyEnemies = ChampionEnemies.Where(i => i.Level == level && !i.IsStrongerThanAve).ToList();
        if (!chempEasyEnemies.Any())
          continue;
        var chempEasyEnemy = chempEasyEnemies.First();

        Roguelike.Generators.GenerationInfo.Difficulty = Roguelike.Difficulty.Normal;
        var worldNormal = CreateWorld(info: _info);
        Assert.AreEqual(GameManager.GameState.CoreInfo.Difficulty, Roguelike.Difficulty.Normal);
        var plainNormalEnemies = PlainEnemies.Where(i => i.Level == plainEasyEnemy.Level && !i.IsStrongerThanAve);
        var plainNormalEnemy = plainNormalEnemies.FirstOrDefault();

        var chempNormalEnemies = ChampionEnemies.Where(i => i.Level == chempEasyEnemy.Level && !i.IsStrongerThanAve).ToList();
        var chempNormalEnemy = chempNormalEnemies.FirstOrDefault();

        if (plainNormalEnemy != null)
        {
          var pnd = plainNormalEnemies.Average(i => i.Stats.Defense);
          var ped = plainEasyEnemies.Average(i => i.Stats.Defense);
          Assert.Greater(pnd, ped);
          Assert.Greater(plainNormalEnemy.Stats.Defense, plainEasyEnemy.Stats.Defense);
          plainChecks++;
        }

        if (chempNormalEnemy != null)
        {
          Assert.Greater(chempNormalEnemies.Average(i => i.Stats.Defense), chempEasyEnemies.Average(i => i.Stats.Defense));
          chempChecks++;
        }
      }
      Assert.GreaterOrEqual(plainChecks, 1);
      Assert.GreaterOrEqual(chempChecks, 1);
    }

    List<Enemy> GetBosses(Difficulty diff)
    {
      var bosses = new List<Enemy>();
      Roguelike.Generators.GenerationInfo.Difficulty = diff;
      var world = CreateWorld();
      var pits = GetPitsWithBoss(world);
      foreach (var pit in pits)
      {
        var bossName = pit.Item1.GetLastLevelBossName();
        Assert.True(!string.IsNullOrEmpty(bossName));
        GotoLastLevel(pit.Item1, pit.Item2);
        var en = GameManager.CurrentNode.GetTiles<Enemy>().ToList();
        var boss = en.Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).FirstOrDefault();
        Assert.NotNull(boss);
        bosses.Add(boss);
      }

      return bosses;
    }

    [Test]
    [Repeat(1)]
    public void TestDifficultyPits()
    {
      var easyBosses = GetBosses(Difficulty.Easy).OrderBy(i => i.Level).ToList();
      var normalBosses = GetBosses(Difficulty.Normal).OrderBy(i => i.Level).ToList();
      var hardBosses = GetBosses(Difficulty.Hard).OrderBy(i => i.Level).ToList();
      Assert.AreEqual(easyBosses.Count, normalBosses.Count);
      int checkedCount = 0;
      foreach (var easyBoss in easyBosses)
      {
        //var normalBoss = normalBosses.Where(i=>i.Name == easyBoss.Name).Single();
        var normalBoss = normalBosses.Where(i => i.Level == easyBoss.Level).SingleOrDefault();
        if (normalBoss != null && normalBoss.Level == easyBoss.Level)
        {
          Assert.Greater(normalBoss.Stats.Defense, easyBoss.Stats.Defense);
          checkedCount++;

          //var hardBoss = hardBosses.Where(i => i.Name == easyBoss.Name).Single();
          var hardBoss = hardBosses.Where(i => i.Level == easyBoss.Level).SingleOrDefault();
          if (hardBoss != null && normalBoss.Level == hardBoss.Level)
          {
            Assert.Greater(hardBoss.Stats.Defense, normalBoss.Stats.Defense);
            checkedCount++;
          }
        }
      }

      Assert.Greater(checkedCount, 0);
    }

    [Test]
    [Repeat(1)]
    public void LevelAtGameEnd()
    {

      bool newGame = true;
      var info = CreateGenerationInfo();
      var size = 100;
      info.MinNodeSize = new System.Drawing.Size(size, size);
      info.MaxNodeSize = info.MinNodeSize;

      var worldEnemiesCount = 200;
      info.Counts.WorldEnemiesCount = worldEnemiesCount;



      var world = CreateWorld(newGame, info);
      Assert.AreEqual(world.Width, size);



      var worldEnemies = world.GetTiles<Enemy>();
      var maxEn = worldEnemiesCount + Roguelike.Generators.GenerationInfo.MaxEnemyPackCount * info.Counts.WorldEnemiesPacksCount;
      Assert.GreaterOrEqual(worldEnemies.Count, maxEn);
      Assert.LessOrEqual(worldEnemies.Count, maxEn + 10);

      Assert.AreEqual(GameManager.Hero.Level, 1);
      //KillAllEnemies();
      worldEnemies = world.GetTiles<Enemy>();


      //kill from dungeons
      var stairsPitDown = world.GetAllStairs(StairsKind.PitDown).ToList();

      // var killedInDungeons = 0;
      Assert.GreaterOrEqual(stairsPitDown.Count, 6);
      Dictionary<int, int> killedEnemiesToLevel = new Dictionary<int, int>();
      List<Enemy> dungeonEnemies = new List<Enemy>();

      foreach (var stair in stairsPitDown)
      {
        WriteLine("----------------------------------");
        WriteLine("testing pit " + stair.PitName);
        WriteLine("----------------------------------");
        Stairs stairsPitUp = null;
        var pit = world.GetPit(stair.PitName);
        if (stair.PitName == "pit_down_Smiths")
          continue;
        //var stairsPitUp = GotoLastLevel(pit, stair);
        GameManager.InteractHeroWith(stair);

        for (int i = 0; i <= pit.LevelGenerator.MaxLevelIndex; i++)
        {
          if (i == 0)
            stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);

          var enemies = GameManager.CurrentNode.GetTiles<Enemy>().ToList();
          dungeonEnemies.AddRange(enemies);
          //killedInDungeons += KillAllEnemies(); 

          var stairsDown = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
          if (i == pit.LevelGenerator.MaxLevelIndex)
          {
            GameManager.InteractHeroWith(stairsPitUp);
            break;
          }

          GameManager.InteractHeroWith(stairsDown);
        }

      }
      Assert.Greater(dungeonEnemies.Count, 400);

      List<Enemy> totalEnemies = new List<Enemy>();
      totalEnemies.AddRange(worldEnemies);
      totalEnemies.AddRange(dungeonEnemies);
      totalEnemies = totalEnemies.OrderBy(i => i.Level).ToList();
      double nextLevelUpExp = GameManager.Hero.NextLevelExperience;
      int killedCount = 0;
      foreach (var en in totalEnemies)
      {
        int prevLevel = GameManager.Hero.Level;
        KillEnemy(en);
        if (GameManager.Hero.Level > prevLevel)//leveled up
        {
          killedEnemiesToLevel[GameManager.Hero.Level] = killedCount;
          var scale = GameManager.Hero.CalcExpScale();
          Assert.Greater(GameManager.Hero.NextLevelExperience, nextLevelUpExp * 2);
          Assert.Less(GameManager.Hero.NextLevelExperience, nextLevelUpExp * 3);
          nextLevelUpExp = GameManager.Hero.NextLevelExperience;
          Assert.Less(scale, 0.6f);
        }
        killedCount++;
      }

      //Assert.AreEqual(worldEnemies.Count, 0);
      Assert.GreaterOrEqual(GameManager.Hero.Level, 10);
      Assert.LessOrEqual(GameManager.Hero.Level, 12);
    }

    List<Tuple<DungeonPit, Stairs>> GetPitsWithBoss(World world)
    {
      var pits = new List<Tuple<DungeonPit, Stairs>>();
      var stairsPitDown = world.GetAllStairs(StairsKind.PitDown);
      foreach (var stair in stairsPitDown)
      {
        var pit = world.GetPit(stair.PitName);
        if (pit.QuestKind != OuaDII.Quests.QuestKind.Unset)
          continue;

        pits.Add(new Tuple<DungeonPit, Stairs>(pit, stair));
      }
      return pits;
    }

    [Test]
    public void EnsureBossAtPitLastLevel()
    {
      var world = CreateWorld();
      var pits = GetPitsWithBoss(world);
      foreach (var pit in pits)
      {
        WriteLine("----------------------------------");
        WriteLine("----------------------------------");
        WriteLine("testing pit " + pit.Item1.Name);
        WriteLine("----------------------------------");
        Stairs stairsPitUp = null;

        var bossName = pit.Item1.GetLastLevelBossName();
        Assert.True(!string.IsNullOrEmpty(bossName));

        stairsPitUp = GotoLastLevel(pit.Item1, pit.Item2);

        //check boss
        var en = GameManager.CurrentNode.GetTiles<Enemy>().ToList();
        var boss = en.Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).FirstOrDefault();
        Assert.NotNull(boss);
        var eqsBefore = world.GetTiles<OuaDII.Tiles.Looting.GodStatue>();
        Assert.AreEqual(eqsBefore.Count, 0);
        KillEnemy(boss);
        GotoNextHeroTurn();
        var newLoot = GetDiff(eqsBefore);
        Assert.AreEqual(newLoot.Count, 1);
        //go back to world
        GameManager.InteractHeroWith(stairsPitUp);
        Assert.AreEqual(world, GameManager.CurrentNode);
      }
    }

    Stairs GotoLastLevel(DungeonPit pit, Stairs pitEntry)
    {
      GameManager.InteractHeroWith(pitEntry);
      return GotoLastLevel(pit);
    }

    private Stairs GotoLastLevel(DungeonPit pit, Action<bool> iterator = null)
    {
      Stairs stairsPitUp = null;
      for (int i = 0; i <= pit.LevelGenerator.MaxLevelIndex; i++)
      {
        if (i == 0)
          stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);
        var stairsOnLevel = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
        if (iterator != null)
          iterator(i == pit.LevelGenerator.MaxLevelIndex);
        if (i == pit.LevelGenerator.MaxLevelIndex)
        {
          return stairsPitUp;
        }
       
        GameManager.InteractHeroWith(stairsOnLevel);
        
      }

      Assert.True(false);
      return null;
    }

    [Test]
    public void NewGameTest()
    {
      var world = CreateWorld();
      Assert.NotNull(world.GetTiles<Hero>().Single());
      var tiles = world.GetTiles();
      var revealedTiles = tiles.Where(i => i.Revealed).ToList();
      Assert.AreEqual(tiles.Count, revealedTiles.Count);
    }

    [Test]
    public void NewGameTestDataTest()
    {
      var td = new TestingData();
      td.AbilitiesPoints = 15;
      var pers = new JSONPersister(CreateContainer());
      pers.SaveTestingData(td);

      var world = CreateWorld();

      var hero = world.GetTiles<Hero>().Single();
      Assert.NotNull(hero);
      Assert.AreEqual(hero.AbilityPoints, 15);
      pers.DeleteTestingData();
    }

    [Test]
    public void BigWorldTimeCreationTest()
    {
      var _info = CreateGenerationInfo();
      _info.SetMinWorldSize(200);
      TimeTracker tt = new TimeTracker();
      var world = CreateWorld(info: _info);
      Assert.AreEqual(GameManager.SaveCounter, 0);
      var ts = tt.TotalSeconds;
      Assert.Less(ts, 5);
    }

    [Test]
    public void EnemyPower()
    {
      var _info = CreateGenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 20;
      var world = CreateWorld(info: _info);
      var enemies = world.GetTiles<Enemy>();

      Assert.GreaterOrEqual(enemies.Count, 20);

      var byLevel = enemies.GroupBy(i => i.Level).ToList();
      var normalPits = world.Pits.Where(i => i.QuestKind == OuaDII.Quests.QuestKind.Unset).Count();
      //Assert.Greater(normalPits, 3);
      Assert.Greater(byLevel.Count, normalPits / 2);
      //Assert.AreEqual(byLevel.Count(), normalPits);
    }

    [Test]
    public void BossRoomTest()
    {
      DungeonPit pit = GotoNonQuestPit(null);
      GotoLastLevel(pit);

      var boss = AllEnemies.Cast<Enemy>().Where(e => e.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).ToList();
      Assert.AreEqual(boss.Count, 1);
      var chests = GameManager.CurrentNode.GetTiles<Chest>().Where(i => i.ChestKind == ChestKind.GoldDeluxe).ToList();
      Assert.AreEqual(chests.Count, 1);
    }

    [TestCase(KeyPuzzle.Barrel)]
    [TestCase(KeyPuzzle.Chest)]
    [TestCase(KeyPuzzle.Grave)]
    //[TestCase(KeyPuzzle.Unset)]
    [TestCase(KeyPuzzle.DeadBody)]
    [TestCase(KeyPuzzle.Half)]
    [TestCase(KeyPuzzle.SecretRoom)]
    [TestCase(KeyPuzzle.Mold)]
    [TestCase(KeyPuzzle.Enemy)]
    [TestCase(KeyPuzzle.LeverSet)]
    [Repeat(1)]
    public void RoomKeyTestDungeonLayouterDefault(KeyPuzzle keyPuzzle)
    {
      DungeonPit pit = GotoNonQuestPit(null, keyPuzzle: keyPuzzle, defaultForcedDungeonLayouterKind: Dungeons.DungeonLayouterKind.Default);
      GameManager.Hero.d_immortal = true;
      CheckLevel(keyPuzzle, false);
      GotoLastLevel(pit);
      CheckLevel(keyPuzzle, true);
    }

    [TestCase(KeyPuzzle.Barrel)]
    [TestCase(KeyPuzzle.Chest)]
    [TestCase(KeyPuzzle.Grave)]
    //[TestCase(KeyPuzzle.Unset)]
    [TestCase(KeyPuzzle.DeadBody)]
    [TestCase(KeyPuzzle.Half)]
    [TestCase(KeyPuzzle.SecretRoom)]
    [TestCase(KeyPuzzle.Mold)]
    [TestCase(KeyPuzzle.Enemy)]
    [TestCase(KeyPuzzle.LeverSet)]
    [Repeat(1)]
    public void RoomKeyTestDungeonLayouterCorridor(KeyPuzzle keyPuzzle)
    {
      OuaDII.Generators.GenerationInfo.DefaultForcedDungeonLayouterKind = DungeonLayouterKind.Corridor;
      DungeonPit pit = GotoNonQuestPit(null, keyPuzzle: keyPuzzle, defaultForcedDungeonLayouterKind: Dungeons.DungeonLayouterKind.Corridor);
      GameManager.Hero.d_immortal = true;
      CheckLevel(keyPuzzle, false);
      GotoLastLevel(pit);
      CheckLevel(keyPuzzle, true);
    }

    private void CheckLevel(KeyPuzzle keyPuzzle, bool lastLevel)
    {
      var bossDoor = GameManager.CurrentNode.GetTiles<Door>().Where(i => i.BossBehind.Any()).ToList();
      var keys = GameManager.CurrentNode.GetTiles<Key>().ToList();
      
      if (lastLevel)
      {
        Assert.Greater(bossDoor.Count, 0);
        Assert.True(bossDoor.All(i => i.KeyPuzzle == keyPuzzle));
        Assert.False(bossDoor.All(i => i.Opened));
        GameManager.InteractHeroWith(bossDoor.First());
        Assert.False(bossDoor.All(i => i.Opened));
        if (keyPuzzle == KeyPuzzle.Unset || keyPuzzle == KeyPuzzle.SecretRoom)
        {
          Assert.AreEqual(keys.Count, 1);
          
        }
        //else if (keyPuzzle == KeyPuzzle.Barrel)
        //{
        //  Assert.AreEqual(keys.Count, 0);
        //  var inter = GameManager.CurrentNode.GetTiles<Barrel>().Where(i => i.ForcedReward is Key).SingleOrDefault();
        //  Assert.NotNull(inter);
        //  GameManager.InteractHeroWith(inter);
        //}
        else //if (keyPuzzle == KeyPuzzle.Chest || keyPuzzle == KeyPuzzle.Grave)
        {
          Assert.AreEqual(keys.Count, 0);

          if (keyPuzzle == KeyPuzzle.Half)
          {
            GameManager.CurrentNode.GetTiles<Barrel>().ForEach(i =>
            {
              GameManager.InteractHeroWith(i);
              GotoNextHeroTurn();
            });
            GameManager.CurrentNode.GetTiles<Chest>().ForEach(i =>

            {
              GameManager.InteractHeroWith(i);
              GotoNextHeroTurn();
            });
            GameManager.CurrentNode.GetTiles<DeadBody>().ForEach(i =>

            {
              GameManager.InteractHeroWith(i);
              GotoNextHeroTurn();
            });

            var keyHalfs = GameManager.CurrentNode.GetTiles<KeyHalf>().ToList();
            Assert.AreEqual(keyHalfs.Count, 2);
            var keyMerged = new Key();//TODO from quest
            keyMerged.KeyName = keyHalfs[0].KeyName;
            GameManager.CurrentNode.SetTileAtRandomPosition(keyMerged);
          }
          else if (keyPuzzle == KeyPuzzle.LeverSet)
          {
            var inter = GameManager.CurrentNode.GetTiles<Lever>();
            Assert.Greater(inter.Count, 0);
            var ls = (GameManager.CurrentNode as GameLevel).BossRoomLeverSet;
            Assert.IsFalse(ls.IsOpened());
            for (int i=0;i<ls.OpeningSequence.Count;i++)
            {
              if (ls.OpeningSequence[i])
                ls.Levers[i].SwitchState();
            }
            Assert.IsTrue(ls.IsOpened());
          }
          else
          {
                        
            
            //else
            {
              var inter = GameManager.CurrentNode.GetTiles<ILootSource>().Where(i => i.ForcedReward is IKey).SingleOrDefault();
              Assert.NotNull(inter);
              GameManager.InteractHeroWith(inter as Tile);

              if (keyPuzzle == KeyPuzzle.Mold)
              {
                var keyMerged = new Key();//TODO from questL
                keyMerged.KeyName = (inter.ForcedReward as IKey).KeyName;
                GameManager.CurrentNode.SetTileAtRandomPosition(keyMerged);
              }

              if (inter is Enemy en && en.Alive)
              {
                KillEnemy(en);
              }
            }
            
          }
        }
        if (keyPuzzle != KeyPuzzle.LeverSet)
        {
          keys = GameManager.CurrentNode.GetTiles<Key>().ToList();
          var key = keys.SingleOrDefault();
          Assert.NotNull(key);

          GameManager.Hero.Inventory.Add(key);

          
        }
        GameManager.InteractHeroWith(bossDoor.First());
        Assert.True(bossDoor.All(i => i.Opened));
      }
      else
      {
        Assert.AreEqual(bossDoor.Count, 0);
        Assert.AreEqual(keys.Count, 0);
      }
      
    }

    //[Test]
    //public void PitTestStairsSorrounding()
    //{
    //  CreateWorld();
    //  var loot = GameManager.CurrentNode.GetTiles<MinedLoot>().Cast<MinedLoot>().Where(i => i.Kind == MinedLootKind.IronOre);
    //  Assert.Greater(loot.Count(), 0);
    //}

    [Test]
    public void PitTestStairs()
    {
      DungeonPit pit = GotoNonQuestPit(null);

      int MaxLevelIndex = pit.Levels.Count;
      for (int i = 0; ; i++)
      {
        Assert.NotNull(pit.Levels[i].GetTiles<Hero>());

        var stairsOnLevel = GameManager.CurrentNode.GetTiles<Stairs>();
        if (i == MaxLevelIndex)
          Assert.AreEqual(stairsOnLevel.Count, 1);
        else
          Assert.AreEqual(stairsOnLevel.Count, 2);

        var stairsUpKind = i == 0 ? StairsKind.PitUp : StairsKind.LevelUp;
        Stairs stair = GameManager.CurrentNode.GetStairs(stairsUpKind);
        Assert.NotNull(stair);

        stair = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
        Assert.True(stair != null || i == MaxLevelIndex);
        if (stair != null)
          GameManager.InteractHeroWith(stair);
        else
          break;
      }

      Assert.AreEqual(pit.Levels.Count, MaxLevelIndex + 1);
    }

    [Test]
    [Repeat(1)]
    public void FixedRoomSize()
    {

      var world = CreateWorld();

      // //world.AddStairsWithPit("ratPit", new System.Drawing.Point(6, 6));

      var stairs = world.GetTiles<Stairs>();
      Assert.Greater(stairs.Count, 0);

      // GameManager.InteractHeroWith(stairs[0]);
      // var container = new OuaDII.ContainerConfiguratorDungeonLevel<Dungeons.Core.Logger>().Container;
      //var gen = container.GetInstance<IDungeonGenerator>();
      var gi = new Roguelike.Generators.GenerationInfo();
      gi.NumberOfRooms = 1;
      gi.MaxNodeSize = new System.Drawing.Size(9, 9);
      gi.MinNodeSize = gi.MaxNodeSize;
      gi.PreventSecretRoomGeneration = true;
      gi.ChildIslandAllowed = false;
      gi.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;

      Assert.Greater(world.Pits.Count, 0);
      var pit = world.Pits.Where(i => i.Name.Contains("rat")).Single();
      Assert.AreEqual(pit.Levels.Count, 0);
      pit.SetGenInfoAtLevelIndex(0, gi);
      var st = stairs.Where(i => i.PitName == pit.Name).Single();
      GameManager.InteractHeroWith(st);

      Assert.AreEqual(pit.Levels.Count, 1);
      Assert.AreEqual(pit.Levels[0].Width, gi.MaxNodeSize.Width);
      Assert.AreEqual(pit.Levels[0].Height, gi.MaxNodeSize.Height);
    }

    [Test]
    [Repeat(1)]
    public void EnemyNearStartOftenWounded()
    {
      var _info = CreateGenerationInfo();
      _info.SetMinWorldSize(50);
      _info.Counts.WorldEnemiesCount = 80;
      var world = CreateWorld(true, _info);
      var enemies = world.GetTiles<Enemy>();

      Assert.Greater(enemies.Count, 10);

      var byDistance = enemies.OrderBy(i => i.DistanceFrom(GameManager.Hero)).Take(12);
      var wounded = byDistance.Where(i => i.IsWounded).ToList();
      Assert.GreaterOrEqual(wounded.Count, 2);
      Assert.Less(wounded.Count, 10);
    }

    [Test]
    [Repeat(1)]
    public void EachLevelHasSecretRoom()
    {
      IterateNormalPits((bool lastLevel) => {
          var secretNode = GameManager.CurrentNode.Nodes.FirstOrDefault(i => i.Secret);
          var doors = GameManager.CurrentNode.GetTiles<Door>();
          Assert.NotNull(secretNode);
          Assert.True(doors.Where(k => k.Secret).Any());
      });
    }

    [Test]
    [Repeat(1)]
    public void EachLevelHasKeyToBossRoom()
    {
      int co =  0;
      IterateNormalPits((bool lastLevel) => {
        if (lastLevel)
        {
          var door = GameManager.CurrentNode.GetTiles<Door>().Where(i => i.BossBehind.Any()).FirstOrDefault();
          Assert.NotNull(door);
          Assert.True(door.KeyPuzzle != KeyPuzzle.Unset);
          co++;
        }
      });
      Assert.Greater(co, 5);
    }

    private void IterateNormalPits(Action<bool> ac)
    {
      var world = CreateWorld();

      var stairsPitDown = world.GetAllStairs(StairsKind.PitDown);

      foreach (var stair in stairsPitDown)
      {
        Stairs stairsPitUp = null;
        var pit = world.GetPit(stair.PitName);
        if (pit.QuestKind != QuestKind.Unset)
          continue;

        GameManager.InteractHeroWith(stair);
        for (int levelIndex = 0; levelIndex <= pit.LevelGenerator.MaxLevelIndex; levelIndex++)
        {
          if (levelIndex == 0)
            stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);

          ac(levelIndex == pit.LevelGenerator.MaxLevelIndex);

          //go down
          var stairsDown = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
          GameManager.InteractHeroWith(stairsDown);
        }

        //go back
        GameManager.InteractHeroWith(stairsPitUp);
        Assert.AreEqual(world, GameManager.CurrentNode);
      }
    }

    [Test]
    public void GatheringPitGenerationTest()
    {
      var world = CreateWorld();
      var pit = InteractHeroWithPit(QuestKind.GatheringEntry);
      
      int MaxLevelIndex = pit.Levels.Count -1;
      Assert.GreaterOrEqual(MaxLevelIndex, 0);
      for (int i = 0;  i< MaxLevelIndex; i++)
      {
        Assert.NotNull(pit.Levels[i].GetTiles<Hero>());

        var stairsOnLevel = GameManager.CurrentNode.GetTiles<Stairs>();
        if (i == MaxLevelIndex)
          Assert.AreEqual(stairsOnLevel.Count, 1);
        else
          Assert.AreEqual(stairsOnLevel.Count, 2);

        var stairsUpKind = i == 0 ? StairsKind.PitUp : StairsKind.LevelUp;
        Stairs stair = GameManager.CurrentNode.GetStairs(stairsUpKind);
        Assert.NotNull(stair);

        stair = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
        Assert.True(stair != null || i == MaxLevelIndex);
        if (stair != null)
          GameManager.InteractHeroWith(stair);
        else
        {
          var boss = GameManager.EnemiesManager.GetEnemies().Where(i => i.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).FirstOrDefault();
          Assert.NotNull(boss);
          break;
        }
      }

      Assert.AreEqual(pit.Levels.Count, MaxLevelIndex + 1);
    }
  }
}
