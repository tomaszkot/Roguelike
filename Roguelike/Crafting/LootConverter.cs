using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Roguelike.Crafting
{
  public interface ILootCrafter
  {
    Tuple<Loot, string> Craft(Recipe recipe, List<Loot> lootToCraft);
  }

  public abstract class LootCrafterBase : ILootCrafter
  {
    public abstract Tuple<Loot, string> Craft(Recipe recipe, List<Loot> lootToCraft);

    protected Tuple<Loot, string> ReturnCraftedLoot(Loot loot)
    {
      return new Tuple<Loot, string>(loot, string.Empty);
    }

    protected Tuple<Loot, string> ReturnCraftingError(string error)
    {
      return new Tuple<Loot, string>(null, error);
    }

    public int LastCraftStackedCount { get; set; }
  }

  public class LootCrafter : LootCrafterBase
  {
    Container container;
    EquipmentFactory equipmentFactory;
    const string InvalidIngredients = "Invalid ingredients";

    public LootCrafter(Container container)
    {
      this.container = container;
      equipmentFactory = container.GetInstance<EquipmentFactory>();
    }

    Jewellery createJewellery(EquipmentKind kind, int minDropDungeonLevel)
    {
      var juwell = new Jewellery();
      juwell.EquipmentKind = kind;
      juwell.MinDropDungeonLevel = minDropDungeonLevel;
      juwell.Price = 10;
      return juwell;
    }

    public override Tuple<Loot, string> Craft(Recipe recipe, List<Loot> lootToConvert)
    {
      if (recipe == null)
        return ReturnCraftingError("Recipe not set");
      LastCraftStackedCount = 0;
      if (lootToConvert.Count(i => i is MagicDust) != recipe.MagicDustRequired)
      {
        return ReturnCraftingError("Invalid amount of Magic Dust");
      }
      lootToConvert = lootToConvert.Where(i => !(i is MagicDust)).ToList();
      var eqs = lootToConvert.Where(i => i is Equipment).Cast<Equipment>().ToList();

      if (eqs.Any(i => i.Class == EquipmentClass.Unique))
        return ReturnCraftingError("Unique items can not crafted");

      if (lootToConvert.Any())
      {
        var sulfCount = lootToConvert.Where(i => i is Sulfur).Count();
        var hoochCount = lootToConvert.Where(i => i is Hooch).Count();

        if (recipe.Kind == RecipeKind.Custom)
        {
          return HandleCustomRecipe(lootToConvert);
        }

        if (recipe.Kind == RecipeKind.Pendant)
        {
          var cords = lootToConvert.Where(i => i is Cord).Count();
          if (cords != 0)
            return ReturnCraftingError(InvalidIngredients);

          var tinyTrophyCount = lootToConvert.Where(i => i is TinyTrophy).Count();
          if(tinyTrophyCount == 0 || tinyTrophyCount > 3)
            return ReturnCraftingError("Amount of ornaments must be between 1-3");

          var amulet = createJewellery(EquipmentKind.Amulet, 1);
          string err;
          amulet.Enchant(EntityStatKind.Attack, 5, GemKind.Diamond, out err);//TODO
          return ReturnCraftedLoot(amulet);
        }

        if ((recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.ExplosiveCocktail) &&
          sulfCount > 0 && hoochCount > 0)
        {

          if (sulfCount != hoochCount)
            return ReturnCraftingError("Number of ingradients must be the same (not counting Magic Dust)");
          LastCraftStackedCount = sulfCount;//TODO
          return ReturnCraftedLoot(new ExplosiveCocktail());
        }

        //if ((recipe.Kind == RecipeKind.Custom) &&
        // lootToConvert.Count == 2 && lootToConvert.Any(i => i is SheepRemains) && lootToConvert.Any(i => i is Sulfur))
        //{
        //  return ReturnCraftedLoot(new SheepRemainsStuffed());
        //}

        var allGems = lootToConvert.All(i => i is Gem);
        if ((recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.ThreeGems) && allGems && lootToConvert.Count == 3)
        {
          //TODO
          //return HandleAllGems(lootToConvert);
        }
        var allHp = lootToConvert.All(i => i.IsPotion(PotionKind.Health));
        var allMp = lootToConvert.All(i => i.IsPotion(PotionKind.Mana));
        if ((recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.Potion) && (allHp || allMp))
        {
          LastCraftStackedCount = lootToConvert.Count;//TODO
          if (lootToConvert[0].IsPotion(PotionKind.Mana))
            return ReturnCraftedLoot(new Potion(PotionKind.Health));
          else
            return ReturnCraftedLoot(new Potion(PotionKind.Mana));
        }

        if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.Toadstool2Potions)
        {
          var allToadstool = lootToConvert.All(i => i.IsToadstool());
          if (allToadstool && lootToConvert.Count == 1)
          {
            var toadstool = lootToConvert[0].AsToadstool();
            if (toadstool != null)
            {
              LastCraftStackedCount = 3;
              Potion potion = null;
              if (toadstool.MushroomKind == MushroomKind.BlueToadstool)
                potion = new Potion(PotionKind.Mana);
              else
                potion = new Potion(PotionKind.Health);
              return ReturnCraftedLoot(potion);
            }
          }
        }
      }

      if (lootToConvert.Count == 1)
      {
        var srcEq = lootToConvert[0] as Equipment;
        if (srcEq != null && (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.OneEq))
        {
          return CraftOneEq(srcEq);
        }

        else if (lootToConvert[0] is Gem && (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.Gem))
        {
          var srcGem = lootToConvert[0] as Gem;
          var destKind = RandHelper.GetRandomEnumValue<GemKind>(new GemKind[] { srcGem.GemKindValue });
          var destGem = new Gem(destKind);
          destGem.GemSizeValue = srcGem.GemSizeValue;
          destGem.SetProps();
          return ReturnCraftedLoot(destGem);
        }
      }
      else if (lootToConvert.Count == 2 && eqs.Count == 2 && (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.TwoEq))
      {
        return CraftTwoEq(eqs);
      }
      else if (recipe.Kind == RecipeKind.Custom && eqs.Count == 1)
      {
        if (eqs[0].EquipmentKind == EquipmentKind.Weapon)
        {
          var sps = lootToConvert.Where(i => i is SpecialPotion).Cast<SpecialPotion>().ToList();
          if (sps.Count == lootToConvert.Count - 1 && sps.Count <= 2)
          {
            return ReturnStealingEq(eqs, sps);
          }
        }
      }
      else if (recipe.Kind == RecipeKind.Custom && lootToConvert.Count == 2)
      {
        var toadstools = lootToConvert.Where(i =>  i.IsToadstool()).ToList();
        var potions = lootToConvert.Where(i => i is Potion).ToList();
        if (toadstools.Count == 1 && potions.Count == 1)
        {
          var tk = (toadstools[0].AsToadstool()).MushroomKind;
          var pk = (potions[0] as Potion).Kind;

          if (tk == MushroomKind.RedToadstool && pk == PotionKind.Mana)
            return ReturnCraftedLoot(new Mushroom(MushroomKind.BlueToadstool));
          else if (tk == MushroomKind.BlueToadstool && pk == PotionKind.Health)
            return ReturnCraftedLoot(new Mushroom(MushroomKind.RedToadstool));
        }
      }

      return ReturnCraftingError(InvalidIngredients);
    }

    private Tuple<Loot, string> HandleCustomRecipe(List<Loot> lootToConvert)
    {
      if (lootToConvert.Count == 2)
      {
        if (
          (lootToConvert[0].IsToadstool() && lootToConvert[1].IsPotion(PotionKind.Health)) ||
          (lootToConvert[1].IsToadstool() && lootToConvert[0].IsPotion(PotionKind.Health)) ||
          (lootToConvert[0].IsToadstool() && lootToConvert[1].IsPotion(PotionKind.Mana)) ||
          (lootToConvert[1].IsToadstool() && lootToConvert[0].IsPotion(PotionKind.Mana))
          )
        {
          var srcLoot = lootToConvert[0].IsToadstool() ? lootToConvert[0] : lootToConvert[1];
          var destLoot = lootToConvert[0].IsToadstool() ? lootToConvert[1] : lootToConvert[0];
          var crafted = srcLoot.CreateCrafted(destLoot);
          return ReturnCraftedLoot(crafted);
        }
        //////////////////////////////////////////////////////////////////////////////
        if (lootToConvert[0] is Gem && lootToConvert[1] is Equipment ||
           lootToConvert[1] is Gem && lootToConvert[0] is Equipment
          )
        {
          var gem = lootToConvert[0] is Gem ? lootToConvert[0] as Gem : lootToConvert[1] as Gem;
          var eq = lootToConvert[0] is Equipment ? lootToConvert[0] as Equipment : lootToConvert[1] as Equipment;
          var err = "";
          //TODO
          if (gem.ApplyTo(eq, out err))
          {
            return ReturnCraftedLoot(eq);
          }
          else
            return ReturnCraftingError(err);
        }
        //////////////////////////////////////////////////////////////////////////////
        if (lootToConvert[0].IsCraftableWith(lootToConvert[1]) ||
            lootToConvert[1].IsCraftableWith(lootToConvert[0])
          )
        {
          if (lootToConvert[0].IsCraftableWith(lootToConvert[1]))
            return ReturnCraftedLoot(lootToConvert[0].CreateCrafted(lootToConvert[1]));
          else
            return ReturnCraftedLoot(lootToConvert[1].CreateCrafted(lootToConvert[0]));
        }

        return ReturnCraftingError("Improper ingredients");
      }
      else
        return ReturnCraftingError("Invalid amount of Magic Dust");
    }


    private Tuple<Loot, string> ReturnStealingEq(List<Equipment> eqs, List<SpecialPotion> sps)
    {
      if (eqs[0].ExtendedInfo.Stats.GetFactor(EntityStatKind.LifeStealing) > 10 ||
                    eqs[0].ExtendedInfo.Stats.GetFactor(EntityStatKind.ManaStealing) > 10)
      {
        return ReturnCraftingError("Max level of one of statistics reached");
      }

      float ls = 0;
      float ms = 0;
      foreach (var sp in sps)
      {
        var esk = sp.Kind == Tiles.Looting.SpecialPotionKind.Strength ? EntityStatKind.LifeStealing : EntityStatKind.ManaStealing;
        if (esk == EntityStatKind.LifeStealing)
          ls += sp.BigPotion ? 5 : 2;
        else
          ms += sp.BigPotion ? 5 : 2;

      }
      if (ls > 0)
      {
        eqs[0].AddMagicStat(EntityStatKind.LifeStealing);
        eqs[0].ExtendedInfo.Stats[EntityStatKind.LifeStealing].Factor = ls;
      }
      if (ms > 0)
      {
        eqs[0].AddMagicStat(EntityStatKind.ManaStealing);
        eqs[0].ExtendedInfo.Stats[EntityStatKind.ManaStealing].Factor = ms;
      }

      return ReturnCraftedLoot(eqs[0]);
    }

    private Tuple<Loot, string> CraftTwoEq(List<Equipment> eqs)
    {

      var eq1 = eqs[0];
      var eq2 = eqs[1];
      if (eq1.EquipmentKind != eq2.EquipmentKind)
        return ReturnCraftingError("Equipment for crafting must be of the same type");

      if (eq1.WasCraftedBy(RecipeKind.TwoEq) || eq2.WasCraftedBy(RecipeKind.TwoEq))
        return ReturnCraftingError("Can not craft equipment which was already crafted in pairs"); ;
      var destEq = eq1.Price > eq2.Price ? eq1 : eq2;

      var srcEq = destEq == eq1 ? eq2 : eq1;
      var srcHadEmptyEnch = srcEq.Enchantable && srcEq.Enchants.Count < srcEq.GetMaxEnchants();
      var destHadEmptyEnch = destEq.Enchantable && destEq.Enchants.Count < destEq.GetMaxEnchants();

      //destEq = destEq.Clone() as Equipment; //TODo why clone  ?
      float priceInc = 0;
      var enhPr = GetEnhStatValue(destEq.PrimaryStatValue, destEq.Price, srcEq.Price);
      destEq.PrimaryStatValue += enhPr;
      priceInc += destEq.GetPriceForFactor(destEq.PrimaryStatKind, enhPr);

      var destStats = destEq.GetMagicStats();
      var srcStats = srcEq.GetMagicStats();
      var srcDiffStats1 = srcStats.Where(i => !destStats.Any(j => j.Key == i.Key)).ToList();

      if (destStats.Count < 3 && srcDiffStats1.Any())
      {
        var countToAdd = 3 - destStats.Count;
        foreach (var statToAdd in srcDiffStats1)
        {
          if (destEq.Class == EquipmentClass.Plain)
            destEq.Class = EquipmentClass.Magic;
          destEq.SetMagicStat(statToAdd.Key, statToAdd.Value);
          countToAdd--;
          priceInc += destEq.GetPriceForFactor(statToAdd.Key, (int)statToAdd.Value.Factor);
          if (countToAdd == 0)
            break;
        }
      }
      else
      {
        foreach (var destStat in destStats)
        {
          var enh = GetEnhStatValue(destStat.Value.Factor, destEq.Price, srcEq.Price);
          destStat.Value.Factor += enh;
          priceInc += destEq.GetPriceForFactor(destStat.Key, (int)enh);
          destEq.SetMagicStat(destStat.Key, destStat.Value);
        }
      }

      if (srcHadEmptyEnch || destHadEmptyEnch)
      {
        if (destEq.GetMagicStats().Count < 3 && !destEq.Enchantable)
          destEq.MakeEnchantable();
      }
      destEq.WasCrafted = true;
      destEq.CraftingRecipe = RecipeKind.TwoEq;

      //I noticed price is too high comparing to unique items, maybe price should be calculated from scratch ?
      //priceInc /= 2;

      destEq.Price += (int)priceInc;

      return ReturnCraftedLoot(destEq);
    }

    int GetEnhStatValue(float currentVal, float betterEqPrice, float worseEqPrice)
    {
      float factor = 10;
      factor += factor * (worseEqPrice / betterEqPrice);
      if (factor > 16)
        factor = 16;
      var enh = currentVal * factor / 100f;
      if (enh < 1)
        enh = 1;

      return (int)Math.Ceiling(enh);
    }

    private Tuple<Loot, string> CraftOneEq(Equipment srcEq)
    {
      if (srcEq.WasCraftedBy(RecipeKind.TwoEq))
        return ReturnCraftingError("Can not craft equipment which was already crafted in pairs"); ;

      var srcLootKind = srcEq.EquipmentKind;
      var lks = Equipment.GetPossibleLootKindsForCrafting().ToList();
      var destLk = RandHelper.GetRandomElem<EquipmentKind>(lks, new EquipmentKind[] { srcLootKind });
      var lootGenerator = container.GetInstance<LootGenerator>();
      var destEq = lootGenerator.GetRandomEquipment(destLk, srcEq.MinDropDungeonLevel);
      if (srcEq.Class == EquipmentClass.Magic)
      {
        destEq.SetClass(EquipmentClass.Magic, srcEq.MinDropDungeonLevel, null, srcEq.IsSecondMagicLevel);
      }
      var srcStatsCount = srcEq.GetMagicStats().Count;
      if (srcStatsCount > destEq.GetMagicStats().Count)
      {
        var diff = srcStatsCount - destEq.GetMagicStats().Count;
        for (int i = 0; i < diff; i++)
        {
          destEq.AddRandomMagicStat();
        }
      }
      if (srcEq.Enchantable)
      {
        destEq.MakeEnchantable();
      }
      destEq.WasCrafted = true;
      destEq.CraftingRecipe = RecipeKind.OneEq;
      return ReturnCraftedLoot(destEq);
    }

    private Tuple<Loot, string> HandleAllGems(List<Loot> lootToConvert)
    {
      if (lootToConvert.Count != 3)
        return ReturnCraftingError("Invalid amount of gems");
      List<Gem> gems = lootToConvert.Cast<Gem>().ToList();
      var allSameKind = gems.All(i => i.GemKindValue == gems[0].GemKindValue);
      var allSameSize = gems.All(i => i.GemSizeValue == gems[0].GemSizeValue);
      if (allSameKind && allSameSize)
      {
        if (gems[0].GemSizeValue == GemSize.Big)
          return ReturnCraftingError("Big gems can not be crafted");
        var gem = new Gem();
        gem.GemKindValue = gems[0].GemKindValue;
        gem.GemSizeValue = gems[0].GemSizeValue == GemSize.Small ? GemSize.Medium : GemSize.Big;
        gem.SetProps();
        return ReturnCraftedLoot(gem);
      }

      return ReturnCraftingError("All gems must be of the same size and type");
    }

    //public Loot Convert(Loot loot)
    //{
    //  return new ManaPotion();
    //  }
    //  else if (loot is Gem)
    //  {
    //    var gem = loot as Gem;
    //    var kind = gem.GemKindValue;
    //    var destKind = RandHelper.GetRandomEnumValue<GemKind>(new GemKind[] { kind });
    //    var destGem = new Gem(1);
    //    destGem.GemSizeValue = gem.GemSizeValue;
    //    destGem.Price = gem.Price;
    //    return destGem;
    //  }

    //  return null;
    //}
  }
}
