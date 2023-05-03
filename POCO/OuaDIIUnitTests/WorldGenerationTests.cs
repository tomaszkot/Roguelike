using NUnit.Framework;
using OuaDII.TileContainers;
using OuaDII.Tiles.LivingEntities;
using OuaDII.Tiles.Looting;
using Roguelike;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class WorldGenerationTests : TestBase
  {
    [Test]
    [Repeat(2)]
    public void TestDifficultyWorld()
    {
      int plainChecks = 0;
      int chempChecks = 0;
      // Assert.Null(GameManager);//TODO
      for (int level = 1; level < 6; level++)
      {
        var _info = new OuaDII.Generators.GenerationInfo();
        _info.SetMinWorldSize(100);
        _info.Counts.WorldEnemiesCount = 200;

        Roguelike.Generators.GenerationInfo.Difficulty = Roguelike.Difficulty.Easy;
        //CreateManager();//TODO
        var worldEasy = CreateWorld(info: _info);
        Assert.AreEqual(GameManager.GameState.CoreInfo.Difficulty, Roguelike.Difficulty.Easy);

        var plainEasyEnemies = PlainEnemies.Where(i => i.Level == level && !i.IsStrongerThanAve);
        var plainEasyEnemy = plainEasyEnemies.First();
        var chempEasyEnemies = ChampionEnemies.Where(i => i.Level == level && !i.IsStrongerThanAve).ToList();
        if (!chempEasyEnemies.Any())
          continue;
        var chempEasyEnemy = chempEasyEnemies.First();

        Roguelike.Generators.GenerationInfo.Difficulty = Roguelike.Difficulty.Normal;
        //CreateManager();//TODO
        var worldNormal = CreateWorld(info:_info);
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
          Assert.Greater(chempNormalEnemies.Average(i=> i.Stats.Defense), chempEasyEnemies.Average(i => i.Stats.Defense));
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
    public void TestDifficultyPits()
    {
      var easyBosses = GetBosses(Difficulty.Easy).OrderBy(i=>i.Level).ToList();
      var normalBosses = GetBosses(Difficulty.Normal).OrderBy(i => i.Level).ToList();
      var hardBosses = GetBosses(Difficulty.Hard).OrderBy(i => i.Level).ToList();
      Assert.AreEqual(easyBosses.Count, normalBosses.Count);
      int checkedCount = 0;
      foreach (var easyBoss in easyBosses)
      {
        var normalBoss = normalBosses.Where(i=>i.Name == easyBoss.Name).Single();
        //var normalBoss = normalBosses.Where(i => i.Level == easyBoss.Level).Single();
        if (normalBoss.Level == easyBoss.Level)
        {
          Assert.Greater(normalBoss.Stats.Defense, easyBoss.Stats.Defense);
          checkedCount++;
        }

        var hardBoss = hardBosses.Where(i => i.Name == easyBoss.Name).Single();
        //var hardBoss = hardBosses.Where(i => i.Level == easyBoss.Level).Single();
        if (normalBoss.Level == hardBoss.Level)
        {
          Assert.Greater(hardBoss.Stats.Defense, normalBoss.Stats.Defense);
          checkedCount++;
        }
      }

      Assert.Greater(checkedCount, 0);
    }

    [Test]
    [Repeat(1)]
    public void LevelAtGameEnd()
    {
      bool newGame = true;
      var info = new OuaDII.Generators.GenerationInfo();
      var size = 100;
      info.MinNodeSize = new System.Drawing.Size(size, size);
      info.MaxNodeSize = info.MinNodeSize;

      var worldEnemiesCount = 200;
      info.Counts.WorldEnemiesCount = worldEnemiesCount;

      var world = CreateWorld(newGame, info);
      Assert.AreEqual(world.Width, size);

      var worldEnemies = world.GetTiles<Enemy>();
      Assert.GreaterOrEqual(worldEnemies.Count, worldEnemiesCount);
      Assert.LessOrEqual(worldEnemies.Count, worldEnemiesCount + 10);

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
          Assert.Greater(GameManager.Hero.NextLevelExperience, nextLevelUpExp*2);
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

        pits.Add(new Tuple<DungeonPit, Stairs>( pit, stair));
      }
      return pits;
    }

    [Test]
    public void EnsureBossAtPitLastLevel()
    {

      for (int j = 0; j < 5; j++)
      {
        if (GameManager != null)
          GameManager.DisconnectEvents();
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
          var eqsBefore = world.GetTiles<GodStatue>();
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
    }

    Stairs GotoLastLevel(DungeonPit pit, Stairs pitEntry)
    {
      Stairs stairsPitUp = null;
      GameManager.InteractHeroWith(pitEntry);
      for (int i = 0; i <= pit.LevelGenerator.MaxLevelIndex; i++)
      {
        if (i == 0)
          stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);
        var stairsOnLevel = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
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
    public void EnemyPower()
    {
      var _info = new OuaDII.Generators.GenerationInfo();
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
    public void BoosRoomTest()
    {
      DungeonPit pit = GotoNonQuestPit(null);
      int MaxLevelIndex = new OuaDII.Generators.GenerationInfo().MaxLevelIndex;
      //goto last level
      for (int i = 0; ; i++)
      {
        //Assert.NotNull(pit.Levels[i].GetTiles<Hero>());

        var stairsDown = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
        GameManager.InteractHeroWith(stairsDown);

        if (i == MaxLevelIndex)
          break;
      }

      var boss = AllEnemies.Cast<Enemy>().Where(e => e.PowerKind == Roguelike.Tiles.LivingEntities.EnemyPowerKind.Boss).ToList();
      Assert.AreEqual(boss.Count, 1);
      var chests = GameManager.CurrentNode.GetTiles<Chest>().Where(i => i.ChestKind == ChestKind.GoldDeluxe).ToList();
      Assert.AreEqual(chests.Count, 1);
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
    public void EnemyNearStartOftenWounded()
    {
      var world = CreateWorld();
      var enemies = world.GetTiles<Enemy>();

      Assert.Greater(enemies.Count, 10);

      var byDistance = enemies.OrderBy(i => i.DistanceFrom(GameManager.Hero)).Take(10);
      var wounded = byDistance.Where(i => i.IsWounded).ToList();
      Assert.GreaterOrEqual(wounded.Count, 4);
      Assert.Less(wounded.Count, 10);
    }

    [Test]
    [Repeat(3)]
    public void EachLevelHasSecretRoom()
    {
      //for (int worldIndex = 0; worldIndex < 1; worldIndex++)
      {
        var world = CreateWorld();

        var stairsPitDown = world.GetAllStairs(StairsKind.PitDown);

        foreach (var stair in stairsPitDown)
        {
          Stairs stairsPitUp = null;
          var pit = world.GetPit(stair.PitName);
          if (pit.QuestKind != OuaDII.Quests.QuestKind.Unset)
            continue;

          GameManager.InteractHeroWith(stair);
          for (int levelIndex = 0; levelIndex <= pit.LevelGenerator.MaxLevelIndex; levelIndex++)
          {
            if (levelIndex == 0)
              stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);

            var secretNode = GameManager.CurrentNode.Nodes.FirstOrDefault(i => i.Secret);
            var doors = GameManager.CurrentNode.GetTiles<Door>();
            Assert.NotNull(secretNode);
            Assert.True(doors.Where(k => k.Secret).Any());
            var stairsDown = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
            GameManager.InteractHeroWith(stairsDown);
          }

          //go back
          GameManager.InteractHeroWith(stairsPitUp);
          Assert.AreEqual(world, GameManager.CurrentNode);
        }
      }
    }
  }
}
