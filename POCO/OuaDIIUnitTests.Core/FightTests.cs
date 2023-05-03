using NUnit.Framework;
using OuaDII.TileContainers;
using Roguelike;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class FightTests : TestBase
  {
    float reqMin = 50;
    float reqMax = 94;

    [Test]
    public void TestReqStatsAxes()
    {
      var game = CreateWorld();
      var names = new[] { "sickle", "hatchet", "axe", "solid_axe", "double_axe", "war_axe" };

      
      CheckReqStats(names, reqMin, reqMax, EntityStatKind.Strength);
    }

    [Test]
    public void TestReqStatsBashing()
    {
      var game = CreateWorld();
      var names = new[] { "club", "spiked_club", "enhanced_club", "power_club", "hammer", "solid_hammer" };

    
      CheckReqStats(names, reqMin, reqMax, EntityStatKind.Strength);
    }

    [Test]
    public void TestMinWpnPower()
    {
      var game = CreateWorld();
      var names = new[] { "club", "sickle", "needle", "rusty_sword",  "crude_bow", "crude_crossbow", "fire_scepter1", "fire_wand1", "fire_staff1" };
      var hero = GameManager.Hero;
      hero.Stats.GetStat(EntityStatKind.Strength).Value.Nominal = 100;
      Dictionary<string, float> name2Power = new Dictionary<string, float>();
      var weapons = new Dictionary<string, Weapon>();
      foreach (var name in names)
      {
        var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>(name);
        name2Power[name] = wpn.Damage;
        weapons[name] = wpn;
      }
      Assert.GreaterOrEqual(weapons["sickle"].Damage, weapons["rusty_sword"].Damage);
      Assert.Greater(weapons["rusty_sword"].Damage, weapons["club"].Damage);
      Assert.GreaterOrEqual(weapons["club"].Damage, weapons["needle"].Damage);

      Assert.GreaterOrEqual(weapons["club"].Damage, weapons["fire_scepter1"].Damage);
      Assert.GreaterOrEqual(weapons["club"].Damage, weapons["fire_wand1"].Damage);
      Assert.GreaterOrEqual(weapons["sickle"].Damage, weapons["fire_staff1"].Damage);

      float melee = CalcMeleeDamage(weapons["rusty_sword"].Damage);
      Assert.Greater(melee, weapons["crude_bow"].Damage);
      Assert.Greater(melee, weapons["crude_crossbow"].Damage);

      Assert.Greater(weapons["club"].Damage, 0);
    }

    [Test]
    public void TestMaxWpnPower()
    {
      var game = CreateWorld();
      var names = new[] { "war_dagger", "war_hammer", "war_sword", "war_axe" };
      //"war_crossbow", "war_bow"
      var hero = GameManager.Hero;
      hero.Stats.GetStat(EntityStatKind.Strength).Value.Nominal = 100;
      Dictionary<string, float> name2Power = new Dictionary<string, float>();
      var weapons = new Dictionary<string, Weapon>();
      foreach (var name in names)
      {
        var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>(name);
        name2Power[name] = wpn.Damage;
        weapons[name] = wpn;
      }
      var maxMelee = name2Power.Where(i => i.Value == name2Power.Max(j => j.Value)).FirstOrDefault();
      var minMelee = name2Power.Where(i => i.Value == name2Power.Min(j => j.Value)).FirstOrDefault();
      var ratio = maxMelee.Value / minMelee.Value;
      Assert.Greater(ratio, 1.8f);
      Assert.Less(ratio, 2.5f);
      Assert.AreEqual(maxMelee.Key, "war_hammer");

      Assert.Greater(weapons["war_axe"].Damage, weapons["war_sword"].Damage);
      Assert.Greater(weapons["war_sword"].Damage, weapons["war_dagger"].Damage);

      float maxMel = CalcMeleeDamage(maxMelee.Value);
      //BowLike
      {
        names = new[] { "war_bow", "war_crossbow" };
        Dictionary<string, float> name2PowerBowLike = new Dictionary<string, float>();
        foreach (var name in names)
        {
          var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>(name);
          name2PowerBowLike[name] = wpn.Damage;
        }
        var maxBowLike = name2PowerBowLike.Where(i => i.Value == name2PowerBowLike.Max(j => j.Value)).FirstOrDefault();

        Assert.Greater(maxMel, maxBowLike.Value);
      }
      //magic
      {
        names = new[] { "fire_scepter11", "fire_wand11", "fire_staff11" };
        Dictionary<string, float> name2PowerMagic = new Dictionary<string, float>();
        foreach (var name in names)
        {
          var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>(name);
          name2PowerMagic[name] = wpn.Damage;
        }
        var maxMagic = name2PowerMagic.Where(i => i.Value == name2PowerMagic.Max(j => j.Value)).FirstOrDefault();
        Assert.Greater(maxMagic.Value, 10);
        Assert.Greater(maxMel, maxMagic.Value);
        //var minBowLike = name2PowerBowLike.Where(i => i.Value == name2PowerBowLike.Min(j => j.Value)).FirstOrDefault();
      }
    }

    private static float CalcMeleeDamage( float maxMelee)
    {
      var leStr = LivingEntity.StartStatValues[Roguelike.Attributes.EntityStatKind.Strength];
      var heroStr_ = Hero.GetStrengthStartStat();
      var heroStr = leStr + heroStr_;
      var maxMel = maxMelee + heroStr;
      return maxMel;
    }

    [Test]
    public void TestReqStatsBows()
    {
      var game = CreateWorld();
      var names = new[] { "crude_bow", "bow", "solid_bow", "composite_bow", "war_bow" };
            

      CheckReqStats(names, reqMin, reqMax, EntityStatKind.Dexterity);
    }

    [Test]
    public void TestReqStatsCrossBows()
    {
      var game = CreateWorld();

      var names = new[] { "crude_crossbow", "crossbow", "solid_crossbow", "composite_crossbow", "war_crossbow" };


      CheckReqStats(names, reqMin, reqMax, EntityStatKind.Dexterity);
    }

    private float CheckReqStats(string[] names, float reqMin, float reqMax, EntityStatKind esk)
    {
      float req = 0;
      var weapons = new List<Weapon>();
      foreach (var name in names)
      {
        var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>(name);
        var nextReq = wpn.GetReqStatValue(esk);
        Assert.Greater(nextReq, req);
        req = nextReq;
        weapons.Add(wpn);
      }

      Assert.Greater(req, reqMin);
      Assert.Less(req, reqMax);
      return req;
    }


    [Test]
    public void TestAttackDescriptionMelee()
    {
      var game = CreateWorld();
      var hero = GameManager.Hero;
      hero.UseAttackVariation = false;
      Assert.Null(hero.GetActiveWeapon());

      var ad = new AttackDescription(hero, false);
      Assert.AreEqual(ad.CurrentTotal, hero.Stats.Strength);
      var wpn = GetTestSword();
      SetHeroEquipment(wpn);

      var ad1 = new AttackDescription(hero, false);
      Assert.Greater(ad1.CurrentTotal, ad.CurrentTotal);
      Assert.AreEqual(ad1.CurrentTotal, hero.Stats.Strength+ wpn.Damage);

      var wpnElementalDamage = 2;
      wpn = GetTestSword(wpnElementalDamage, EntityStatKind.FireAttack);
      SetHeroEquipment(wpn);
      var ad2 = new AttackDescription(hero, false);
      Assert.Greater(ad2.CurrentTotal, ad1.CurrentTotal);
      Assert.AreEqual(ad2.CurrentTotal, hero.Stats.Strength + wpn.Damage + wpnElementalDamage);

      for (int i = 0; i < 5; i++)
        hero.IncreaseAbility(AbilityKind.SwordsMastering);

      var ad4 = new AttackDescription(hero, false);
      Assert.Greater(ad4.CurrentTotal, ad2.CurrentTotal);
      Assert.Greater(ad2.CurrentTotal * 2, ad4.CurrentTotal);
    }

    AttackDescription createHeroAttackDescription(AttackKind kind, OffensiveSpell spell = null)
    {
      return new AttackDescription(GameManager.Hero, false, kind, spell);
    }

    [Test]
    public void TestAttackDescriptionBow()
    {
      var game = CreateWorld();
      var hero = GameManager.Hero;
      Assert.Null(hero.GetActiveWeapon());
      Assert.Null(hero.ActiveFightItem);

      var adMelee = createHeroAttackDescription(AttackKind.Melee);

      var ad = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.AreEqual(ad.CurrentTotal, 0);

      //add bow but no arrows!
      var wpn = GetTestBow();
      SetHeroEquipment(wpn);

      var adMelee0 = createHeroAttackDescription(AttackKind.Melee);
      //no arrows!
      Assert.AreEqual(adMelee.CurrentTotal, adMelee0.CurrentTotal);

      Assert.Null(hero.ActiveFightItem);

      var ad1 = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.AreEqual(ad1.CurrentTotal, 0);

      Assert.Null(hero.ActiveFightItem);
      var fi = new ProjectileFightItem(FightItemKind.PlainArrow, hero);
      fi.Count = 10;
      hero.Inventory.Add(fi);
      Assert.NotNull(hero.ActiveFightItem);

      var ad2 = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.Greater(ad2.CurrentTotal, 0);
      Assert.AreEqual(ad2.CurrentTotal, wpn.Damage + hero.ActiveFightItem.Damage);

      wpn = GetTestBow(2, EntityStatKind.FireAttack );
      Assert.True(SetHeroEquipment(wpn));

      var ad3 = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.Greater(ad3.CurrentTotal, 0);
      Assert.AreEqual(ad3.CurrentTotal, wpn.Damage + hero.ActiveFightItem.Damage + 2);

      for (int i = 0; i < 5; i++)
        hero.IncreaseAbility(AbilityKind.BowsMastering);

      var ad4 = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.Greater(ad4.CurrentTotal, ad3.CurrentTotal);
      Assert.Greater(ad3.CurrentTotal*2, ad4.CurrentTotal);

      var adMelee1 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee.CurrentTotal, adMelee1.CurrentTotal);
    }

    [Test]
    public void TestDescriptionOfMagicalWeapons()
    {
      var game = CreateWorld();
      var hero = GameManager.Hero;
      var wpn = GetTestWeapon("fire_scepter1");
            
      var wss = new WeaponSpellSource(wpn, SpellKind.FireBall);
      var ex1 = wss.GetExtraStatDescriptionFormatted(hero);
      Assert.AreEqual(wpn.LevelIndex, 1);
      Assert.True(ex1.Contains("Level: 1"));
    }

    [Test]
    public void TestAttackDescriptionMagicalWeaponConsistent()
    {
      var game = CreateWorld();
      var hero = GameManager.Hero;
      hero.UseAttackVariation = false;
      var adElem = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      Assert.AreEqual(adElem.CurrentTotal, 0);
      var wpn = GetTestWeapon("fire_scepter1");
      Assert.True(SetHeroEquipment(wpn));

      var ad1 = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      var fireAtt = ad1.NonPhysical[EntityStatKind.FireAttack];
      Assert.Greater(fireAtt, 0);
      for (int i = 0; i < 10; i++)
      {
        var adNext = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
        Assert.AreEqual(fireAtt, adNext.NonPhysical[EntityStatKind.FireAttack]);
      }
    }

    const int baseElementalDamage = OffensiveSpell.BaseDamage + OffensiveSpell.DefaultAddNominal+1;

    [Test]
    public void TestAttackDescriptionMagical()
    {
      var game = CreateWorld();
      var hero = GameManager.Hero;
      hero.UseAttackVariation = false;
      Assert.Null(hero.GetActiveWeapon());
      Assert.Null(hero.ActiveFightItem);

      var adElem = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      Assert.AreEqual(adElem.CurrentTotal, 0);

      var adMelee = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee.CurrentTotal, hero.Stats.Strength);
            
      var wpn = GetTestWeapon("fire_scepter1");
      Assert.True(SetHeroEquipment(wpn));

      var adMelee1 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee1.CurrentTotal, hero.Stats.Strength + wpn.Damage);
      var adElem1 = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      

      Assert.AreEqual(adElem1.CurrentTotal, baseElementalDamage);

      wpn = GetMagicalWeapon(2, "fire_scepter1", EntityStatKind.FireAttack);
      Assert.True(SetHeroEquipment(wpn));
            
      //both melee and proj shall be affected
      var adMelee2 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee2.CurrentTotal, hero.Stats.Strength + wpn.Damage + 2);
      var adElem2 = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      Assert.AreEqual(adElem2.CurrentTotal, baseElementalDamage + 2);//+2 => FireAttack

      wpn = GetTestWeapon("fire_scepter1");
      AddAttackFromGem(wpn, GemKind.Emerald);
      Assert.True(SetHeroEquipment(wpn));

      var adMelee3 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee3.CurrentTotal, adMelee1.CurrentTotal+1);// 1 from gem
      var adElem3 = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      Assert.AreEqual(adElem3.CurrentTotal, adElem1.CurrentTotal /*+ 1*/);//not matching elemental

      for (int i = 0; i < 5; i++)
        hero.IncreaseAbility(AbilityKind.SceptersMastering);

      var adMelee4 = createHeroAttackDescription(AttackKind.Melee);
      Assert.Greater(adMelee4.CurrentTotal, adMelee3.CurrentTotal);
      var adElem4 = createHeroAttackDescription(AttackKind.WeaponElementalProjectile);
      Assert.Greater(adElem4.CurrentTotal, adElem3.CurrentTotal);//for magical weapons projectiles are also affected by ability
    }

    [Test]
    public void TestAttackFireBall()
    {
      var game = CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var ad1 = createHeroAttackDescription(AttackKind.SpellElementalProjectile);
      Assert.AreEqual(ad1.CurrentTotal, 0);

      var loot = new Scroll(SpellKind.FireBall);
      Assert.True(hero.Inventory.Add(loot));
      Assert.True(hero.ShortcutsBar.SetAt(1, loot));
      Assert.AreEqual(hero.ActiveManaPoweredSpellSource, loot);

      var ad2 = createHeroAttackDescription(AttackKind.SpellElementalProjectile, loot.CreateSpell<OffensiveSpell>(hero));
      
      //Assert.AreEqual(ad2.CurrentTotal, baseElementalDamage);
      Assert.Greater(ad2.CurrentTotal, 0);
      Assert.Less(ad2.CurrentTotal, 3);

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(SetHeroEquipment(rusty));

      var ad3 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(ad3.CurrentTotal, hero.Stats.Strength + rusty.Damage);

      var ad4 = createHeroAttackDescription(AttackKind.SpellElementalProjectile, loot.CreateSpell<OffensiveSpell>(hero));
      Assert.AreEqual(ad4.CurrentTotal, ad2.CurrentTotal);
    }

    [TestCase(FightItemKind.Stone)]
    [TestCase(FightItemKind.ThrowingKnife)]
    [TestCase(FightItemKind.ThrowingTorch)]
    public void TestAttackThrowingItems(FightItemKind fik)
    {
      var game = CreateWorld();
      var hero = GameManager.Hero;
      Assert.Null(hero.GetActiveWeapon());
      Assert.Null(hero.ActiveFightItem);

      var adMelee1 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee1.CurrentTotal, hero.Stats.Strength);

      var adProj1 = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.AreEqual(adProj1.CurrentTotal, 0);

      var fi = new ProjectileFightItem(fik, hero);
      fi.Count = 10;
      hero.Inventory.Add(fi);
      Assert.NotNull(hero.ActiveFightItem);
      var adMelee2 = createHeroAttackDescription(AttackKind.Melee);
      Assert.AreEqual(adMelee2.CurrentTotal, adMelee1.CurrentTotal);

      var adProj2 = createHeroAttackDescription(AttackKind.PhysicalProjectile);
      Assert.AreEqual(adProj2.CurrentTotal, adMelee1.CurrentTotal/2+fi.Damage);

    }

    [Test]
    public void TestMagicalProps()
    {
      var game = CreateWorld();
      var initCount = 0;
      float initReqMagic = 0;
      var lg = GameManager.LootGenerator;
      {
        int max = 15;
        bool maxTested = false;
        for (int i = 0; i < max; i++)
        {
          var name = "fire_scepter" + (i + 1);
          var wpn = lg.GetLootByTileName<Weapon>(name);
          Assert.NotNull(wpn);
          Assert.AreEqual(wpn.Name, "FireBall Scepter");
          Assert.AreEqual(wpn.LevelIndex, (i+1));
          Assert.NotNull(wpn.SpellSource);
          var spellCount = wpn.SpellSource.Count;
          if (i == 0)
          {
            initCount = spellCount;
            initReqMagic = wpn.RequiredStats.Magic;
          }

          if (i == max - 1)
          {
            Assert.Less(initCount, spellCount);
            Assert.Less(initReqMagic, wpn.RequiredStats.Magic);
            maxTested = true;
          }
        }

        Assert.True(maxTested);
      }
    }

    [Test]
    public void TestMagicalWeaponPowerByLevel()
    {
      var game = CreateWorld();
      var lg = GameManager.LootGenerator;
      var price = 0;
      {
        float wpnSpellDamage = 0;
        {
          var wpn = lg.GetLootByTileName<Weapon>("fire_scepter1");
          Assert.NotNull(wpn.SpellSource);
          var wss = wpn.SpellSource as WeaponSpellSource;
          Assert.AreEqual(wss.Level, 1);
          var spell = wpn.SpellSource.CreateSpell();
          var wpnOffensiveSpell = spell as OffensiveSpell;
          Assert.Greater(wpnOffensiveSpell.Damage, 0);
          wpnSpellDamage = wpnOffensiveSpell.Damage;
          price = wpn.Price;
        }
        const int nextLevel = 2;
        var eq = lg.GetRandomLoot(LootKind.Equipment, nextLevel);
        var fs1 = lg.GetLootByAsset("fire_scepter1");
        var fs2 = lg.GetLootByAsset("fire_scepter2");

        while (true)
        {
          if (eq is Weapon wpn1 && wpn1.IsMagician)
          {
            var wss1 = (wpn1.SpellSource as WeaponSpellSource);
            Assert.AreEqual(wss1.Level, nextLevel);
            var spell1 = wpn1.SpellSource.CreateSpell();
            var offSpell1 = spell1 as OffensiveSpell;
            Assert.Greater(offSpell1.Damage, wpnSpellDamage);

            var ex1 = wss1.GetExtraStatDescriptionFormatted(GameManager.Hero);
            Assert.AreEqual(wpn1.LevelIndex, nextLevel);
            Assert.True(ex1.Contains("Level: "+ nextLevel));
            if (eq.Price <= price)
            {
              int k = 0;
              k++;
            }
            Assert.Greater(eq.Price, price);
            wpnSpellDamage = offSpell1.Damage;
            price = wpn1.Price;
            break;
          }
          else 
            eq = lg.GetRandomLoot(LootKind.Equipment, nextLevel);
        }
      }
    }

    //Done by other test
    //[Test]
    //public void TestMagicalWeaponPowerVariesABit()
    //{
    //  var game = CreateWorld();
    //  var lg = GameManager.LootGenerator;
    //  {
    //    var wpn = lg.GetLootByTileName<Weapon>("fire_scepter1");
    //    SetHeroEquipment(wpn);
    //    var damages = new List<float>();
    //    for (int i = 0; i < 10; i++)
    //    {
    //      Assert.NotNull(wpn);
    //      Assert.NotNull(wpn.SpellSource);
    //      var wss = wpn.SpellSource as WeaponSpellSource;
    //      Assert.AreEqual(wss.Level, 1);
          
    //      //var spell = wpn.SpellSource.CreateSpell(GameManager.Hero);
    //      //var offSpell = spell as OffensiveSpell;
    //      //var desc = spell.CreateSpellStatsDescript(true, true);
    //      //Assert.Greater(desc.Damage, 0);
    //      //Assert.AreEqual(desc.Damage, offSpell.Damage);
    //      //damages.Add(offSpell.Damage);
          
    //      //check desc
    //      var descStrings = desc.GetDescription(false);
    //      Assert.AreEqual(descStrings.Count(), 2);
    //      Assert.AreEqual(descStrings[0], "FireBall Damage: " + offSpell.Damage);
    //      Assert.True(descStrings[1].Contains("Range"));
    //    }
    //    var grouped = damages.GroupBy(i => i);
    //    Assert.Greater(grouped.Count() , 1);
    //    //wpn.SpellSource.FixedLevel
    //  }
    //}

    [Test]
    [Repeat(1)]
    public void TestMagicalWeapon()
    {
      var gi = CreateGenerationInfo();
      gi.Counts.WorldEnemiesCount = 50;
      var world = CreateWorld(true, gi);
      var lg = GameManager.LootGenerator;
      var heros = world.GetTiles<Hero>();
      Assert.AreEqual(heros.Count, 1);
      {
        var wpn = lg.GetLootByTileName<Weapon>("fire_scepter1");
        Assert.NotNull(wpn);
        Assert.NotNull(wpn.SpellSource);
        var spellCount = wpn.SpellSource.Count;
        Assert.Greater(spellCount, 5);
        //PrepareEntityForLongLiving(GameManager.Hero);

        var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
        hero.d_immortal = true;
        //hero.Stats.GetStat(EntityStatKind.Health).Value.Nominal = 300;
        var ens = GameManager.CurrentNode.GetTiles<Enemy>().Where(i => i.DistanceFrom(hero) < 6).ToList();

        Assert.IsNull(hero.ActiveSpellSource);
        SetHeroEq(wpn);
        var scepterCharges = Weapon.ScepterChargesCount.ToString();

        Assert.AreEqual(wpn.PrimaryStatDescription, "Emits FireBall charges\r\n(" + scepterCharges + "/" + scepterCharges + " charges available)");
        Assert.IsNotNull(hero.ActiveSpellSource);
        var ad = new AttackDescription(hero, true, AttackKind.WeaponElementalProjectile);
        Assert.Greater(ad.CurrentTotal, 0);
        Assert.Greater(Enemies.Count, 0);
        var enemy = Enemies.Cast<OuaDII.Tiles.LivingEntities.Enemy>().FirstOrDefault();
        PlaceCloseToHero(hero, enemy);
        GotoNextHeroTurn();

        var scepterChargesReduced = (Weapon.ScepterChargesCount - 1).ToString();
        for (int i = 0; i < spellCount; i++)
        {
          var enemyHealth = enemy.Stats.Health;
          Assert.True(UseSpellSource(hero, enemy, wpn.SpellSource) || wpn.SpellSource.Count == 0);
          if (enemy.Alive)
          {
            if(wpn.SpellSource.Count > 0)
              Assert.Less(enemy.Stats.Health, enemyHealth, "i = "+ i);
            if (i == 0)
              Assert.AreEqual(wpn.PrimaryStatDescription, "Emits FireBall charges\r\n(" + scepterChargesReduced + "/" + scepterCharges + " charges available)");
            //Assert.AreEqual(wpn.PrimaryStatDescription, "Emits FireBall charges\r\n(19/20 charges available)");
          }

          Assert.Less(wpn.SpellSource.Count, spellCount);
          Assert.GreaterOrEqual(wpn.SpellSource.Count, 0);
          GotoNextHeroTurn();
        }
        Assert.AreEqual(wpn.SpellSource.Count, 0);
        var lootToCraft = new List<Loot> { wpn, new MagicDust() };
        Craft(hero, lootToCraft, RecipeKind.RechargeMagicalWeapon);
        Assert.Greater(wpn.SpellSource.Count, spellCount / 2);

        var heros1 = world.GetTiles<Hero>();

        //save/load
        var wss = (wpn.SpellSource as WeaponSpellSource);
        var ssCount = wpn.SpellSource.Count;
        SaveLoad();
        hero = GameManager.OuadHero;
        wpn = hero.GetActiveWeapon();
        Assert.AreEqual(wpn.tag1, "fire_scepter1");
        var wssLoaded = (wpn.SpellSource as WeaponSpellSource);
        Assert.AreEqual(wssLoaded.Level, wss.Level);
        Assert.AreEqual(wssLoaded.Count, wss.Count);
        Assert.AreEqual(wssLoaded.Kind, wss.Kind);
      }
    }
        
    [Repeat(2)]
    [Test]
    public void EnemyPowerInPitsTest()
    {
      var world = CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var countAtLevel = world.Pits.Where(i => i.StartEnemiesLevel == 1).Count();
      Assert.LessOrEqual(countAtLevel, 2);
      var pitDowns = world.GetAllStairs(StairsKind.PitDown).OrderBy(i => i.DistanceFrom(hero)).ToList();
      Assert.Greater(pitDowns.Count, 1);//there shall be around 8 of them, meybe more
      
      DungeonPit prevPit = null;
      Stairs pitUp = null;
      foreach (var pitDown in pitDowns)
      {
        var pit = world.GetPit(pitDown);
        if (pit.QuestKind != OuaDII.Quests.QuestKind.Unset)
          continue;
        if (prevPit != null)
        {
          var diff = pit.StartEnemiesLevel - prevPit.StartEnemiesLevel;
          Assert.GreaterOrEqual(diff, 1);
        }
        GameManager.InteractHeroWith(pitDown);
        for (int i = 0; ; i++)
        {
          Enemy prevEn = null;
          if (i == 0)
            pitUp = GameManager.CurrentNode.GetStairs(StairsKind.PitUp);
          if (i == pit.Levels.Count)
            break;

          Assert.NotNull(pit.Levels[i].GetTiles<OuaDII.Tiles.LivingEntities.Hero>());

          var stairsDown = GameManager.CurrentNode.GetStairs(StairsKind.LevelDown);
          
          var enemy = PlainEnemies.FirstOrDefault();
          if (prevEn != null)
          {
            Assert.AreEqual(enemy.Level, prevEn.Level + 1);
          }

          prevEn = enemy;
          GameManager.InteractHeroWith(stairsDown);
        }
        prevPit = pit;
        GameManager.SetContext(world, hero, GameContextSwitchKind.DungeonSwitched, ()=> { }, pitUp);
        Assert.AreEqual(GameManager.CurrentNode, world);
      }
    }
        
    Weapon GetMagicalWeapon(int magicDamageAmount = 0, string name = "fire_scepter1", EntityStatKind esk = EntityStatKind.FireAttack)
    {
      return GetTestWeapon(name, magicDamageAmount, esk);
    }
        
    Weapon GetTestSword(int magicDamageAmount = 0, EntityStatKind esk = EntityStatKind.Unset)
    {
      return GetTestWeapon("rusty_sword", magicDamageAmount, esk);
    }
        
    [Test]
    [Repeat(1)]
    public void EquipmentImpactTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      Assert.Greater(PlainEnemies.Count, 0);
      var enemy = PlainEnemies.First();
      enemy.SetIsWounded(false);
      var enemyHealth = enemy.Stats.Health;
      var inflicted1 = enemy.OnMeleeHitBy(hero);
      Assert.Greater(inflicted1, 0);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;

      var lg = GameManager.LootGenerator;
      var wpn = GetTestSword();

      SetHeroEq(wpn);
      var inflicted2 = enemy.OnMeleeHitBy(hero);
      Assert.Greater(inflicted2, inflicted1);
      var diff = enemyHealth - enemy.Stats.Health;
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;

      var wpn1 = lg.GetLootByTileName<Weapon>("rusty_sword");
      wpn1.StableDamage = true;
      wpn1.MakeMagic(EntityStatKind.MeleeAttack, 2);
      wpn1.Identify();
      SetHeroEq(wpn1);
      //hero.RecalculateStatFactors(false);WTF?
      var inflicted3 = enemy.OnMeleeHitBy(hero);
      Assert.Greater(inflicted3, inflicted2);
      var diff1 = enemyHealth - enemy.Stats.Health;
      Assert.Greater(diff1, diff);
    }

    private void SetHeroEq(Weapon wpn)
    {
      PutEqOnLevelAndCollectIt(wpn);
      if (GameManager.Hero.CurrentEquipment.GetWeapon() != wpn)
        Assert.True(SetHeroEquipment(wpn));
    }

    public void PutEqOnLevelAndCollectIt(IEquipment eq)
    {
      PutLootOnLevel(eq as Loot);
      CollectLoot(eq as Loot);
    }

    public void PutLootOnLevel(Loot loot)
    {
      if (loot != null)
      {
        var tile = GameManager.CurrentNode.SetTileAtRandomPosition(loot);
        Assert.AreEqual(loot, tile);
      }
    }
       
    [Test]
    [Repeat(1)]
    public void EnemyWoundedLowerDefense()
    {
      var world = CreateWorld();

      var notWounded = AllEnemies.Where(i => !i.IsWounded).First();
      var oldDefense = notWounded.Stats.GetCurrentValue(EntityStatKind.Defense);
      var oldHealth = notWounded.Stats.Health;

      notWounded.SetIsWounded(true);

      Assert.Less(notWounded.Stats.GetCurrentValue(EntityStatKind.Defense), oldDefense);
      Assert.AreEqual(notWounded.Stats.Health, oldHealth);
      Assert.True(!notWounded.LastingEffects.Any());

      var hero = GameManager.Hero;
      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      SetHeroEquipment(rusty);

      PlaceCloseToHero(hero, notWounded);
      var inflicted = notWounded.OnMeleeHitBy(hero);
      Assert.Greater(inflicted, 0);
      Assert.Less(notWounded.Stats.Health, oldHealth);
      Assert.True(notWounded.LastingEffects.Any());

      oldHealth = notWounded.Stats.Health;
      GotoNextHeroTurn();
      Assert.Less(notWounded.Stats.Health, oldHealth);//bleeding shall apply
    }

    [TestCase("bow", "solid_bow")]
    [TestCase("crossbow", "solid_crossbow")]
    [Repeat(1)]
    public void ArrowFightItemTestWithBetterWeapon(string eqName1, string eqName2)
    {
      var world = CreateWorld();
      var hero = GameManager.Hero;
      MakeEntityLongLiving(hero, 600);
      var fi = new ProjectileFightItem(FightItemKind.PlainArrow, hero);
      fi.Count = 20;
      var hits = fi.Count / 2;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO

      var enemy = PlainEnemies.First();
      MakeEntityLongLiving(enemy, 600);
      var enemyHealth = enemy.Stats.Health;
      enemy.Stats.SetNominal(EntityStatKind.Defense, 10);
      enemy.AddImmunity(Roguelike.Effects.EffectType.Bleeding);//would corrupt test
      var mana = hero.Stats.Mana;

      Assert.True(GameManager.HeroTurn);
      var bow = GenerateEquipment<Weapon>(eqName1);
      
      Assert.True(SetHeroEquipment(bow));
      PlaceCloseToHero(hero, enemy);
            
      for (int i = 0; i < hits; i++)
      {
        Assert.True(UseFightItem(hero, enemy, fi));

        Assert.Greater(enemyHealth, enemy.Stats.Health);
        var dd = enemyHealth - enemy.Stats.Health;
        Assert.False(enemy.LastingEffects.Any());
        Assert.AreEqual(mana, hero.Stats.Mana);
        Assert.False(GameManager.HeroTurn);

        GotoNextHeroTurn();
        Assert.True(GameManager.HeroTurn);
        
      }
      var diff1 = enemyHealth - enemy.Stats.Health;
      Assert.Greater(diff1, 0);
      enemyHealth = enemy.Stats.Health;
      var bowSolid = GenerateEquipment<Weapon>(eqName2);
      Assert.True(SetHeroEquipment(bowSolid));

      for (int i = 0; i < hits; i++)
      {
        UseFightItem(hero, enemy, fi);
        Assert.Greater(enemyHealth, enemy.Stats.Health);
        var dd = enemyHealth - enemy.Stats.Health;
        GotoNextHeroTurn();
        Assert.True(GameManager.HeroTurn);
      }
      var diff2 = enemyHealth - enemy.Stats.Health;
      Assert.Greater(diff2, diff1);
    }

    [TestCase("bow", FightItemKind.PlainArrow)]
    [TestCase("crossbow", FightItemKind.PlainBolt)]
    public void DeadEnemyLeavesAmmo(string weapon, FightItemKind fightItemKind)
    {
      var world = CreateWorld();
      var hero = GameManager.Hero;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;
      var fi = new ProjectileFightItem(fightItemKind, hero);
      fi.Count = 20;
      hero.Inventory.Add(fi);
      hero.ActiveFightItem = fi;

      var enemy = PlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.Stats.SetNominal(EntityStatKind.Defense, 10);
      var mana = hero.Stats.Mana;
      var emp = GameManager.CurrentNode.GetClosestEmpty(hero);
      GameManager.CurrentNode.SetTile(enemy, emp.point);

      Assert.True(GameManager.HeroTurn);
      var bow = GenerateEquipment<Weapon>(weapon);
      Assert.True(SetHeroEquipment(bow));
      
      var eqsBefore = world.GetTiles<Loot>();
      
      while (enemy.Alive)
      {
        Assert.True(UseFightItem(hero, enemy, fi));
        GotoNextHeroTurn();
      }

      var newLoot = GetDiff(eqsBefore);
      Assert.Greater(newLoot.Count, 0);
      var ammo = newLoot.Where(i => i is ProjectileFightItem).Cast<ProjectileFightItem>().Where(i=>i.FightItemKind == fightItemKind).ToList();
      Assert.Greater(ammo.Count, 0);
    }
        
    [Test]
    [Repeat(1)]
    public void TorchFightItemTest()
    {
      var world = CreateWorld();
      var hero = GameManager.OuadHero;
      var fi = new ProjectileFightItem(FightItemKind.ThrowingTorch, hero);
      fi.Count = 20;
      var enemy = PlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.Stats.SetNominal(EntityStatKind.Defense, 10);
      var mana = hero.Stats.Mana;

      Assert.True(GameManager.HeroTurn);
      PlaceCloseToHero(hero, enemy);
      var ab = PrepareAbility(hero, AbilityKind.ThrowingTorch, 5);

      bool firing = false;
      int hitCount = 0;
      for (int i = 0; i < 20; i++)
      {
        Assert.True(UseFightItem(hero, enemy, fi));
                
        if(enemyHealth > enemy.Stats.Health)//chance to hit
          hitCount++;
        enemyHealth = enemy.Stats.Health;
        if (enemy.HasLastingEffect(Roguelike.Effects.EffectType.Firing))
        {
          firing = true;
          break;
        }
        GotoNextHeroTurn();
      }
      Assert.Greater(hitCount, 0);
      Assert.True(firing);
    }

    [Test]
    [TestCase(FightItemKind.ExplosiveCocktail)]
    [TestCase(FightItemKind.PoisonCocktail)]
    [Repeat(1)]
    public void TestExplosiveOnHeroSaveLoad(FightItemKind fightItemKind)
    {
      var gi = new OuaDII.Generators.GenerationInfo();
      gi.GenerateInterior = false;
      gi.Counts.WorldEnemiesCount = 10;
      var world = CreateWorld(info: gi);
      
      var enBefore = PlainEnemies.First(i=>i.EntityKind == EntityKind.Undead || i.EntityKind == EntityKind.Human);
      var pfiCount = 3;
      RoguelikeUnitTests.TestBase.MakeEnemyThrowProjectileAtHero(enBefore,  GameManager, fightItemKind, true, ()=> GotoNextHeroTurn(), true, pfiCount);
      var enName = enBefore.Name;
      var enFIC = enBefore.ActiveFightItem.Count;
      Assert.AreEqual(enFIC, 2);
      SaveLoad();
      var en = GameManager.EnemiesManager.GetActiveEnemies().Where(i => i.point == enBefore.point).Single();
      en.AlwaysHit[AttackKind.PhysicalProjectile] = true;
      Assert.AreEqual(enFIC, en.ActiveFightItem.Count);

      RoguelikeUnitTests.TestBase.MakeEnemyThrowProjectileAtHero(en, GameManager, fightItemKind, true, () => GotoNextHeroTurn(), false);
      Assert.AreEqual(en.ActiveFightItem.Count, 1);
    }


  }
}


