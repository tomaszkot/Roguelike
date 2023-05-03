using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Generators;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Roguelike.Crafting
{
  public interface ILootCrafter
  {
    CraftingResult Craft(Recipe recipe, List<Loot> lootToCraft);
  }

  public abstract class LootCrafterBase : ILootCrafter
  {
    static CraftingResult error = new CraftingResult(null);
    public abstract CraftingResult Craft(Recipe recipe, List<Loot> lootToCraft);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loot"></param>
    /// <param name="deleteCraftedLoot">Normally true but in rare cases when Eq is enhanced/fixed (e.g. Magical weapon recharge) false</param>
    /// <returns></returns>
    protected CraftingResult ReturnCraftedLoot(List<Loot> loot, bool deleteCraftedLoot = true)
    {
      return new CraftingResult(loot) { DeleteCraftedLoot = deleteCraftedLoot };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loot"></param>
    /// <param name="deleteCraftedLoot">Normally true but in rare cases when Eq is enhanced/fixed (e.g. Magical weapon recharge) false</param>
    /// <returns></returns>
    protected CraftingResult ReturnCraftedLoot(Loot loot, bool deleteCraftedLoot = true)
    {
      if (loot == null)//ups
        return ReturnCraftingError("Improper ingredients");
      return ReturnCraftedLoot(new List<Loot>() { loot }, deleteCraftedLoot);
    }

    protected CraftingResult ReturnCraftingError(string errorMessage)
    {
      error.Message = errorMessage;
      return error;
    }
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

    protected List<T> Filter<T>(List<Loot> lootToConvert)
    {
      return lootToConvert.Where(i => i is T).Cast<T>().ToList();
    }

    protected T FilterOne<T>(List<Loot> lootToConvert)
    {
      return lootToConvert.Where(i => i is T).Cast<T>().FirstOrDefault();
    }

    int GetStackedCount<T>(List<Loot> lootToConvert, string name = "") where T : StackedLoot
    {
      if(name.Any())
      {
        lootToConvert = lootToConvert.Where(i => i.Name == name).ToList();
      }
      var stacked = Filter<T>(lootToConvert).FirstOrDefault();
      return stacked != null ? stacked.Count : 0;
    }

    List<Mushroom> GetToadstools(List<Loot> lootToConvert)
    {
      return lootToConvert.Where(i => i.IsToadstool()).Cast<Mushroom>().ToList();
    }

    int GetToadstoolsCount(List<Loot> lootToConvert) 
    {
      var toadstools = lootToConvert.Where(i => i.IsToadstool()).ToList();
      return GetStackedCount<StackedLoot>(toadstools);
    }

    public override CraftingResult Craft(Recipe recipe, List<Loot> lootToConvert)
    {
      if (recipe == null)
        return ReturnCraftingError("Recipe not set");

      if (recipe.MagicDustRequired > 0)
      {
        var magicDust = lootToConvert.Where(i => i is MagicDust).Cast<MagicDust>().FirstOrDefault();
        if (magicDust == null || magicDust.Count < recipe.MagicDustRequired)
        {
          return ReturnCraftingError("Invalid amount of Magic Dust");
        }
      }
      lootToConvert = lootToConvert.Where(i => !(i is MagicDust)).ToList();
      var eqs = lootToConvert.Where(i => i is Equipment).Cast<Equipment>().ToList();

      if (eqs.Any(i => i.Class == EquipmentClass.Unique))
        return ReturnCraftingError("Unique items can not crafted");

      if (lootToConvert.Any())
      {
        var sulfCount = GetStackedCount<StackedLoot>(lootToConvert, "Sulfur");
        var hoochCount = GetStackedCount<Hooch>(lootToConvert);

        if (lootToConvert.Count == 2 && recipe.Kind == RecipeKind.Custom)
        {
          return HandleCustomRecipe(lootToConvert);
        }
        else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.UnEnchantEquipment)
        {
          return UnEnchantEquipment(lootToConvert);
        }
        else if (recipe.Kind == RecipeKind.CraftSpecialPotion && lootToConvert.Count == 2)
        {
          var healthPotion = lootToConvert.Where(i => i.IsPotionKind(PotionKind.Health)).FirstOrDefault();
          var manaPotion = lootToConvert.Where(i => i.IsPotionKind(PotionKind.Mana)).FirstOrDefault();
          if (healthPotion != null || manaPotion != null)
          {
            var toad = lootToConvert.Where(i => i.IsToadstool());
            if (toad != null)
            {
              Potion pot = null;
              if (healthPotion != null)
                pot = new SpecialPotion(SpecialPotionKind.Strength, SpecialPotionSize.Small);
              else
                pot = new SpecialPotion(SpecialPotionKind.Magic, SpecialPotionSize.Small);
              return ReturnCraftedLoot(pot);
            }
          }
        }
        else if (recipe.Kind == RecipeKind.RechargeMagicalWeapon)
        {
          return RechargeMagicalWeapon(lootToConvert);
        }
        else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.AntidotePotion)
        {
          return AntidotePotion(lootToConvert);
        }
        else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.Pendant)
        {
          return Pendant(lootToConvert);
        }
        else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.EnchantEquipment)
        {
          return EnchantEq(lootToConvert);
        }
        else if ((recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.ExplosiveCocktail) &&
          sulfCount > 0 && hoochCount > 0)
        {
          if (sulfCount != hoochCount)
            return ReturnCraftingError("Number of ingradients must be the same (except for Magic Dust)");
          return ReturnCraftedLoot(new ProjectileFightItem(FightItemKind.ExplosiveCocktail, null));
        }

        var allGems = lootToConvert.All(i => i is Gem);
        if ((recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.ThreeGems) && allGems && lootToConvert.Count == 1 &&
          GetStackedCount<Gem>(lootToConvert) == 3)
        {
          return HandleAllGems(lootToConvert);
        }

        var hpCount = lootToConvert.Where(i => i.IsPotionKind(PotionKind.Health)).Count();
        var mpCount = lootToConvert.Where(i => i.IsPotionKind(PotionKind.Mana)).Count();
        var toadstools = lootToConvert.Where(i => i.IsToadstool()).ToList();
        var toadstoolsCount = GetStackedCount<StackedLoot>(toadstools);
        if ((recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.TransformPotion) && 
            (lootToConvert.Count == 2 && toadstoolsCount == 1 && (hpCount == 1 || mpCount == 1)))
        {
          //if ((lootToConvert[0] as Potion).Count == 1)//TODO allow many conv (use many M Dust)
          var potion = lootToConvert.Where(i => i.IsPotion()).Single();
   
          if (potion.AsPotion().Kind == PotionKind.Mana)
          {
            if (toadstools.Single().AsToadstool().MushroomKind == MushroomKind.RedToadstool)
              return ReturnCraftedLoot(new Potion(PotionKind.Health));
          }
          else
          {
            if (toadstools.Single().AsToadstool().MushroomKind == MushroomKind.BlueToadstool)
              return ReturnCraftedLoot(new Potion(PotionKind.Mana));
          }
        }

        if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.Toadstools2Potion)
        {
          var allToadstool = lootToConvert.All(i => i.IsToadstool());
          if (allToadstool)
          {
            var toadstool = lootToConvert[0].AsToadstool();
            if (toadstool != null && toadstool.Count == 3)
            {
              return Toadstools2Potion(toadstool);
            }
          }
        }
      }

      if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.OneEq)
      {
        if (lootToConvert.Count == 1)
        {
          var srcEq = lootToConvert[0] as Equipment;
          if (srcEq != null)
          {
            return CraftOneEq(srcEq);
          }
        }
      }
      else if (lootToConvert[0] is Gem && (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.TransformGem))
      {
        return TransformGem(lootToConvert);
      }
      else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.Arrows || recipe.Kind == RecipeKind.Bolts)
      {
        return ConvertBoltsOrArrows(recipe, lootToConvert);
      }
      else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.NiesiolowskiSoup)
      {
        return CraftNiesiolowskiSoup(recipe, lootToConvert);
      }
      else if (recipe.Kind == RecipeKind.Custom || recipe.Kind == RecipeKind.TwoEq)
      {
        if (lootToConvert.Count == 2)
        {
          if(eqs.Count == 2)
            return CraftTwoEq(eqs);
          if (lootToConvert.Any(i => i is KeyHalf))
          {            
            var parts = Filter<KeyHalf>(lootToConvert);
            if (parts.Count != 2 || !parts[0].Matches(parts[1]))
            {
              return ReturnCraftingError("Two matching parts of a key are needed");
            }

            return ReturnCraftedLoot(new Key() { KeyName = (lootToConvert[0] as KeyHalf).KeyName, Kind = KeyKind.BossRoom }); ;  ;
          }

          if (lootToConvert.Any(i => i is KeyMold))
          {
            return ConvertMold(recipe, lootToConvert);
            
          }
        }
      }
      else if (recipe.Kind == RecipeKind.Custom && eqs.Count == 1)
      {
        if (eqs[0].EquipmentKind == EquipmentKind.Weapon)
        {
          var spsCount = GetStackedCount<SpecialPotion>(lootToConvert);
          if (spsCount == lootToConvert.Count - 1 && spsCount <= 2)
          {
            return ReturnStealingEq(eqs, Filter<SpecialPotion>(lootToConvert));
          }
        }
      }
      else if (recipe.Kind == RecipeKind.Custom && lootToConvert.Count == 2)
      {
        //turn one Toadstool kind into other (using Potion)
        var toadstoolsCount = GetToadstoolsCount(lootToConvert);
        var potions = lootToConvert.Where(i => i.IsPotion()).ToList();
        if (toadstoolsCount == 1 && potions.Count == 1)
        {
          var tk = GetToadstools(lootToConvert)[0].MushroomKind;
          var pk = (potions[0].AsPotion()).Kind;

          if (tk == MushroomKind.RedToadstool && pk == PotionKind.Mana)
            return ReturnCraftedLoot(new Mushroom(MushroomKind.BlueToadstool));
          else if (tk == MushroomKind.BlueToadstool && pk == PotionKind.Health)
            return ReturnCraftedLoot(new Mushroom(MushroomKind.RedToadstool));
        }
      }
      

      return ReturnCraftingError(InvalidIngredients);
    }

    protected virtual CraftingResult ConvertMold(Recipe recipe, List<Loot> lootToConvert)
    {
      return ReturnCraftingError("TODO");
    }

    private CraftingResult UnEnchantEquipment(List<Loot> lootToConvert)
    {
      var eqsToUncraft = Filter<Equipment>(lootToConvert);
      if (eqsToUncraft.Count != 1 || eqsToUncraft[0].Enchants.Count == 0)
        return ReturnCraftingError("One enchanted piece of equipment is required");
      var eq = eqsToUncraft[0];
      var enchs = eqsToUncraft[0].Enchants.Select(i => i.Enchanter).ToList();
      enchs.ForEach(i => i.Count = 1);
      foreach (var ench in eqsToUncraft[0].Enchants)
      {
        foreach (var stat in ench.StatKinds)
        {
          eq.RemoveMagicStat(stat, ench.StatValue);
        }
      }
      eq.Enchants = new List<Enchant>();

      var lootItems = new List<Loot>() { eqsToUncraft[0] };
      lootItems.AddRange(enchs);
      return ReturnCraftedLoot(lootItems);//deleteCraftedLoot:false
    }

    private CraftingResult TransformGem(List<Loot> lootToConvert)
    {
      var srcGem = lootToConvert[0] as Gem;
      var destKind = RandHelper.GetRandomEnumValue<GemKind>(new GemKind[] { srcGem.GemKind, GemKind.Unset });
      var destGem = new Gem(destKind);
      destGem.EnchanterSize = srcGem.EnchanterSize;
      destGem.SetProps();
      return ReturnCraftedLoot(destGem);
    }

    private CraftingResult Toadstools2Potion(Mushroom toadstool)
    {
      Potion potion = null;
      if (toadstool.MushroomKind == MushroomKind.BlueToadstool)
        potion = new Potion(PotionKind.Mana);
      else
        potion = new Potion(PotionKind.Health);
      return ReturnCraftedLoot(potion);
    }

    private CraftingResult EnchantEq(List<Loot> lootToConvert)
    {
      var equips = lootToConvert.Where(i => i is Equipment);
      var equipsCount = equips.Count();
      if (equipsCount == 0)
        return ReturnCraftingError("Equipment is needed by the Recipe");
      if (equipsCount > 1)
        return ReturnCraftingError("One equipment is needed by the Recipe");
      var enchanters = lootToConvert.Where(i => !(i is Equipment)).ToList();
      if (enchanters.Any(i => !(i is Enchanter)))
      {
        return ReturnCraftingError("Only enchanting items (gems, claws,...)are alowed by the Recipe");
      }
      var eq = equips.ElementAt(0) as Equipment;
      if (!eq.Enchantable)
        return ReturnCraftingError("Equipment is not " + Translations.Strings.Enchantable.ToLower());
      var freeSlots = eq.EnchantSlots - eq.Enchants.Count;
      if (freeSlots < enchanters.Count())
        return ReturnCraftingError("Too many enchantables added");
      string err;
      foreach (var ench in enchanters.Cast<Enchanter>())
      {
        if (!ench.ApplyTo(eq, out err))
          return ReturnCraftingError(InvalidIngredients);
      }

      return ReturnCraftedLoot(eq, false);
    }

    private CraftingResult Pendant(List<Loot> lootToConvert)
    {
      var cords = GetStackedCount<Cord>(lootToConvert);
      if (cords == 0)
        return ReturnCraftingError("Cord is needed by the Recipe");

      var amulet = Equipment.CreatePendant();
      return ReturnCraftedLoot(amulet);
    }

    private CraftingResult AntidotePotion(List<Loot> lootToConvert)
    {
      var plants = GetStackedCount<Plant>(lootToConvert);
      if (plants != 1 || Filter<Plant>(lootToConvert).Where(i => i.Kind == PlantKind.Thistle).Count() != 1)
        return ReturnCraftingError("One thistle is needed by the Recipe");

      var hoohCount = GetStackedCount<Hooch>(lootToConvert);
      if (hoohCount != 1)
        return ReturnCraftingError("One hooch is needed by the Recipe");

      return ReturnCraftedLoot(new Potion(PotionKind.Antidote), true);
    }

    private CraftingResult RechargeMagicalWeapon(List<Loot> lootToConvert)
    {
      var equips = lootToConvert.Where(i => i is Weapon wpn0 && wpn0.IsMagician).Cast<Weapon>().ToList();
      var equipsCount = equips.Count();
      if (equipsCount != 1)
        return ReturnCraftingError("One charge emitting weapon is needed by the Recipe");

      var wpn = equips[0];
      //foreach (var wpn in equips)
      {
        (wpn.SpellSource as WeaponSpellSource).Restore();
        wpn.UpdateMagicWeaponDesc();
      }
      return ReturnCraftedLoot(wpn, false);
    }

    private CraftingResult CraftNiesiolowskiSoup(Recipe recipe, List<Loot> lootToConvert)
    {
      var sorrel = Filter<Plant>(lootToConvert).Where(i => i.Kind == PlantKind.Sorrel).FirstOrDefault();
      if (sorrel == null)
        ReturnCraftingError("Sorrel not available");

      var plum = Filter<Food>(lootToConvert).Where(i => i.Kind == FoodKind.Plum).FirstOrDefault();
      if (plum == null)
        ReturnCraftingError("Plum not available");

      var count = GetCraftedStackedCount(lootToConvert);

      return ReturnCraftedLoot(new Food() { Kind = FoodKind.NiesiolowskiSoup, Count = count });
    }

    protected int GetCraftedStackedCount(List<Loot> lootToConvert)
    {
      return lootToConvert.Cast<StackedLoot>().Min(i => i.Count);
    }

    protected virtual CraftingResult ConvertBoltsOrArrows(Recipe recipe, List<Loot> lootToConvert)
    {
      return ReturnCraftingError("TODO");
    }

    private CraftingResult HandleCustomRecipe(List<Loot> lootToConvert)
    {
      if (lootToConvert.Count == 2)
      {
        //if (
        //  (lootToConvert[0].IsToadstool() && lootToConvert[1].IsPotion(PotionKind.Health)) ||
        //  (lootToConvert[1].IsToadstool() && lootToConvert[0].IsPotion(PotionKind.Health)) ||
        //  (lootToConvert[0].IsToadstool() && lootToConvert[1].IsPotion(PotionKind.Mana)) ||
        //  (lootToConvert[1].IsToadstool() && lootToConvert[0].IsPotion(PotionKind.Mana))
        //  )
        //{
        //  var srcLoot = lootToConvert[0].IsToadstool() ? lootToConvert[0] : lootToConvert[1];
        //  var destLoot = lootToConvert[0].IsToadstool() ? lootToConvert[1] : lootToConvert[0];
        //  var crafted = srcLoot.CreateCrafted(destLoot);
        //  if(crafted!=null)
        //    return ReturnCraftedLoot(crafted);
        //}
        //////////////////////////////////////////////////////////////////////////////
        if (lootToConvert[0] is Gem && lootToConvert[1] is Equipment ||
           lootToConvert[1] is Gem && lootToConvert[0] is Equipment
          )
        {
          var gem = lootToConvert[0] is Gem ? lootToConvert[0] as Gem : lootToConvert[1] as Gem;
          var eq = lootToConvert[0] is Equipment ? lootToConvert[0] as Equipment : lootToConvert[1] as Equipment;
          var err = "";
          if (gem.ApplyTo(eq, out err))
          {
            return ReturnCraftedLoot(eq);
          }
          else
            return ReturnCraftingError(err);
        }
        //////////////////////////////////////////////////////////////////////////////
        //if (lootToConvert[0].IsCraftableWith(lootToConvert[1]) ||
        //    lootToConvert[1].IsCraftableWith(lootToConvert[0])
        //  )
        //{
        //  if (lootToConvert[0].IsCraftableWith(lootToConvert[1]))
        //    return ReturnCraftedLoot(lootToConvert[0].CreateCrafted(lootToConvert[1]));
        //  else
        //    return ReturnCraftedLoot(lootToConvert[1].CreateCrafted(lootToConvert[0]));
        //}

        return ReturnCraftingError("Improper ingredients");
      }
      else
        return ReturnCraftingError("Invalid amount of ingradients");
    }


    private CraftingResult ReturnStealingEq(List<Equipment> eqs, List<SpecialPotion> sps)
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
        var esk = sp.SpecialPotionKind == Tiles.Looting.SpecialPotionKind.Strength ? EntityStatKind.LifeStealing : EntityStatKind.ManaStealing;
        if (esk == EntityStatKind.LifeStealing)
          ls += sp.GetEnhValue();
        else
          ms += sp.GetEnhValue();

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

    private CraftingResult CraftTwoEq(List<Equipment> eqs)
    {
      var eq1 = eqs[0];
      var eq2 = eqs[1];
      if (eq1.EquipmentKind != eq2.EquipmentKind)
        return ReturnCraftingError("Equipment for crafting must be of the same type");

      if (eq1.WasCraftedBy(RecipeKind.TwoEq) || eq2.WasCraftedBy(RecipeKind.TwoEq))
        return ReturnCraftingError("Can not craft equipment which was already crafted in pairs");
      var destEq = eq1.Price > eq2.Price ? eq1 : eq2;

      var srcEq = destEq == eq1 ? eq2 : eq1;
      var srcHadEmptyEnch = srcEq.Enchantable && !srcEq.MaxEnchantsReached();
      var destHadEmptyEnch = destEq.Enchantable && !destEq.MaxEnchantsReached();

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

    private CraftingResult CraftOneEq(Equipment srcEq)
    {
      if (srcEq.WasCraftedBy(RecipeKind.TwoEq))
        return ReturnCraftingError("Can not craft equipment which was already crafted in pairs"); ;

      var srcLootKind = srcEq.EquipmentKind;
      var lks = Equipment.GetPossibleLootKindsForCrafting().ToList();
      var destLk = RandHelper.GetRandomElem<EquipmentKind>(lks, new EquipmentKind[] { srcLootKind });
      var lootGenerator = container.GetInstance<LootGenerator>();
      var destEq = lootGenerator.GetRandomEquipment(destLk, srcEq.MinDropDungeonLevel, null);
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

    private CraftingResult HandleAllGems(List<Loot> lootToConvert)
    {
      if (lootToConvert.Count != 1)
        return ReturnCraftingError("Invalid amount of gems");
      List<Gem> gems = Filter<Gem>(lootToConvert);
      var allSameKind = gems.All(i => i.GemKind == gems[0].GemKind);
      var allSameSize = gems.All(i => i.EnchanterSize == gems[0].EnchanterSize);
      if (allSameKind && allSameSize)
      {
        if (gems[0].EnchanterSize == EnchanterSize.Big)
          return ReturnCraftingError("Big gems can not be crafted");
        var gem = new Gem();
        gem.GemKind = gems[0].GemKind;
        gem.EnchanterSize = gems[0].EnchanterSize == EnchanterSize.Small ? EnchanterSize.Medium : EnchanterSize.Big;
        gem.SetProps();
        return ReturnCraftedLoot(gem);
      }

      return ReturnCraftingError("All gems must be of the same size and type");
    }

  }
}
