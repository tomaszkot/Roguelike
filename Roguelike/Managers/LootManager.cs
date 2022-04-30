using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Events;
using Roguelike.Generators;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Managers
{
  public class LootManager
  {
    List<Loot> extraLoot = new List<Loot>();
    public GameManager GameManager { get; set; }
    public LootGenerator LootGenerator => GameManager.LootGenerator;

    public Dictionary<string, string> PowerfulEnemyLoot { get => powerfulEnemyLoot; set => powerfulEnemyLoot = value; }

    public LootManager() { }

    public LootManager(GameManager mgr)
    {
      this.GameManager = mgr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lootSource"></param>
    /// <returns>generated loot</returns>
    public List<Loot> TryAddForLootSource(ILootSource lootSource)//Barrel, Chest from attackPolicy.Victim
    {
      var lootItems = new List<Loot>();
            
      if (lootSource == null)//hit a wall ?
        return lootItems;
      Loot debugLoot = null;
      if (debugLoot != null)
        lootItems.Add(debugLoot);

      if (!lootItems.Any())
      {
        if (lootSource is Enemy)
        {
          var loot = TryAddLootForDeadEnemy(lootSource as Enemy);
          if (loot != null)
            lootItems.Add(loot);
        }
        else
        {
          lootItems = TryAddForNonEnemy(lootSource);
        }
      }
      foreach (var loot in lootItems)
      {
        if (loot is Equipment)
        {
          var eq = loot as Equipment;
          if (eq.Class == EquipmentClass.Plain && !eq.Enchantable)
          {
            if (RandHelper.GetRandomDouble() > GenerationInfo.ChangeToGetEnchantableItem)
              eq.MakeEnchantable();
          }
        }
      }

      return lootItems;
    }

    private List<Loot> TryAddForNonEnemy(ILootSource lootSource)
    {
      //GameManager.Logger.LogInfo("TryAddForNonEnemy lootSource.Level: " + lootSource.Level);
      var lootItems = new List<Loot>();
      var inter = lootSource as Roguelike.Tiles.Interactive.InteractiveTile;
      var lsk = LootSourceKind.Barrel;
      Chest chest = null;

      if (lootSource is Chest)
      {
        chest = (lootSource as Chest);
        if (!chest.Closed)
          return lootItems;
        lsk = chest.LootSourceKind;
      }

      var enFromChest = (lootSource is Chest ch 
        && ch.ChestVisualKind == ChestVisualKind.Grave 
        && RandHelper.GetRandomDouble() < GenerationInfo.ChanceToGenerateEnemyFromGrave);
      if (lootSource is Barrel barrel &&
          !(lootSource is DeadBody) &&
          RandHelper.GetRandomDouble() < GenerationInfo.ChanceToGenerateEnemyFromBarrel ||
          enFromChest
          )
      {
        if (enFromChest)
          GameManager.RegisterDelayedEnemy(lootSource);
        else
          GameManager.AppendEnemy(lootSource);
        if (chest != null)
        {
          if (!chest.Open())
            return lootItems;

          GameManager.SoundManager.PlaySound("grave_open");
          GameManager.AppendAction<InteractiveTileAction>((InteractiveTileAction ac) => { ac.InvolvedTile = chest; ac.InteractiveKind = InteractiveActionKind.ChestOpened; });
        }
        return lootItems;
      }

      var loot = GameManager.TryGetRandomLootByDiceRoll(lsk, lootSource);
      loot = EnsureVariety(lootSource, lsk, loot);

      //bool test = true;
      //if (test)
      //  loot = GameManager.LootGenerator.GetLootByAsset("ice_scepter5") as Equipment;

      if (loot != null)
      {
        loot.Source = lootSource;
        lootItems.Add(loot);
      }
      if (lootSource is Barrel)
      {
        bool repl = GameManager.ReplaceTile(loot, lootSource as Tile);
        GameManager.Assert(repl, "ReplaceTileByLoot " + loot);
        //GameManager.Logger.LogInfo("ReplaceTileByLoot " + loot + " " + repl);
      }
      else if (lootSource is DeadBody db)
      {
        if (!db.IsLooted)
        {
          GameManager.AddLootReward(loot, lootSource, true);//add loot at closest empty
          GameManager.AppendAction<InteractiveTileAction>((InteractiveTileAction ac) => { ac.InvolvedTile = db; ac.InteractiveKind = InteractiveActionKind.DeadBodyLooted; });
          db.SetLooted(true);
          TryAddExtraLoot(lootSource, lootItems, loot, loot is Equipment);
          TryAddExtraLoot(lootSource, lootItems, loot, loot is Equipment);
        }
      }
      else
      {
        if (!chest.Open())
          return lootItems;
        GameManager.AppendAction<InteractiveTileAction>((InteractiveTileAction ac) => { ac.InvolvedTile = chest; ac.InteractiveKind = InteractiveActionKind.ChestOpened; });
        GameManager.AddLootReward(loot, lootSource, true);//add loot at closest empty
        Loot lootEx1 = null;
        if (RandHelper.GetRandomDouble() > .2f)
          lootEx1 = LootGenerator.GetRandomEquipment(GameManager.Hero.Level, GameManager.Hero.GetLootAbility());
        else
          lootEx1 = GetExtraLoot(lootSource, false, loot.LootKind);
        if (lootEx1 != null)
        {
          if (lootItems.Where(i => i.Name == lootEx1.Name).Any())
          {
            //try avoid duplicates
            lootEx1 = LootGenerator.GetRandomEquipment(GameManager.Hero.Level, GameManager.Hero.GetLootAbility());
          }
          if (lootEx1 != null)
          {
            lootItems.Add(lootEx1);
            GameManager.AddLootReward(lootEx1, lootSource, true);
          }
        }

        if (chest.ChestKind == ChestKind.GoldDeluxe || RandHelper.GetRandomDouble() > 0.33f)
        {
          TryAddExtraLoot(lootSource, lootItems, loot, true);
        }
      }

      //lootItems.Where(i => i is Equipment).Cast<Equipment>().ToList();

      return lootItems;
    }
    public Loot EnsureVariety(ILootSource lootSource, LootSourceKind lsk, Loot loot)
    {
      if (loot is Recipe rec)
      {
        //help luck
        if (GameManager.Hero.Crafting.Recipes.Inventory.Items.Where(i => i.Name == rec.Name).Any())
          loot = GameManager.LootManager.LootGenerator.LootFactory.MiscLootFactory.GetRandomRecipe(lootSource.Level);
      }

      return loot;
    }

    private void TryAddExtraLoot(ILootSource lootSource, List<Loot> lootItems, Loot loot, bool nonEquipment)
    {
      var lootEx2 = GetExtraLoot(lootSource, nonEquipment, loot.LootKind);
      if (lootEx2 != null)
      {
        lootItems.Add(lootEx2);
        GameManager.AddLootReward(lootEx2, lootSource, true);
      }
    }

    Loot TryAddLootForDeadEnemy(Enemy enemy)
    {
      //GameManager.Logger.LogInfo("TryAddLootForDeadEnemy "+ enemy);
      Loot loot = null;
      bool addRichConsumableOrOtherReward = false;
      if (enemy.PowerKind == EnemyPowerKind.Champion ||
          enemy.PowerKind == EnemyPowerKind.Boss)
      {
        loot = GenerateLootForPowerfulEnemy(enemy);
        addRichConsumableOrOtherReward = true;
      }
      else
      {
        loot = GameManager.TryGetRandomLootByDiceRoll(LootSourceKind.Enemy, enemy);
        loot = EnsureVariety(enemy, LootSourceKind.Enemy, loot);
      }

      if (loot != null)
        GameManager.AddLootReward(loot, enemy, false);

      if (enemy.DeathLoot != null)
        GameManager.AddLootReward(enemy.DeathLoot, enemy, false);

      var extraLootItems = GetExtraLoot(enemy, loot);
      if (addRichConsumableOrOtherReward)
      {
        var rand = RandHelper.GetRandomDouble();
        var potion = rand > 0.5 ? new Potion(PotionKind.Health) : new Potion(PotionKind.Mana);

        extraLootItems.Add(potion);
        if (RandHelper.GetRandomDouble() > 0.5f)
        {
          //var exLoot = enemy.PowerKind == EnemyPowerKind.Plain ? GameManager.LootGenerator.GetRandomLoot(1, LootKind.Scroll) :
          //                               GenerateLootForPowerfulEnemy(enemy);
          var exLoot = GameManager.LootGenerator.GetRandomLoot(1, LootKind.Scroll);
          if (exLoot is Equipment eq && eq.IsPlain())
          {
            eq.MakeEnchantable(2);
          }
          extraLootItems.Add(exLoot);
        }
      }

      if (enemy.EntityKind == EntityKind.Animal)
      {
        extraLootItems.Add(new Food(FoodKind.Meat));
      }

      foreach (var extraLoot in extraLootItems)
      {
        if (extraLoot != null)
          GameManager.AddLootReward(extraLoot, enemy, false);
      }

      return loot;
    }

    Dictionary<string, string> powerfulEnemyLoot = new Dictionary<string, string>();

    private Loot GenerateLootForPowerfulEnemy(Enemy enemy)
    {
      Loot loot = null;
      if (powerfulEnemyLoot.ContainsKey(enemy.Name))
        loot = GameManager.LootGenerator.GetLootByAsset(powerfulEnemyLoot[enemy.Name]);
      else
        loot = GameManager.GetBestLoot(enemy.PowerKind, enemy.Level, GameManager.GameState.History.Looting);

      return loot;
    }

    private Loot GetExtraLoot(ILootSource victim, bool nonEquipment, LootKind skip)
    {
      if (victim is Chest)
      {
        var chest = victim as Chest;
        if (
          chest.ChestKind == ChestKind.Gold ||
          chest.ChestKind == ChestKind.GoldDeluxe
          )
        {
          if (nonEquipment)
          {
            var loot = LootGenerator.GetRandomLoot(chest.Level, skip);
            int counter = 0;
            while (loot is Equipment && counter++ < 1000)
            {
              loot = LootGenerator.GetRandomLoot(chest.Level, skip);
            }
            return loot;
          }
          else
          {
            var eq = LootGenerator.GetRandomEquipment(GameManager.Hero.Level, GameManager.Hero.GetLootAbility());
            if (eq.IsPlain())
            {
              eq.MakeEnchantable();
              eq.MakeMagic(true);
            }

            return eq;
          }
        }
      }

      return LootGenerator.GetRandomLoot(victim.Level, skip);
    }

    protected virtual List<Loot> GetExtraLoot(ILootSource lootSource, Loot primaryLoot)
    {
      extraLoot.Clear();

      if (GenerationInfo.DebugInfo.EachEnemyGivesPotion)
      {
        var potion = LootGenerator.GetRandomLoot(LootKind.Potion, lootSource.Level);
        extraLoot.Add(potion);
      }
      if (GenerationInfo.DebugInfo.EachEnemyGivesJewellery)
      {
        var loot = LootGenerator.GetRandomRing();
        extraLoot.Add(loot);
      }
      if (primaryLoot is Gold)
      {
        var loot = LootGenerator.TryGetRandomLootByDiceRoll(LootSourceKind.Enemy, lootSource.Level, GameManager.Hero.GetLootAbility());
        if (!(loot is Gold))
          extraLoot.Add(loot);
      }

      if (lootSource is Enemy en)
      {
        var fis = new FightItemKind[] { FightItemKind .PlainArrow, FightItemKind.PlainBolt, FightItemKind.ThrowingKnife};
        foreach (var fi in fis)
        {
          var co = en.GetFightItemKindHitCounter(fi);
          if (co > 0)
          {
            if (co > 1)
              co--;//one is lost in action
            extraLoot.Add(new ProjectileFightItem(fi) { Count = co }); 
          }
        }
      }

      return extraLoot;
    }
  }
}
