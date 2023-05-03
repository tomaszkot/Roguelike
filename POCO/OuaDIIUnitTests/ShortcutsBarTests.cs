using System.Linq;
using NUnit.Framework;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using Roguelike.Abstract.HotBar;
using Roguelike.Abilities;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class ShortcutsBarTests : TestBase
  {
    [Test]
    public void ScrollTests()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var loot = GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1);
      Assert.NotNull(loot.tag1);
      Assert.True(hero.Inventory.Add(loot));
      Assert.True(hero.ShortcutsBar.SetAt(1, loot));

      Assert.True(hero.Inventory.Contains(loot));
    }

    [Test]
    public void SwitchActiveShortcutsBarTest()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var loot = new Scroll(Roguelike.Spells.SpellKind.FireBall);// //GameManager.LootGenerator.GetRandomLoot(LootKind.Food, 1);
      Assert.NotNull(loot.tag1);
      Assert.True(hero.Inventory.Add(loot));
      Assert.True(hero.ShortcutsBar.SetAt(1, loot));
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), loot);
      Assert.True(hero.Inventory.Contains(loot));
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 1);

      hero.ShortcutsBar.SetCurrentContainerIndex(1);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), null);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, -1);

      hero.ShortcutsBar.SetCurrentContainerIndex(0);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), loot);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 1);
    }

    [Test]
    public void ShortcutsBarSaveLoadNormalLoot()
    {
      CreateWorld();

      var world = GameManager.World;
      Assert.NotNull(world);
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.NotNull(hero);

      var wpn = GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      hero.Inventory.Add(wpn);
      Assert.True(hero.ShortcutsBar.SetAt(1, wpn));
      Assert.NotNull(hero.ShortcutsBar.GetAt(1));

      Assert.AreEqual(hero.Inventory.ItemsCount, 1);
      hero.ShortcutsBar.Disconnect();

      GameManager.Save();
      GameManager.Load(hero.Name);

      var heroLoaded = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreNotEqual(hero, heroLoaded);
      Assert.AreEqual(heroLoaded.Inventory.ItemsCount, 1);
      Assert.NotNull(heroLoaded.ShortcutsBar.GetAt(1));

      heroLoaded.Inventory.Remove(wpn);
      Assert.Null(heroLoaded.ShortcutsBar.GetAt(1));
    }

    public StackedLoot GetRandomStackedLoot()
    {
      var loot = new Food();
      return loot;
    }

    [Test]
    public void ShortcutsBarSaveLoadStackedLootInBars()
    {
      CreateWorld();

      //test StackedInInventory
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var loot = GetRandomStackedLoot();
      loot.Count = 2;
      Assert.True(loot.StackedInInventory);
      hero.Inventory.Add(loot);
      Assert.True(hero.ShortcutsBar.SetAt(1, loot));
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), loot);

      GameManager.Save();
      GameManager.Load(hero.Name);

      var heroLoaded1 = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(heroLoaded1.Inventory.ItemsCount, 1);
      Assert.AreNotEqual(heroLoaded1, hero);
      loot = heroLoaded1.Inventory.Items[0] as StackedLoot;
      Assert.AreEqual(heroLoaded1.ShortcutsBar.GetAt(1), loot);
      var food = loot as Food;
      Assert.AreEqual(food.Count, 2);

      heroLoaded1.Consume(food);
      Assert.AreEqual(heroLoaded1.Inventory.ItemsCount, 1);
      Assert.AreEqual(food.Count, 1);
      heroLoaded1.Consume(food);
      Assert.AreEqual(heroLoaded1.Inventory.ItemsCount, 0);
      Assert.AreEqual(heroLoaded1.ShortcutsBar.GetAt(1), null);
    }

    [Test]
    public void ShortcutsBarGeneral()
    {
      CreateWorld();

      var world = GameManager.World;
      Assert.NotNull(world);
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      
      var wpn = GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      hero.Inventory.Add(wpn);
      Assert.True(hero.ShortcutsBar.SetAt(1, wpn));
      Assert.NotNull(hero.ShortcutsBar.GetAt(1));
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, hero.ActiveShortcutsBarItemDigit);
      hero.Inventory.Remove(wpn);
      Assert.Null(hero.ShortcutsBar.GetAt(1));
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, -1);

      //test StackedInInventory
      var loot = new Food(FoodKind.Plum);//lg.GetLootByTag("plum_mirabelka") as StackedLoot;
      Assert.True(loot.StackedInInventory);
      hero.Inventory.Add(loot);
      Assert.True(hero.ShortcutsBar.HasItem(loot));
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot), 1);
      Assert.AreEqual(hero.Inventory.ItemsCount, 1);//stacked so 1
      
      //add second StackedInInventory
      var loot1 = new Food(FoodKind.Plum);
      hero.Inventory.Add(loot1);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot1), 2);
      Assert.AreEqual(hero.Inventory.ItemsCount, 1);//stacked so 1
      
      hero.Inventory.Remove(loot);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot1), 1);
      hero.Inventory.Remove(loot);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot1), 0);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), null);

      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == Roguelike.Abilities.AbilityKind.PoisonCocktail).First();
      Assert.AreEqual(hero.ShortcutsBar.SetAt(1, ab), true);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 1);
      //SetActiveHeroAbility(Roguelike.Abilities.AbilityKind.PoisonMastering);
      //Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 1);

    }

    void SetActiveHeroAbility(Roguelike.Abilities.AbilityKind kind)
    {
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == kind).First();
      hero.SelectedActiveAbility = ab;
      Assert.NotNull(hero.SelectedActiveAbility);
      Assert.AreEqual(hero.SelectedActiveAbility.Kind, kind);
    }

    [Test]
    public void ShortcutsBarSwap()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.NotNull(hero);

      var wpn = GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      hero.Inventory.Add(wpn);

      var stacked = GetRandomStackedLoot();
      hero.Inventory.Add(stacked);

      foreach (var it in hero.Inventory.Items)
      {
        Assert.True(hero.ShortcutsBar.SetAt(1, it));
        Assert.AreEqual(hero.ShortcutsBar.GetAt(1), it);

        Assert.True(hero.ShortcutsBar.SetAt(2, it));

        Assert.Null(hero.ShortcutsBar.GetAt(1));
        Assert.AreEqual(hero.ShortcutsBar.GetAt(2), it);
      }

    }

    [Test]
    public void ShortcutsBarActiveAbilityPlacedOnlyOnce()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == AbilityKind.Stride).First();
      Assert.AreEqual(hero.ShortcutsBar.SetAt(1, ab), true);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1) , ab);

      Assert.AreEqual(hero.ShortcutsBar.SetAt(2, ab), true);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(2), ab);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), null);
    }

    [Test]
    public void ShortcutsBarUseStride()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var enemy = PrepareForAbilityUsage(AbilityKind.Stride);

      var enemyHealth = enemy.Stats.Health;
      var heroHealth = hero.Stats.Health;
      var enPos = enemy.Position;
      GameManager.ApplyHeroPhysicalAttackPolicy(enemy, true);
      Assert.AreNotEqual(enPos, enemy.Position);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
    }

    //[Test]
    //[TestCase(AbilityKind.StrikeBack)]
    //public void ShortcutsBarUseAutoAppliedAbility(AbilityKind kind)
    //{
    //  CreateWorld();

    //  var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
    //  var enemy = PrepareForAbilityUsage(kind);

    //  var enemyHealth = enemy.Stats.Health;
    //  var heroHealth = hero.Stats.Health;

    //  GotoNextHeroTurn();//enemy will hit hero, hero will strike back
    //  Assert.Greater(heroHealth, hero.Stats.Health);
    //  Assert.Greater(enemyHealth, enemy.Stats.Health);

    //  enemyHealth = enemy.Stats.Health;
    //  heroHealth = hero.Stats.Health;

    //  var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == kind).First();
    //  for (int i = 0; i <= ab.CollDownCounter; i++)
    //  {
    //    GotoNextHeroTurn();//enemy will hit hero, hero will NOT strike back (cooldown)
    //    Assert.Greater(heroHealth, hero.Stats.Health);
    //    Assert.AreEqual(enemyHealth, enemy.Stats.Health);
    //  }

    //  GotoNextHeroTurn();//enemy will hit hero, hero will strike back
    //  Assert.Greater(heroHealth, hero.Stats.Health);
    //  Assert.Greater(enemyHealth, enemy.Stats.Health);

    //}

    private Roguelike.Tiles.LivingEntities.Enemy PrepareForAbilityUsage(AbilityKind kind)
    {
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;

      hero.Abilities.GetAbility(kind).SetFactor(0, 100);
      //done by game
      //hero.Abilities.ActiveItems.Add(new ActiveAbility() { Kind = kind });
      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == kind).First();
      Assert.AreEqual(hero.ShortcutsBar.SetAt(1, ab), true);
      hero.ShortcutsBar.ActiveItemDigit = 1;
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 1);

      var enemy = PlainEnemies.First();
      enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ChanceToMeleeHit, 100);
      PlaceCloseToHero(hero, enemy);
      SetActiveHeroAbility(kind);

      return enemy;
    }

    [Test]
    [Repeat(5)]
    public void ShortcutsBarScrollUseSpell()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      hero.Inventory.Add(fireBallScroll);

      Assert.AreEqual(hero.ActiveShortcutsBarItemDigit, 1);

      Assert.AreEqual(hero.ActiveManaPoweredSpellSource, fireBallScroll);

      var enemy =  PlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;

      var mana = hero.Stats.Mana;
      PlaceCloseToHero(hero, enemy);
      UseActiveShortcutsBarItem(enemy);

      Assert.Greater(enemyHealth, enemy.Stats.Health);

      Assert.Greater(mana, hero.Stats.Mana);
    }

    public bool UseActiveShortcutsBarItem(Roguelike.Tiles.LivingEntities.LivingEntity target)
    {
      if(GameManager.Hero.ActiveManaPoweredSpellSource!=null)
        return GameManager.SpellManager.ApplyAttackPolicy(GameManager.Hero, target, GameManager.Hero.ActiveManaPoweredSpellSource)
                                              == Roguelike.Managers.ApplyAttackPolicyResult.OK;

      return false;
    }

    [Test]
    public void ShortcutsBarGetsLootAutomatically()
    {
      CreateWorld();

      var world = GameManager.World;
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var loot = GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1);
      hero.Inventory.Add(loot);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(1), loot);

      var loot1 = new Potion(PotionKind.Health);
      hero.Inventory.Add(loot1);
      Assert.AreEqual(hero.ShortcutsBar.GetAt(2), loot1);

      var loot2 = new Potion(PotionKind.Health);
      hero.Inventory.Add(loot2);

      var at2 = hero.ShortcutsBar.GetAt(2) as Potion;
      Assert.AreEqual(at2, loot2);
      Assert.AreEqual(at2.Count, 2);

      Assert.AreEqual(hero.ShortcutsBar.GetAt(3), null);
      var fi = new ProjectileFightItem(FightItemKind.ThrowingKnife);
      fi.Count = 10;
      hero.Inventory.Add(fi);

      //var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == Roguelike.Abilities.AbilityKind.ThrowingKnife).First();
      Assert.AreEqual(hero.ShortcutsBar.GetAt(3), fi);
    }

    [Test]
    public void ShortcutsBarFightItemsForAbilitiesStartFromAbility()
    {
      CreateWorld();

      var world = GameManager.World;
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      Assert.AreEqual(hero.ShortcutsBar.GetProjectileDigit(FightItemKind.HunterTrap), -1);

      var trapAb = hero.Abilities.ActiveItems.Where(i => i.Kind == AbilityKind.HunterTrap).Single();
      Assert.True(hero.ShortcutsBar.AssignItem(trapAb, 1));
      Assert.NotNull(hero.ShortcutsBar.GetAt(1));
      Assert.AreNotEqual(hero.ShortcutsBar.GetAt(1), trapAb);
      var pfi = hero.ShortcutsBar.GetAt(1) as ProjectileFightItem;
      Assert.NotNull(pfi);
      Assert.AreEqual(pfi.FightItemKind, FightItemKind.HunterTrap);

      Assert.AreEqual(pfi.Count , 0);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 1);
      Assert.AreEqual(hero.ActiveProjectileFightItem, pfi);
      hero.Inventory.Add(new ProjectileFightItem( FightItemKind.HunterTrap) { Count = 2 });
      pfi = hero.ShortcutsBar.GetAt(1) as ProjectileFightItem;
      Assert.AreEqual(pfi.Count, 2);
    }

    [Test]
    public void ShortcutsBarFightItemsForAbilitiesStartFromFightItem()
    {
      CreateWorld();

      var world = GameManager.World;
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      Assert.AreEqual(hero.ShortcutsBar.GetProjectileDigit(FightItemKind.HunterTrap), -1);

      hero.Inventory.Add(new ProjectileFightItem(FightItemKind.HunterTrap) { Count = 2 });
      var pfi = hero.ShortcutsBar.GetAt(1) as ProjectileFightItem;
      Assert.AreEqual(pfi.Count, 2);

      var trapAb = hero.Abilities.ActiveItems.Where(i => i.Kind == AbilityKind.HunterTrap).Single();
      Assert.True(hero.ShortcutsBar.AssignItem(trapAb, 2));
      Assert.NotNull(hero.ShortcutsBar.GetAt(2));
      Assert.Null(hero.ShortcutsBar.GetAt(1));//now assigned to 2
      pfi = hero.ShortcutsBar.GetAt(2) as ProjectileFightItem;
      Assert.NotNull(pfi);
      Assert.AreEqual(pfi.FightItemKind, FightItemKind.HunterTrap);
      Assert.AreEqual(pfi.Count, 2);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, 2);
      Assert.AreEqual(hero.ActiveProjectileFightItem, pfi);

    }
  }
}
