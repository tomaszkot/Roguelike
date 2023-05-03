using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Crafting;
using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class CraftingTests : TestBase
  {
    const int firstLevelOfMagicItemSlotsCount = 1;

    [Test]
    public void TestEnchantMagicItem()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      rusty.MakeMagic();
      Assert.False(rusty.IsIdentified);
      Assert.False(rusty.MakeEnchantable());
      Assert.AreEqual(rusty.EnchantSlots, 0);

      rusty.Identify();

      Assert.True(rusty.IsIdentified);
      Assert.True(rusty.MakeEnchantable());
      Assert.AreEqual(rusty.EnchantSlots, 1);
    }

    [Test]
    public void TestEnchantByTinyTrophyArmor()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var eq = GameManager.LootGenerator.GetLootByTileName<Armor>("damaged_buckler");
      var ench = new HunterTrophy(HunterTrophyKind.Fang);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.Defense);

      eq = GameManager.LootGenerator.GetLootByTileName<Armor>("worn_gloves");
      ench = new HunterTrophy(HunterTrophyKind.Fang);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.Defense);

      eq = GameManager.LootGenerator.GetLootByTileName<Armor>("damaged_buckler");
      ench = new HunterTrophy(HunterTrophyKind.Tusk);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.Health);

      eq = GameManager.LootGenerator.GetLootByTileName<Armor>("damaged_buckler");
      ench = new HunterTrophy(HunterTrophyKind.Claw);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.ChanceToMeleeHit);

      var hit = hero.Stats.GetCurrentValue(EntityStatKind.ChanceToMeleeHit);
      Assert.AreEqual(hit, 75);
      Assert.True(SetHeroEquipment(eq));
      hit = hero.Stats.GetCurrentValue(EntityStatKind.ChanceToMeleeHit);
      Assert.AreEqual(hit, 77);
    }

    [Test]
    public void TestEnchantByTinyTrophyJewellery()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var eq = GameManager.LootGenerator.GetLootByTileName<Jewellery>("ring_Defense");
      var ench = new HunterTrophy(HunterTrophyKind.Fang);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.ChanceToEvadeMeleeAttack);

      eq = GameManager.LootGenerator.GetLootByTileName<Jewellery>("ring_Defense");
      ench = new HunterTrophy(HunterTrophyKind.Tusk);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.Strength);

      eq = GameManager.LootGenerator.GetLootByTileName<Jewellery>("ring_Defense");
      ench = new HunterTrophy(HunterTrophyKind.Claw);
      TestEnchantByTinyTrophyForEq(eq, ench, EntityStatKind.Dexterity);

    }


    [Test]
    public void TestEnchantByTinyTrophyWeapon()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      var ench = new HunterTrophy(HunterTrophyKind.Claw);
      TestEnchantByTinyTrophyForEq(rusty, ench, EntityStatKind.ChanceToStrikeBack);

      rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      ench = new HunterTrophy(HunterTrophyKind.Fang);
      TestEnchantByTinyTrophyForEq(rusty, ench, EntityStatKind.ChanceToBulkAttack);

      rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      ench = new HunterTrophy(HunterTrophyKind.Tusk);
      TestEnchantByTinyTrophyForEq(rusty, ench, EntityStatKind.ChanceToCauseBleeding);
    }

    public void TestEnchantByTinyTrophyForEq(Equipment eq, HunterTrophy enchant, EntityStatKind esk)
    {
      Assert.AreEqual(eq.Class, EquipmentClass.Plain);
      eq.MakeEnchantable();

      Assert.AreEqual(eq.EnchantSlots, 1);
      Assert.AreEqual(eq.Enchants.Count, 0);
      Assert.AreEqual(eq.GetMagicStats().Count, 0);

      string err;
      enchant.ApplyTo(eq, out err);
      Assert.AreEqual(eq.Class, EquipmentClass.Plain);
      Assert.AreEqual(eq.Enchants.Count, 1);

      var ms = eq.GetMagicStats();
      Assert.AreEqual(ms.Count, 1);
      Assert.AreEqual(ms[0].Key, esk);
      Assert.AreEqual(ms[0].Value.Factor, (eq is Jewellery) ? 5 : 2);
    }

    [Test]
    public void TestEnchantPlainItem()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(rusty.Class, EquipmentClass.Plain);
      Assert.AreEqual(rusty.GetMagicStats().Count, 0);

      var price = rusty.Price;
      rusty.MakeEnchantable();
      Assert.Greater(rusty.Price, price);
      price = rusty.Price;
      Assert.AreEqual(rusty.Class, EquipmentClass.Plain);
      Assert.AreEqual(rusty.EnchantSlots, 1);
      Assert.AreEqual(rusty.Enchants.Count, 0);

      var gem = new Gem(GemKind.Diamond, hero.Level);
      
      EnchantPlainItem(rusty, gem);
      Assert.Greater(rusty.Price, price);
      Assert.AreEqual(rusty.Enchants.Count, 1);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestUnEnchantPlainItem(bool useSave)
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      rusty.MakeEnchantable();
      Assert.AreEqual(rusty.Class, EquipmentClass.Plain);
      Assert.AreEqual(rusty.EnchantSlots, 1);
      Assert.AreEqual(rusty.Enchants.Count, 0);
      Assert.AreEqual(rusty.GetMagicStats().Count, 0);

      var gem = new Gem(GemKind.Diamond, hero.Level);
      //enchant
      EnchantPlainItem(rusty, gem);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      Assert.AreEqual(rusty.GetMagicStats().Count, 1);

      if (useSave)
      {
        GameManager.Save(false);
        GameManager.Load(hero.Name, false);
        hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
        rusty = hero.Inventory.Items.Where(i => i.Name == rusty.Name).Single() as Weapon;
      }

      var recipe = new Recipe(RecipeKind.UnEnchantEquipment);
      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { rusty, new MagicDust() { Count = 1 } };
      //unenchant
      var res = crafter.Craft(recipe, lootToConvert);
      Assert.True(res.Success);
      Assert.AreEqual(res.LootItems.Count, 2);
      var sword = res.LootItems.Where(i => i is Weapon).Single() as Weapon;
      Assert.True(sword.Enchantable);
      Assert.AreEqual(sword.Enchants.Count, 0);
      Assert.AreEqual(rusty.EnchantSlots, 1);
      var gemBack = res.LootItems.Where(i => i is Enchanter).Single() as Gem;
      Assert.AreEqual(gemBack.Count, 1);
      Assert.AreEqual(rusty.GetMagicStats().Count, 0);
    }

    void EnchantPlainItem(Equipment rusty, Enchanter gem)
    {
      GameManager.Hero.Inventory.Add(gem);
      GameManager.Hero.Inventory.Add(rusty);
      List<Loot> lootToConvert = new List<Loot>();
      lootToConvert.Add(rusty);
      lootToConvert.Add(gem);
      var recipe = new Recipe(RecipeKind.EnchantEquipment);
      GameManager.Craft(lootToConvert, recipe, null);

      Assert.AreEqual(rusty.Class, EquipmentClass.Plain);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      
      Assert.AreEqual(rusty.GetMagicStats().Count, 1);
    }

    [Test]
    [Repeat(1)]
    public void TestEnchantTwoGems()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(rusty.GetMagicStats().Count, 0);

      var price = rusty.Price;
      rusty.MakeEnchantable(2);
      Assert.AreEqual(rusty.EnchantSlots, 2);
      Assert.AreEqual(rusty.Enchants.Count, 0);

      var gem1 = new Gem(GemKind.Diamond, hero.Level);
      var gem2 = new Gem(GemKind.Diamond, hero.Level);
      GameManager.Hero.Inventory.Add(gem1);
      GameManager.Hero.Inventory.Add(gem2);
      Assert.AreEqual(GameManager.Hero.Inventory.Items.Count, 1);
      var gemInInv = GameManager.Hero.Inventory.Items[0] as StackedLoot;
      Assert.AreEqual(gemInInv, gem1);
      Assert.AreEqual(gemInInv.Count, 2);
      var recipe = new Recipe(RecipeKind.EnchantEquipment);

      List<Loot> lootToConvert = new List<Loot>();
      lootToConvert.Add(rusty);
      lootToConvert.Add(gem1);
      var res = GameManager.Craft(lootToConvert, recipe, null);
      Assert.True(res.Success);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      Assert.Greater(rusty.Price, price);
      Assert.AreEqual(rusty.GetMagicStats().Count, 1);
      Assert.AreEqual(gemInInv.Count, 1);//one used

      lootToConvert = new List<Loot>();
      lootToConvert.Add(rusty);
      lootToConvert.Add(gem2);
      res = GameManager.Craft(lootToConvert, recipe, null);
      Assert.True(res.Success);
      Assert.AreEqual(rusty.Enchants.Count, 2);
      Assert.Greater(rusty.Price, price);
      Assert.AreEqual(rusty.GetMagicStats().Count, 1);


    }

    [Test]
    public void TestAmberForWeaponPower()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.UseAttackVariation = false;
      var ad = new AttackDescription(hero, false, AttackKind.Melee);
      var heroBasicAttack = ad.CurrentTotal;
      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      SetHeroEquipment(rusty);

      ad = new AttackDescription(hero, false, AttackKind.Melee);
      var heroAttackPlainRusty = ad.CurrentTotal;
      Assert.Greater(heroAttackPlainRusty, heroBasicAttack);

      //put off weapon
      Assert.True(hero.MoveEquipmentCurrent2Inv(rusty, CurrentEquipmentKind.Weapon));

      var c1 = rusty.GetStats().Values().Where(i => i.TotalValue > 0).ToList();
      rusty.MakeEnchantable();
      Assert.AreEqual(rusty.EnchantSlots, 1);
      Assert.AreEqual(rusty.Enchants.Count, 0);

      var gem = new Gem(GemKind.Amber, hero.Level);
      string err;
      gem.ApplyTo(rusty, out err);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      var ms = rusty.GetMagicStats();
      Assert.AreEqual(ms.Count, 3);//amber gives 3 improvements
      Assert.True(ms.Any(i => i.Key == EntityStatKind.FireAttack));
      var valueFire = ms.Where(i => i.Key == EntityStatKind.FireAttack).Single();
      var tvFire = valueFire.Value.Value.TotalValue;
      Assert.AreEqual(tvFire, 1);

      Assert.True(ms.Any(i => i.Key == EntityStatKind.ColdAttack));
      Assert.True(ms.Any(i => i.Key == EntityStatKind.PoisonAttack));

      //SetHeroEquipment(rusty) was already called - see above the bug is  gem.ApplyTo is not causing stats recalc!
      Assert.True(hero.MoveEquipmentInv2Current(rusty, CurrentEquipmentKind.Weapon));

      ad = new AttackDescription(hero, false, AttackKind.Melee);
      var heroAttackWithEnhancedSword = ad.CurrentTotal;

      Assert.Greater(heroAttackWithEnhancedSword, heroAttackPlainRusty);

      //put off weapon
      Assert.True(hero.MoveEquipmentCurrent2Inv(rusty, CurrentEquipmentKind.Weapon));

      ad = new AttackDescription(hero, false, AttackKind.Melee);
      Assert.AreEqual(ad.CurrentTotal, heroBasicAttack);
    }

    [Test]
    public void TestAmberForWeaponPriceVsUnique()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("sickle");
      var c1 = rusty.GetStats().Values().Where(i => i.TotalValue > 0).ToList();
      rusty.MakeEnchantable(2);
      Assert.AreEqual(rusty.EnchantSlots, 2);
      Assert.AreEqual(rusty.Enchants.Count, 0);

      var uniqSword = GameManager.LootGenerator.GetLootByAsset("shark") as Equipment;
      Assert.AreEqual(uniqSword.Class, EquipmentClass.Unique);
      uniqSword.Identify();
      Assert.True(uniqSword.IsIdentified);

      Assert.Greater(uniqSword.Price, rusty.Price);

      var oldPrice = rusty.Price;
      var gem = new Gem(GemKind.Amber, hero.Level);
      string err;
      gem.ApplyTo(rusty, out err);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      Assert.Greater(rusty.Price, oldPrice);

      Assert.Greater(uniqSword.Price, rusty.Price);
    }

    [Test]
    public void TestAmberForWeapon()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      var c1 = rusty.GetStats().Values().Where(i => i.TotalValue > 0).ToList();
      rusty.MakeEnchantable();
      Assert.AreEqual(rusty.EnchantSlots, 1);
      Assert.AreEqual(rusty.Enchants.Count, 0);

      var gem = new Gem(GemKind.Amber, hero.Level);
      string err;
      gem.ApplyTo(rusty, out err);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      var ms = rusty.GetMagicStats();
      Assert.AreEqual(ms.Count, 3);//amber gives 3
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.FireAttack));
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.ColdAttack));
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.PoisonAttack));

      rusty.IncreaseEnchantSlots();
      gem = new Gem(GemKind.Diamond, hero.Level);
      gem.ApplyTo(rusty, out err);
      Assert.AreEqual(rusty.Enchants.Count, 2);

      rusty.IncreaseEnchantSlots();
      gem = new Gem(GemKind.Ruby, hero.Level);
      gem.ApplyTo(rusty, out err);
      Assert.True(!err.Any());
      Assert.AreEqual(rusty.Enchants.Count, 3);

      rusty.IncreaseEnchantSlots();
      gem = new Gem(GemKind.Emerald, hero.Level);
      gem.ApplyTo(rusty, out err);
      Assert.True(err.Any());
      Assert.AreEqual(rusty.Enchants.Count, 3);

    }

    [Test]
    public void TestAmberForArmor()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var buckler = GameManager.LootGenerator.GetLootByTileName<Armor>("enhanced_buckler");
      var c1 = buckler.GetStats().Values().Where(i => i.TotalValue > 0).ToList();
      buckler.MakeEnchantable();
      Assert.AreEqual(buckler.EnchantSlots, 1);
      Assert.AreEqual(buckler.Enchants.Count, 0);

      var gem = new Gem(GemKind.Amber, hero.Level);
      string err;
      gem.ApplyTo(buckler, out err);
      Assert.AreEqual(buckler.Enchants.Count, 1);
      var ms = buckler.GetMagicStats();
      Assert.AreEqual(ms.Count, 3);//amber gives 3
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.ResistCold));
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.ResistFire));
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.ResistPoison));
    }

    [Test]
    public void TestAmberForJuwellery()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var amulet = GameManager.LootGenerator.GetLootByTileName<Equipment>("amulet_of_attack");
      var c1 = amulet.GetStats().Values().Where(i => i.TotalValue > 0).ToList();
      amulet.MakeEnchantable();
      Assert.AreEqual(amulet.EnchantSlots, 1);
      Assert.AreEqual(amulet.Enchants.Count, 0);

      var gem = new Gem(GemKind.Amber, hero.Level);
      string err;
      gem.ApplyTo(amulet, out err);
      Assert.AreEqual(amulet.Enchants.Count, 1);
      var ms = amulet.GetMagicStats();
      Assert.AreEqual(ms.Count, 3);//amber gives 3
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.Health));
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.Mana));
      Assert.True(ms.Any(i => i.Key == Roguelike.Attributes.EntityStatKind.ChanceToMeleeHit));
    }

    [Test]
    public void TestPendant()
    {
      DoTestPendant();
      //Assert.Greater(result.PrimaryStatValue, primaryVal + 1);
    }

    private Equipment DoTestPendant()
    {
      CreateWorld();

      var cord = GameManager.LootGenerator.GetLootByTileName<Cord>("cord");
      Assert.NotNull(cord);

      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { cord, new MagicDust() };
      var result = crafter.Craft(new Recipe(RecipeKind.Pendant), lootToConvert).FirstOrDefault<Equipment>();

      Assert.IsNotNull(result);
      Assert.True(result is Jewellery);
      var jew = result as Jewellery;
      Assert.True(jew.IsPendant);
      Assert.True(jew.Enchantable);
      Assert.AreEqual(jew.EnchantSlots, 1);
      return result;
    }
        

    [Test]
    [Repeat(1)]
    public void TestPendantEnchant()
    {
      CreateWorld();

      var ped = GameManager.LootGenerator.GetLootByTileName<Jewellery>("pendant");
      Assert.NotNull(ped);
      Assert.AreEqual(ped.GetMagicStats().Count, 0);

      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { ped, new MagicDust(), new HunterTrophy(HunterTrophyKind.Claw) };
      var result = crafter.Craft(new Recipe(RecipeKind.EnchantEquipment), lootToConvert).FirstOrDefault<Equipment>();

      Assert.IsNotNull(result);
      Assert.True(result is Jewellery);
      Assert.AreEqual(result, ped);
      Assert.AreEqual(ped.GetMagicStats().Count, 1);
    }

    [Test]
    public void TestGloveEnchant()
    {
      CreateWorld();

      var eq = GameManager.LootGenerator.GetLootByTileName<Armor>("worn_gloves");
      eq.MakeEnchantable();
      Assert.NotNull(eq);
      Assert.AreEqual(eq.GetMagicStats().Count, 0);

      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { eq, new MagicDust(), new Gem(GemKind.Ruby) };
      var result = crafter.Craft(new Recipe(RecipeKind.EnchantEquipment), lootToConvert);
      var resultAsLoot = result.FirstOrDefault<Equipment>();
      Assert.IsNotNull(resultAsLoot);
      Assert.True(resultAsLoot is Equipment);
      Assert.AreEqual(resultAsLoot, eq);
      Assert.AreEqual(eq.GetMagicStats().Count, 1);
    }


    [Test]
    public void TestPrimaryStat()
    {
      CreateWorld();

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("gladius");
      var broad = GameManager.LootGenerator.GetLootByTileName<Weapon>("broad_sword");

      var primaryVal = broad.PrimaryStatValue;
      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { rusty, broad, new MagicDust() { Count = 2 } };
      var result = crafter.Craft(new Recipe(RecipeKind.TwoEq), lootToConvert).FirstOrDefault<Equipment>();

      Assert.IsNotNull(result);
      Assert.Greater(result.PrimaryStatValue, primaryVal);
      Assert.Greater(result.PrimaryStatValue, primaryVal + 1);
    }

    [Test]
    public void TestOneEq()
    {
      CreateWorld();

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      GameManager.Hero.Crafting.InvItems.Inventory.Add(rusty);
      var md = new MagicDust() { Count = 5 };
      GameManager.Hero.Crafting.InvItems.Inventory.Add(md);
      var lootToConvert = new List<Loot>() { rusty, md };
      var result = GameManager.Craft(lootToConvert, new Recipe(RecipeKind.OneEq), null);

      Assert.IsNotNull(result);
      Assert.AreNotEqual(result, rusty);
      Assert.True(result.LootItems.Single() is Equipment);
      var eq = result.LootItems.Single() as Equipment;
      Assert.AreNotEqual(eq.EquipmentKind, rusty.EquipmentKind);
      Assert.AreEqual(GameManager.Hero.Crafting.InvItems.Inventory.GetStacked<MagicDust>().Single().Count, 4);
    }
    
    [Test]
    [TestCase(PotionKind.Health, SpecialPotionKind.Strength)]
    [TestCase(PotionKind.Mana, SpecialPotionKind.Magic)]
    public void CraftSpecialPotionTests(PotionKind kind, SpecialPotionKind specialPotionKind)
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;
      hero.Inventory.Add(new Potion() { Kind = kind });
      hero.Inventory.Add(new Mushroom() { MushroomKind = MushroomKind.Boletus });
      hero.Inventory.Add(new MagicDust());

      var lootToCraft = hero.Inventory.Items;
      Craft(hero, lootToCraft, RecipeKind.CraftSpecialPotion);
      Assert.AreEqual(hero.Crafting.InvItems.Inventory.ItemsCount, 1);//crafted
      var pot = hero.Crafting.InvItems.Inventory.Items[0] as SpecialPotion;
      Assert.NotNull(pot);
      Assert.AreEqual(pot.SpecialPotionKind, specialPotionKind);
    }

    [Test]
    public void TestOneEqFull()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;
      var rusty = lg.GetLootByTileName<Weapon>("rusty_sword");
      var lootToCraft = new List<Loot> { rusty, new MagicDust() };
      Craft(hero, lootToCraft, RecipeKind.OneEq);
      Assert.AreEqual(hero.Crafting.InvItems.Inventory.ItemsCount, 1);//crafted
      Assert.True(hero.Crafting.InvItems.Inventory.Items[0] is Equipment);
      Assert.False(lootToCraft.Contains(hero.Crafting.InvItems.Inventory.Items[0]));//new one
    }

    [Test]
    public void TestCraftSlotsSecLevelOfMagic()
    {
      CreateWorld();
      SecLevelTest(true);
      SecLevelTest(false);
    }

    private void SecLevelTest(bool secLevelOfMagic)
    {
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;
      var weapon1 = lg.GetLootByTileName<Weapon>("gladius");
      var weapon1OrgPrice = weapon1.Price;
      weapon1.SetClass(EquipmentClass.Magic, weapon1.LevelIndex, null, secLevelOfMagic);
      Assert.Greater(weapon1.Price, weapon1OrgPrice);
      weapon1.Identify();
      weapon1.MakeEnchantable();
      Assert.AreEqual(weapon1.EnchantSlots, secLevelOfMagic ? 1 : firstLevelOfMagicItemSlotsCount);

      var weapon2 = lg.GetLootByTileName<Weapon>("gladius");
      var weapon2OrgPrice = weapon2.Price;
      Assert.AreEqual(weapon1OrgPrice, weapon2OrgPrice);
      Assert.AreEqual(weapon1.LevelIndex, weapon2.LevelIndex);
      weapon2.MakeEnchantable();
      Assert.AreEqual(weapon2.EnchantSlots, 1);

      Assert.Greater(weapon1.GetMagicStats().Count, weapon2.GetMagicStats().Count);
      Assert.Greater(weapon1.Price, weapon2.Price);

      var primaryVal = weapon2.PrimaryStatValue;
      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { weapon1, weapon2, new MagicDust() { Count = 2 } };
      var result = crafter.Craft(new Recipe(RecipeKind.TwoEq), lootToConvert).FirstOrDefault<Equipment>();

      Assert.NotNull(result);
      Assert.AreEqual(result.Name, "Gladius");//bigger price wins
      Assert.True(result.Enchantable);
      Assert.AreEqual(result.Enchants.Count, 0);
      var maxEnch = result.EnchantSlots;
      Assert.AreEqual(maxEnch, secLevelOfMagic ? 1 : firstLevelOfMagicItemSlotsCount);
    }

    

    [Test]
    public void TestCraftSlotsMagicCrafted()
    {
      CreateWorld();

      CraftSlotsSecLevelOfMagicCrafted(false);
      CraftSlotsSecLevelOfMagicCrafted(true);
    }

    GemKind GetMissingGemKind(Equipment eq)
    {
      var ms = eq.GetMagicStats();
      if (!ms.Any(i => i.Key == EntityStatKind.FireAttack))
      {
        return GemKind.Ruby;
      }
      if (!ms.Any(i => i.Key == EntityStatKind.PoisonAttack))
      {
        return GemKind.Emerald;
      }
      if (!ms.Any(i => i.Key == EntityStatKind.ColdAttack))
      {
        return GemKind.Diamond;
      }

      return GemKind.Unset;
    }


    private void CraftSlotsSecLevelOfMagicCrafted(bool secLevelOfMagic)
    {
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var lg = GameManager.LootGenerator;
      var gladius = lg.GetLootByTileName<Weapon>("gladius");

      //make magic
      gladius.SetClass(EquipmentClass.Magic, gladius.LevelIndex, null, secLevelOfMagic);
      gladius.Identify();
      var ms = gladius.GetMagicStats();
      Assert.AreEqual(ms.Count, secLevelOfMagic ? 2 : 1);
      var oldMS = ms;

      //enchant
      gladius.MakeEnchantable();
      Assert.AreEqual(gladius.EnchantSlots, secLevelOfMagic ? 1 : firstLevelOfMagicItemSlotsCount);
      Assert.AreEqual(gladius.Enchants.Count, 0);

      GemKind gk = GetMissingGemKind(gladius);
      ApplyGem(hero, gladius, gk);
      Assert.AreEqual(gladius.Enchants.Count, 1);
      Assert.AreEqual(gladius.IsSecondMagicLevel, secLevelOfMagic);
      ms = gladius.GetMagicStats();
      Assert.AreEqual(ms.Count, secLevelOfMagic ? 3 : 2);

      var warSword = lg.GetLootByTileName<Weapon>("war_sword");
      warSword.MakeEnchantable();//add one slot
      var dam = warSword.Damage;
      Assert.AreEqual(warSword.EnchantSlots, 1);

      var primaryVal = warSword.PrimaryStatValue;
      var crafter = new LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { gladius, warSword, new MagicDust() { Count = 2 } };
      var result = crafter.Craft(new Recipe(RecipeKind.TwoEq), lootToConvert).FirstOrDefault<Equipment>();
      Assert.AreEqual(result.tag1, "war_sword");
      Assert.Greater((result as Weapon).Damage, dam);

      Assert.NotNull(result);
      Assert.True(result.Enchantable);
      var maxEnch = result.EnchantSlots;
      Assert.AreEqual(maxEnch, secLevelOfMagic ? 1 : firstLevelOfMagicItemSlotsCount);//magic of sec level consumes 2 slots
      var resultMS = result.GetMagicStats();
      Assert.AreEqual(resultMS.Count, secLevelOfMagic ? 3 : 2);
      //Assert.AreEqual(result.Enchants.Count, 1);//1 from 1st enchanted item , TODO shall it work ?

      var err = ApplyGem(hero, result, GetMissingGemKind(result));
      Assert.True(!err.Any());
    }

    private static string ApplyGem(OuaDII.Tiles.LivingEntities.Hero hero, Equipment eq, GemKind kind)
    {
      var gem = new Gem(kind, hero.Level);
      string err;
      gem.ApplyTo(eq, out err);
      return err;
    }

    [Test]
    public void EnchantedItemAttackNonPhysicalInfluence()
    {
      CreateWorld();

      Weapon sword = CreateWeaponAndPutOnHero();
      var swordFireAttack = sword.GetStats()[EntityStatKind.FireAttack];
      Assert.AreEqual(swordFireAttack.TotalValue, 0);

      var enemy = AllEnemies.First();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      int effCounter = 0;
      GameManager.EventsManager.EventAppended += (object s, GameEvent ga) =>
      {
        if (ga is LivingEntityAction lea)
        {
          if (lea.Kind == LivingEntityActionKind.GainedDamage)
          {
            if (lea.Info.ToLower().Contains("fire"))
              effCounter++;
          }
        }
      };

      var enemyDamage = enemy.OnMeleeHitBy(hero);
      Assert.AreEqual(effCounter, 0);
      Assert.Greater(enemyDamage, 0);

      swordFireAttack = AddFireAttackFromGem(sword);

      Assert.AreEqual(hero.GetNonPhysicalDamages().Count, 1);//fire
      var enemyDamageWithFire = enemy.OnMeleeHitBy(hero);
      Assert.AreEqual(effCounter, 1);
      Assert.Greater(enemyDamageWithFire, enemyDamage);
    }

    

    //private static Weapon CreateFireDamageWeaponAndPutOnHero(OuaDII.Tiles.LivingEntities.Hero hero, Roguelike.Generators.LootGenerator lg)
    //{

    //  Weapon sword = CreateWeaponAndPutOnHero(hero, lg);

    //  //var gem1 = new Gem(GemKind.Ruby);
    //  //string err = "";
    //  //gem1.ApplyTo(sword, out err);
    //  //swordFireAttack = sword.GetStats()[EntityStatKind.FireAttack];
    //  //Assert.AreEqual(swordFireAttack.TotalValue, 2);
    //  //Assert.AreEqual(heroBareAttack + swordAttack, hero.Stats.Attack);
    //  return sword;
    //}

    private Weapon CreateWeaponAndPutOnHero()
    {
      var hero = GameManager.Hero;
      var lg = GameManager.LootGenerator;
      var sword = lg.GetLootByTileName<Weapon>("rusty_sword");
      sword.StableDamage = true;
      sword.MakeEnchantable();

      var swordAttack = sword.PrimaryStatValue;
      Assert.Greater(swordAttack, 0);
      var heroBareAttack = hero.Stats.MeleeAttack;
      Assert.True(SetHeroEquipment(sword));
      Assert.AreEqual(heroBareAttack + swordAttack, hero.Stats.MeleeAttack);
      return sword;
    }

    [TestCase(RecipeKind.Arrows)]
    [TestCase(RecipeKind.Bolts)]
    public void TestArrowsCrafting(RecipeKind kind)
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var stone = new FightItem(FightItemKind.Stone) { Count = 10 };
      var feather = new Feather() { Count = 10 };
      var hazel = new Hazel() { Count = 20 };
      hero.Crafting.InvItems.Inventory.Add(stone);
      hero.Crafting.InvItems.Inventory.Add(feather);
      hero.Crafting.InvItems.Inventory.Add(hazel);
      var md = new MagicDust() { Count = 10 };
      hero.Crafting.InvItems.Inventory.Add(md);

      var crafter = new OuaDII.Crafting.LootCrafter(GameManager.Container);
      var lootToConvert = new List<Loot>() { stone, feather, hazel, md };
      var result = GameManager.Craft(lootToConvert, new Recipe(kind), null);
      var resultAsLoot = result.FirstOrDefault<FightItem>();
      Assert.IsNotNull(resultAsLoot);
      Assert.True(resultAsLoot.FightItemKind == FightItemKind.PlainArrow || resultAsLoot.FightItemKind == FightItemKind.PlainBolt);
      Assert.AreEqual(resultAsLoot.Count, 10);
      Assert.AreEqual(hazel.Count, 10);
      Assert.AreEqual(feather.Count, 0);
      Assert.AreEqual(stone.Count, 9);//only one stone is used
    }

    [Test]
    public void TestEnchantAmberAndOtherGem()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(rusty.GetMagicStats().Count, 0);

      var price = rusty.Price;
      rusty.MakeEnchantable(2);
            

      var amber = new Gem(GemKind.Amber, hero.Level);
      var ruby = new Gem(GemKind.Ruby, hero.Level);
      GameManager.Hero.Inventory.Add(amber);
      GameManager.Hero.Inventory.Add(ruby);

      var recipe = new Recipe(RecipeKind.EnchantEquipment);
      List<Loot> lootToConvert = new List<Loot>();
      lootToConvert.Add(rusty);
      lootToConvert.Add(amber);
      var res = GameManager.Craft(lootToConvert, recipe, null);
      Assert.True(res.Success);
      Assert.AreEqual(rusty.Enchants.Count, 1);
      Assert.Greater(rusty.Price, price);

      var ms1 = rusty.GetMagicStats();
      Assert.AreEqual(ms1.Count, 3);
      var es1 = ms1.Where(i => i.Key == EntityStatKind.FireAttack).Single().Value;
      Assert.AreEqual(es1.Value.TotalValue, 1);

      lootToConvert = new List<Loot>();
      lootToConvert.Add(rusty);
      lootToConvert.Add(ruby);
      res = GameManager.Craft(lootToConvert, recipe, null);
      Assert.True(res.Success);
      Assert.AreEqual(rusty.Enchants.Count, 2);
      Assert.Greater(rusty.Price, price);
      var ms2 = rusty.GetMagicStats();
      Assert.AreEqual(ms2.Count, 3);
      var es2 = ms2.Where(i => i.Key == EntityStatKind.FireAttack).Single().Value;
      Assert.AreEqual(es2.Value.TotalValue, 2);
    }

    [TestCase(1,1,1, 0)]
    [TestCase(1, 2, 1, 1)]
    [TestCase(2, 3, 2, 1)]
    public void TestNiesiolSoup(int plumCount, int sorrelCount, int resCount, int remainingSorrel)
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var recipe = new Recipe(RecipeKind.NiesiolowskiSoup);
      var lootToConvert = new List<Loot>();
      lootToConvert.Add(new Food() { Kind = FoodKind.Plum , Count = plumCount });
      lootToConvert.Add(new Plant() { Kind = PlantKind.Sorrel, Count = sorrelCount });
      lootToConvert.Add(new MagicDust());
      foreach(var lc in lootToConvert)
        GameManager.Hero.Inventory.Add(lc);

      var res = GameManager.Craft(lootToConvert, recipe, null);
      Assert.True(res.Success);

      Assert.AreEqual(GameManager.Hero.Inventory.GetStacked<Plant>().Count, remainingSorrel);

      Assert.AreEqual(res.LootItems.Count, 1);
      var food = res.LootItems[0] as Food;
      Assert.NotNull(food);
      Assert.AreEqual(food.Kind, FoodKind.NiesiolowskiSoup);
      Assert.AreEqual(food.Count, resCount);
    }
  }
}
