using NUnit.Framework;
//using OuaDII.Tiles.LivingEntities;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.LootFactories;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class EquipmentTests : TestBase
  {
    [Test]
    //[TestCase(1)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(5)]
    public void TestMagicWpnsTags(int heroLevel)
    {
      var game = CreateWorld();
      GameManager.Hero.SetLevel(heroLevel);
      var wpn = GameManager.LootGenerator.GetLootByAsset("fire_wand" + heroLevel) as Weapon;
      Assert.AreEqual(wpn.tag1, "fire_wand" + 1);//TODO heroLevel);
      Assert.AreEqual(wpn.tag2, "fire_wand"+ 1);
      Assert.AreEqual(wpn.LevelIndex, heroLevel);

      for (int i = 0; i < 100; i++)
      {
        var wpnRand = GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, heroLevel) as Weapon;
        if (wpnRand != null && wpnRand.IsMagician)
        {
          Assert.AreEqual(wpn.LevelIndex, heroLevel);
          var desc = wpn.SpellSource.GetExtraStatDescriptionFormatted(GameManager.Hero);
          Assert.True(desc.Contains("Level"));
        }
      }
    }

    [Test]
    public void HoundEquipmentTest()
    {
      CreateWorld();
      var lg = GameManager.LootGenerator;
      //var ironDamage = 0;
      var arm1 = lg.GetLootByTileName<Armor>("hound_tunic");
      Assert.AreEqual(arm1.Material, EquipmentMaterial.Unset);

      var arm2 = lg.GetLootByTileName<Armor>("bronze_hound_armor");
      Assert.AreEqual(arm2.Material, EquipmentMaterial.Bronze);
      Assert.AreEqual(arm2.Name.ToLower(), "bronze hound armor");
      Assert.Greater(arm2.Defence, arm1.Defence);
    }


      [Test]
    public void EquipmentMaterialTest()
    {
      CreateWorld();
      var lg = GameManager.LootGenerator;
      var ironDamage = 0;
      {
        var sword = lg.GetLootByTileName<Weapon>("rusty_sword");
        Assert.AreEqual(sword.Damage, MaterialProps.BronzeSwordBaseDamage);
        Assert.AreEqual(sword.Material, EquipmentMaterial.Bronze);

        sword.SetMaterial(Roguelike.Tiles.EquipmentMaterial.Iron);
        Assert.Greater(sword.Damage, MaterialProps.BronzeSwordBaseDamage);
        Assert.LessOrEqual(sword.Damage, MaterialProps.BronzeSwordBaseDamage * 2);
        ironDamage = sword.Damage;
      }
      {
        var sword1 = lg.GetLootByTileName<Weapon>("rusty_sword");
        Assert.AreEqual(sword1.Damage, MaterialProps.BronzeSwordBaseDamage);

        sword1.SetMaterial(Roguelike.Tiles.EquipmentMaterial.Steel);
        Assert.Greater(sword1.Damage, ironDamage);
        Assert.LessOrEqual(sword1.Damage, ironDamage*2);
      }

      {
        var axe = lg.GetLootByTileName<Weapon>("sickle");
        Assert.AreEqual(axe.Damage, MaterialProps.BronzeAxeBaseDamage);

        axe.SetMaterial(Roguelike.Tiles.EquipmentMaterial.Iron);
        Assert.Greater(axe.Damage, MaterialProps.BronzeAxeBaseDamage);
        Assert.LessOrEqual(axe.Damage, MaterialProps.BronzeAxeBaseDamage * 2);
        ironDamage = axe.Damage;
      }

      {
        var sword1 = lg.GetLootByTileName<Weapon>("dagger");
        Assert.AreEqual(sword1.Damage, MaterialProps.BronzeDaggerBaseDamage);

        sword1.SetMaterial(Roguelike.Tiles.EquipmentMaterial.Steel);
        Assert.Greater(sword1.Damage, MaterialProps.BronzeDaggerBaseDamage);
        Assert.LessOrEqual(sword1.Damage, MaterialProps.BronzeDaggerBaseDamage * 3);
      }

      var shield = lg.GetLootByTileName<Armor>("damaged_buckler");
      Assert.AreEqual(shield.Material, EquipmentMaterial.Unset);

    }

    [Test]
    public void TestNames()
    {
      CreateWorld();
      var lg = GameManager.LootGenerator;
      var tusk = lg.GetLootByTileName<Weapon>("tusk");
      Assert.AreEqual(tusk.Material, EquipmentMaterial.Unset);
      Assert.AreEqual(tusk.Name, "Tusk");
      Assert.AreEqual(tusk.DisplayedName, "Tusk");

      string name = "";
      while (true)
      {
        var next = lg.GetRandomLoot(1) as Equipment;
        if (next!=null && next.EquipmentKind == EquipmentKind.Amulet)
        {
          Assert.True(next.Name.Contains("Amulet"));
          Assert.True(next.DisplayedName.Contains("Amulet"));
          GameManager.Hero.Inventory.Add(next);
          name = next.Name;
          break;
        }
      }

      GameManager.Save();
      GameManager.Load(GameManager.Hero.Name);
      Assert.AreEqual(GameManager.Hero.Inventory.Items[0].Name, name);

      name = "";
      while (true)
      {
        var next = lg.GetRandomLoot(1) as Equipment;
        if (next != null && next is Weapon wpn && wpn.IsMagician)
        {
          Assert.False(next.Name.Contains("1"));
          Assert.False(next.DisplayedName.Contains("1"));
          name = next.Name;
          break;
        }
      }

      var book = new Book(SpellKind.Portal);
      Assert.True(book.Name.Contains("Portal"));
      Assert.True(book.DisplayedName.Contains("Portal"));

      Assert.False(book.Name.Contains("Unset"));
      Assert.False(book.DisplayedName.Contains("Unset"));

      book = lg.GetLootByAsset("portal_book") as Book;
      Assert.True(book.DisplayedName.Contains("Portal"));
      Assert.False(book.DisplayedName.Contains("Unset"));
    }

    [Test]
    public void MagicItemsInfluenceTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var sword = lg.GetLootByTileName<Weapon>("rusty_sword");
      sword.MakeMagic(EntityStatKind.MeleeAttack, 2);
      Assert.False(sword.IsIdentified);
      var heroBareAttack = hero.Stats.MeleeAttack;

      SetHeroEquipment(sword);
      Assert.AreEqual(heroBareAttack, hero.Stats.MeleeAttack);//wpn not identified

      hero.Inventory.Add(new Scroll(SpellKind.Identify));
      Assert.True(hero.Identify(sword, GameManager.SpellManager));
      Assert.Greater(hero.Stats.MeleeAttack, heroBareAttack);

      var wpnAttack = sword.GetStats().GetTotalValue(EntityStatKind.MeleeAttack);
      Assert.AreEqual(heroBareAttack + wpnAttack, hero.Stats.MeleeAttack);//wpn identified
    }

    [Test]
    public void MagicItemsPriceTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      for (int i = 0; i < 5; i++)
      {
        var sword1 = lg.GetLootByTileName<Weapon>("rusty_sword");
        var sword2 = lg.GetLootByTileName<Weapon>("gladius");
        Assert.Greater(sword2.Price, sword1.Price);

        var sword11 = lg.GetLootByTileName<Weapon>("rusty_sword");
        Assert.Greater(sword11.Price, 0);
        sword11.MakeMagic(EntityStatKind.MeleeAttack, 2);
        Assert.Greater(sword11.Price, sword1.Price);//shall be bit greater
        var price = sword11.Price;
        sword11.Identify();
        Assert.Greater(sword11.Price, price);

        var sword111 = lg.GetLootByTileName<Weapon>("rusty_sword");
        sword111.MakeMagic(true);
        sword111.Identify();
        var err = "";

        Assert.Greater(sword111.Price, sword11.Price, err);
      }
    }

    [Test]
    public void ArmorRequirementsTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var ragged_tunic = lg.GetLootByTileName<Armor>("ragged_tunic");
      Assert.AreEqual(ragged_tunic.RequiredStats.Strength, hero.Stats.Strength);
      var cape = lg.GetLootByTileName<Armor>("cape");
      Assert.Greater(cape.Defence, ragged_tunic.Defence);
      Assert.Greater(cape.RequiredStats.Strength, ragged_tunic.RequiredStats.Strength);

      var padded_armor = lg.GetLootByTileName<Armor>("padded_armor");
      Assert.Greater(padded_armor.Defence, cape.Defence);
      Assert.Greater(padded_armor.RequiredStats.Strength, cape.RequiredStats.Strength);

      var solid_leather_armor = lg.GetLootByTileName<Armor>("solid_leather_armor");
      Assert.Greater(solid_leather_armor.Defence, padded_armor.Defence);
      Assert.Greater(solid_leather_armor.RequiredStats.Strength, padded_armor.RequiredStats.Strength);

      var full_plate_armor = lg.GetLootByTileName<Armor>("full_plate_armor");
      Assert.Greater(full_plate_armor.Defence, padded_armor.Defence);
      Assert.Greater(full_plate_armor.RequiredStats.Strength, padded_armor.RequiredStats.Strength);
      //leather_armor
      //chained_armor
      //plate_armor
    }

    [Test]
    public void ShieldRequirementsTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var damaged_buckler = lg.GetLootByTileName<Armor>("damaged_buckler");
      Assert.AreEqual(damaged_buckler.RequiredStats.Strength, hero.Stats.Strength);
      var buckler = lg.GetLootByTileName<Armor>("edged_buckler");
      Assert.Greater(buckler.Defence, damaged_buckler.Defence);
      Assert.Greater(buckler.RequiredStats.Strength, damaged_buckler.RequiredStats.Strength);

      var long_shield = lg.GetLootByTileName<Armor>("long_shield");
      Assert.Greater(long_shield.Defence, buckler.Defence);
      Assert.Greater(long_shield.RequiredStats.Strength, buckler.RequiredStats.Strength);

      var kings_bucket = lg.GetLootByTileName<Armor>("king's_bucket");
      Assert.Greater(kings_bucket.Defence, long_shield.Defence);
      Assert.Greater(kings_bucket.RequiredStats.Strength, long_shield.RequiredStats.Strength);
    }

    [Test]
    public void GlovesRequirementsTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var worn_gloves = lg.GetLootByTileName<Armor>("worn_gloves");
      Assert.AreEqual(worn_gloves.RequiredStats.Strength, hero.Stats.Strength);
      var leather_gloves = lg.GetLootByTileName<Armor>("enhanced_leather_gloves");
      Assert.Greater(leather_gloves.Defence, worn_gloves.Defence);
      Assert.Greater(leather_gloves.RequiredStats.Strength, worn_gloves.RequiredStats.Strength);

      var gauntlets = lg.GetLootByTileName<Armor>("gauntlets");
      Assert.Greater(gauntlets.Defence, leather_gloves.Defence);
      Assert.Greater(gauntlets.RequiredStats.Strength, leather_gloves.RequiredStats.Strength);
    }

    [Test]
    public void HelmetsRequirementsTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var cap = lg.GetLootByTileName<Armor>("cap");
      Assert.AreEqual(cap.RequiredStats.Strength, hero.Stats.Strength);
      var helm = lg.GetLootByTileName<Armor>("helm");
      Assert.Greater(helm.Defence, cap.Defence);
      Assert.Greater(helm.RequiredStats.Strength, cap.RequiredStats.Strength);

      var holly_helm = lg.GetLootByTileName<Armor>("holly_helm");
      Assert.Greater(holly_helm.Defence, helm.Defence);
      Assert.Greater(holly_helm.RequiredStats.Strength, helm.RequiredStats.Strength);
    }


    [Test]
    public void UniqueItemsPriceTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var swordNormal = lg.GetLootByTileName<Weapon>("rusty_sword");
      var uniqSword = lg.GetLootByAsset("swiatowid_sword") as Equipment;
      Assert.AreEqual(uniqSword.Class, EquipmentClass.Unique);
      Assert.AreEqual(uniqSword.IsIdentified, false);
      Assert.False(uniqSword.GetMagicStats().Any());
      Assert.Greater(uniqSword.Price, swordNormal.Price);
      var price = uniqSword.Price;
      uniqSword.Identify();
      Assert.AreEqual(uniqSword.Class, EquipmentClass.Unique);
      Assert.True(uniqSword.GetMagicStats().Any());
      Assert.Greater(uniqSword.Price, price);
    }

    [Test]
    public void UniqueItemPriceVsMagicItemTest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;

      var swordNormal = lg.GetLootByTileName<Weapon>("gladius");
      swordNormal.MakeMagic(EntityStatKind.MeleeAttack, 2);
      swordNormal.Identify();
      Assert.True(swordNormal.IsIdentified);

      var uniqSword = lg.GetLootByAsset("shark") as Equipment;
      Assert.AreEqual(uniqSword.Class, EquipmentClass.Unique);
      uniqSword.Identify();
      Assert.True(uniqSword.IsIdentified);

      Assert.Greater(uniqSword.Price, swordNormal.Price);

    }

    [Test]
    public void TorchFightItemCountTest()
    {
      var world = CreateWorld();
      var hero = GameManager.OuadHero;

      //prevent from auto put on torch by put on shield
      var lg = GameManager.LootGenerator;
      var shield = lg.GetLootByTileName<Armor>("damaged_buckler");
      PlaceCloseToHero(hero, shield);
      shield.Price = 20;
      CollectLoot(shield);
      Assert.AreEqual(hero.GetActiveEquipment(CurrentEquipmentKind.Shield), shield);

      var fi = CollectTorches(hero, 10);
      Assert.True(hero.Inventory.Contains(fi));
      Assert.True(hero.ShortcutsBar.HasItem(fi));
      Assert.AreEqual(hero.ShortcutsBar.ItemCount(fi), 10);
      Assert.True(hero.MoveEquipmentInv2Current(fi, CurrentEquipmentKind.Shield));
      //hero.SyncShortcutsBarStackedLoot();
      Assert.AreEqual(hero.GetActiveEquipment(CurrentEquipmentKind.Shield), fi);
      Assert.False(hero.Inventory.Contains(fi));

      Assert.True(hero.ShortcutsBar.HasItem(fi));
      var digit = hero.ShortcutsBar.GetProjectileDigit(FightItemKind.ThrowingTorch);
      var fiAtBar = hero.ShortcutsBar.GetAt(digit) as ProjectileFightItem;
      Assert.AreEqual(fiAtBar.Count, 10);

      CollectTorches(hero, 1);
      hero.SyncShortcutsBarStackedLoot();
      
      fiAtBar = hero.ShortcutsBar.GetAt(digit) as ProjectileFightItem;
      Assert.AreEqual(fiAtBar.Count, 11);

      CollectTorches(hero, 1);
      hero.SyncShortcutsBarStackedLoot();
      fiAtBar = hero.ShortcutsBar.GetAt(digit) as ProjectileFightItem;
      Assert.AreEqual(fiAtBar.Count, 12);

    }

    private ProjectileFightItem CollectTorches(OuaDII.Tiles.LivingEntities.Hero hero, int count)
    {
      var fi = new ProjectileFightItem(FightItemKind.ThrowingTorch, hero);
      fi.Count = count;
      PlaceCloseToHero(hero, fi);
      GotoNextHeroTurn();
      CollectLoot(fi);
      return fi;
    }
  }
}