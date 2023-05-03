using Dungeons.Core;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Generators
{
  public class ChanceAtGameStart
  {
    int weaponsCount = 0;
    public enum ChanceKind { Unset, ThrowWeapon, ThrowRecipe, ThrowCord, ThrowTinyTrophy, ThrowScroll, ThrowGem, ThrowPotion, ThrowFood, ThrowArmor };
    Dictionary<ChanceKind, ChanceAtGameStartStatus> chances = new Dictionary<ChanceKind, ChanceAtGameStartStatus>();
    const int DefaultChance = 20;

    public Dictionary<ChanceKind, ChanceAtGameStartStatus> Chances { get => chances; set => chances = value; }
    public bool BowLikeGenerated { get => bowLikeGenerated; set => bowLikeGenerated = value; }

    public ChanceAtGameStart()
    {
      var values = EnumHelper.Values<ChanceKind>(true);
      foreach (var chance in values)
      {
        var ch = DefaultChance;
        if (chance == ChanceKind.ThrowWeapon)
          ch += 10;
        chances[chance] = new ChanceAtGameStartStatus() { Chance = ch };
      }
    }

    bool bowLikeGenerated = false;

    public Loot TryGenerate(Roguelike.Generators.LootGenerator lg, Roguelike.State.GameState gs, ILootSource ls)
    {
      var arlDone = chances.Where(i => i.Value.Done).Select(i => i.Key).ToList();
      var anyNotDone = chances.Where(i => !i.Value.Done).Any();
      Loot loot = null;

      if (anyNotDone)
      {
        arlDone.Add(ChanceKind.Unset);
        var kind = RandHelper.GetRandomEnumValue<ChanceKind>(arlDone.ToArray());
        loot = TryGenerate(lg, gs as OuaDII.State.GameState, kind, ls);
        if (loot is Weapon wpn)
        {
          if (wpn.IsBowLike)
            bowLikeGenerated = true;
        }

      }
      else
      {
        int k = 0;
        k++;
      }
      if (loot == null && !bowLikeGenerated && ls!=null && ls.Level <= 2)
      {
        if (RandHelper.GetRandomDouble() > 0.7f)
        {
          loot = lg.GetLootByAsset("crude_bow");
          bowLikeGenerated = true;
        }
        else if (RandHelper.GetRandomDouble() > 0.7f)
        {
          loot = lg.GetLootByAsset("crude_crossbow");
          bowLikeGenerated = true;
        }
      }
      if (loot != null)
        loot.FromGameStart = true;
      return loot;
    }

    Loot TryGenerate(Roguelike.Generators.LootGenerator LootGenerator, State.GameState gameState, ChanceKind kind, ILootSource ls)
    {
      var ch = chances[kind];
      if (ch.Done)
        return null;

      Loot loot = null;
      var chance = RandHelper.GetRandomDouble() * 100;
      if (chance < ch.Chance)
      {

        if (kind == ChanceKind.ThrowWeapon)
          loot = LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, ls.Level, null);
        else if (kind == ChanceKind.ThrowArmor)
          loot = LootGenerator.GetRandomEquipment(EquipmentKind.Armor, ls.Level, null);
        else if (kind == ChanceKind.ThrowPotion)
        {
          loot = new Potion(PotionKind.Health);
        }
        else if (kind == ChanceKind.ThrowFood)
        {
          loot = LootGenerator.GetRandomLoot(LootKind.Food, ls.Level);
        }
        else if (kind == ChanceKind.ThrowGem)
        {
          GemKind kindGem = RandHelper.GetRandomEnumValue<GemKind>();
          loot = new Gem(kindGem);
        }
        else if (kind == ChanceKind.ThrowScroll)
          loot = LootGenerator.GetRandomLoot(LootKind.Scroll, ls.Level);
        else if (kind == ChanceKind.ThrowCord)
          loot = new Cord();
        else
        {
          string lootName = "";// 
          string[] tags = null;
          if (kind == ChanceKind.ThrowRecipe)
          {
            tags = new[] { "craft_pendant", "craft_one_eq", "craft_two_eq", "craft_recharge_magical_weapon" };
          }
          else if (kind == ChanceKind.ThrowTinyTrophy)
          {
            tags = HunterTrophy.TinyTrophiesTags.Where(i => i.Contains("small")).ToArray();
          }
          lootName = RandHelper.GetRandomElem<string>(tags);
          if (lootName.Any() && gameState.History.Looting.Count(lootName) == 0)
            loot = LootGenerator.GetLootByAsset(lootName);
        }
        if (loot != null)
        {
          if (loot is Weapon wpn)
          {
            weaponsCount++;
            if(weaponsCount > 1 || !wpn.IsBowLike)
              chances[kind].Done = true;
          }
          else
            chances[kind].Done = true;
        }
      }
      else
        ch.Chance *= 3;

      return loot;
    }
  }

}
