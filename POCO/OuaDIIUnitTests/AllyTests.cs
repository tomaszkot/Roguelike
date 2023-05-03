using NUnit.Framework;
using OuaDII.Generators;
using OuaDII.TileContainers;
using Roguelike.LootContainers;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class AllyTests : TestBase
  {
    [Test]
    public void TestAllyCanLevelUpByPhysicalHits()
    {
      var info = new GenerationInfo();
      info.Counts.WorldEnemiesCount = 50;
      CreateWorld(true, info);
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.True(hero.Experience == 0 && hero.Level == 1);
      var ally = AddAlly(hero, true);
      ally.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Strength, 20);
      Assert.AreEqual(ally.Level, 1);
      Assert.AreEqual(ally.LevelUpPoints, 0);
      Assert.AreEqual(ally.Experience, 0);
      Assert.Greater(ally.NextLevelExperience, 0);
      var enemies = GameManager.CurrentNode.GetTiles<Enemy>();
      foreach (var en in enemies)
      {
        en.Level = 1;//to calc easily
        while (en.Alive)
        {
          en.OnMeleeHitBy(ally);
          GotoNextHeroTurn();
        }
      }

      Assert.True(hero.Experience > 0 || hero.Level > 1);//hero also gets some exp
      Assert.Greater(ally.Experience, GenerationInfo.FirstNextLevelExperienceThreshold);
      Assert.Greater(ally.Level, 2);
      Assert.Less(ally.Level, 6);
      Assert.Greater(ally.LevelUpPoints, 0);
    }

    [Test]
    public void TestHoundAllyLevel()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Level, 1);
      var ally1 = AddAlly(hero, false);
      Assert.AreEqual(ally1.Level, 1);
      Assert.Greater(ally1.NextLevelExperience, 50);
      Assert.Less(ally1.NextLevelExperience, 300);

      //Assert.Greater(ally1.Stats.MeleeAttack, 4);
      //Assert.AreEqual(ally1.Stats.Health, 10);
      //Assert.AreEqual(ally1.Stats.Defense, 6);

      GotoNextHeroTurn();

      //var magic = hero.Stats.GetNominal(Roguelike.Attributes.EntityStatKind.Magic);
      //hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Magic, magic * 3);//TODO *3
      hero.Level = 4;
      var ally2 = AddAlly(hero, false, true);
      Assert.Greater(ally2.Level, 1);
      Assert.Greater(ally2.NextLevelExperience, ally1.NextLevelExperience);

      Assert.Greater(ally2.Stats.MeleeAttack, ally1.Stats.MeleeAttack);
      Assert.Greater(ally2.Stats.Health, ally1.Stats.Health);
      Assert.Greater(ally2.Stats.Defense, ally1.Stats.Defense);
    }

    readonly float InitAllyPower = LivingEntity.StartStatValues[Roguelike.Attributes.EntityStatKind.Strength] + SkeletonSpell.SkeletonSpellStrengthIncrease;

    [Test]
    [TestCase(Roguelike.Difficulty.Easy)]
    [TestCase(Roguelike.Difficulty.Normal)]
    public void TestSkeletonAttackIncWithStrength(Roguelike.Difficulty diff)
    {
      GenerationInfo.Difficulty = diff;
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var ally1 = AddAlly(hero, true);
      ally1.RecalculateStatFactors(false);
      Assert.AreEqual(ally1.Level, 1);
      if (diff == Roguelike.Difficulty.Easy)
      {
        Assert.AreEqual(ally1.Stats.Strength, InitAllyPower);
        Assert.AreEqual(ally1.Stats.MeleeAttack, InitAllyPower);
      }
      else
      {
        if (ally1.IncreaseStatsDueToDifficulty)
        {
          Assert.Greater(ally1.Stats.Strength, InitAllyPower);
          Assert.Greater(ally1.Stats.MeleeAttack, InitAllyPower);
        }
      }
      Assert.AreEqual(ally1.Stats.MeleeAttack, ally1.Stats.Strength);
      var initStr = ally1.Stats.Strength;
      ally1.LevelUpPoints = 1;
      ally1.IncreaseStatByLevelUpPoint(Roguelike.Attributes.EntityStatKind.Strength);
      Assert.AreEqual(ally1.Stats.Strength, initStr + 1);
      Assert.AreEqual(ally1.Stats.MeleeAttack, initStr + 1);
    }

    [Test]
    public void TestSkeletonAllyLevel()
    {
      GenerationInfo.Difficulty = Roguelike.Difficulty.Easy;
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Level, 1);
      hero.Stats.GetStat(Roguelike.Attributes.EntityStatKind.Mana).Value.Nominal = 300;
      var ally1 = AddAlly(hero, true);
      
      Assert.AreEqual(ally1.CurrentEquipment.InvBasketKind, InvBasketKind.AllyEquipment);
      Assert.AreEqual(ally1.Inventory.InvBasketKind, InvBasketKind.Ally);

      Assert.AreEqual(ally1.Stats.Strength, InitAllyPower);
      Assert.AreEqual(ally1.Stats.MeleeAttack, InitAllyPower);
      Assert.AreEqual(ally1.Level, 1);
      Assert.AreEqual(ally1.Stats.Health, LivingEntity.StartStatValues[Roguelike.Attributes.EntityStatKind.Health]);
      Assert.Greater(ally1.NextLevelExperience, 50);
      Assert.Less(ally1.NextLevelExperience, 300);

      GotoNextHeroTurn();

      GameManager.AlliesManager.AllEntities.Clear();//TODO

      var magic = hero.Stats.GetNominal(Roguelike.Attributes.EntityStatKind.Magic);
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Magic, magic * 3);//TODO *3
      var ally2 = AddAlly(hero, true, true);
      Assert.Greater(ally2.Level, 1);
      Assert.Greater(ally2.Stats.MeleeAttack, ally1.Stats.MeleeAttack);
      Assert.Greater(ally2.Stats.Health, ally1.Stats.Health);
      Assert.Greater(ally2.NextLevelExperience, ally1.NextLevelExperience);

      var ab = hero.GetPassiveAbility(Roguelike.Abilities.AbilityKind.SkeletonMastering);
      ab.IncreaseLevel(hero);
      GotoNextHeroTurn();
      var ally3 = AddAlly(hero, true, true);
      Assert.AreEqual(GameManager.AlliesManager.AllEntities.Count, 2);
      Assert.Greater(ally3.Level, 1);
      Assert.Greater(ally3.Stats.MeleeAttack, ally2.Stats.MeleeAttack);
      Assert.Greater(ally3.Stats.Defense, ally2.Stats.Defense);

    }

    [Test]
    public void TestIfAllyAliveAfterSaveLoad()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Name = "Hero";
      var ally = AddAlly(hero, true);

      GameManager.Save();
      GameManager.Load(hero.Name);
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 1);
      ally = GameManager.AlliesManager.AllAllies.First() as Ally;
      Assert.Less((ally as Ally).DistanceFrom(hero), 5);
      Assert.AreEqual(ally.Name, "Skeleton");

      CreateWorld();
      var hero1 = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero1.Name = "Hero1";
      GameManager.Save();
      GameManager.Load(hero1.Name);
      Assert.AreEqual(GameManager.Hero.Name, "Hero1");
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 0);

      GameManager.Load(hero.Name);
      Assert.AreEqual(GameManager.Hero.Name, "Hero");
      Assert.AreEqual(GameManager.AlliesManager.AllAllies.Count(), 1);
    }

    private Ally AddAlly(OuaDII.Tiles.LivingEntities.Hero hero, bool skeleton, bool second = false)
    {
      var am = GameManager.AlliesManager;
      //Assert.AreEqual(am.AllEntities.Count, second ? 1 : 0);

      if (skeleton)
      {
        var scroll = new Scroll(SpellKind.Skeleton);
        hero.Inventory.Add(scroll);
        Assert.NotNull(GameManager.SpellManager.ApplySpell(hero, scroll));
      }
      else
      {
        var hound = Container.GetInstance<Roguelike.Tiles.LivingEntities.TrainedHound>();
        GameManager.AddAlly(hound);
      }

      //Assert.AreEqual(am.AllEntities.Count, second ? 2 : 1);
      var ally = am.AllAllies.Last() as Ally;
      Assert.AreEqual(ally.Name, skeleton ? "Skeleton" : "Hound");
      Assert.Less(ally.DistanceFrom(hero), 5);
      return ally as Ally;
    }

    [Test]
    public void TestIfAllyPresentAddedInDungeonGoBackToWorld()
    {
      CreateWorld();
            
      InteractHeroWithPit(OuaDII.Quests.QuestKind.CrazyMiller);

      AddAlly(GameManager.OuadHero);

      AssertAllyInPit();

      var stairsPitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);
      GameManager.InteractHeroWith(stairsPitUp);
            
      Assert.True(GameManager.CurrentNode is World);
      Assert.AreEqual(GetAlliesCountOnLevel(), 1);
      Assert.AreEqual(GameManager.AlliesManager.AllEntities.Count, 1);
      Assert.Less(GameManager.AlliesManager.AllEntities[0].DistanceFrom(GameManager.Hero), 6);
      
    }

    public override World CreateWorld(bool newGame = true, Dungeons.GenerationInfo info = null, string hn = "")
    {
      var res = base.CreateWorld(newGame, info);
      Assert.AreEqual(GameManager.AlliesManager.AllEntities.Count, 0);
      Assert.AreEqual(GetAlliesCountOnLevel(), 0);
      return res;
    }

    [Test]
    public void TestIfAllyAliveAfterSaveLoadInDungeon()
    {
      CreateWorld();

      AddAlly(GameManager.OuadHero);

      InteractHeroWithPit(OuaDII.Quests.QuestKind.CrazyMiller);

      AssertAllyInPit();

      GameManager.Save();
      GameManager.Load(GameManager.Hero.Name);
      Assert.True(GameManager.CurrentNode.GetTiles().Contains(GameManager.Hero));

      if (GameManager.GameSettings.Mechanics.RestoreHeroToDungeon)
      {
        Assert.False(GameManager.CurrentNode is World);
      }
      else
      {
        Assert.True(GameManager.CurrentNode is World);
        InteractHeroWithPit(OuaDII.Quests.QuestKind.CrazyMiller);
        AssertAllyInPit();
      }
            
      Assert.AreEqual(GetAlliesCountOnLevel(), 1);
      Assert.AreEqual(GameManager.AlliesManager.AllEntities.Count, 1);

      var allyAfterLoad = GameManager.AlliesManager.AllAllies.First() as Ally;
      Assert.Less(allyAfterLoad.DistanceFrom(GameManager.Hero), 6);
    }

    private void AssertAllyInPit()
    {
      Assert.False(GameManager.CurrentNode is World);
      Assert.AreEqual(GetAlliesCountOnLevel(GameManager.World), 0);

      Assert.AreEqual(GameManager.AlliesManager.AllEntities.Count, 1);
      Assert.AreEqual(GetAlliesCountOnLevel(), 1);
    }

    private void AddAlly(OuaDII.Tiles.LivingEntities.Hero hero)
    {
      var ally = AddAlly(hero, true);
      Assert.AreEqual(GameManager.AlliesManager.AllEntities.Count, 1);
      Assert.AreEqual(GetAlliesCountOnLevel(), 1);
      Assert.AreEqual(GameManager.Hero, hero);
    }

    private int GetAlliesCountOnLevel()
    {
      return GetAlliesCountOnLevel(GameManager.CurrentNode);
    }

    private int GetAlliesCountOnLevel(AbstractGameLevel level)
    {
      return level.GetTiles<LivingEntity>().Where(i => i is Roguelike.Abstract.Tiles.IAlly _ally && _ally.Active).ToList().Count;
    }

    [Test]
    public void TestIfAllyHealSelf()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(GetAlliesCountOnLevel(), 0);
      var ally = AddAlly(hero, true);

      ally.Inventory.Add(new Potion() { Kind = PotionKind.Health });

      var en = GameManager.CurrentNode.SpawnEnemy(1);
      var initAllyHealth = ally.Stats.Health;
      while (ally.Stats.Health > initAllyHealth / 3)
        ally.OnMeleeHitBy(en);

      var allyHurtHealth = ally.Stats.Health;
      GotoNextHeroTurn();
      Assert.Greater(ally.Stats.Health, allyHurtHealth);
      Assert.False(ally.Inventory.Items.Any());
    }
  }
}
