#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;
using System;
using Roguelike.Events;
using Roguelike.Attributes;

namespace Roguelike.Tiles
{
  public class Hero : AdvancedLivingEntity
  {
    public Hero(): base(new Point().Invalid(), '@')
    {
      Stats.SetNominal(EntityStatKind.Health, 15);//level up +2
                                                                                     // Character.Mana = 40;
      Stats.SetNominal(EntityStatKind.Strength, 15);//15
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defence, 10);

      CreateInventory();

      Dirty = true;//TODO
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public override string ToString()
    {
      return base.ToString();// + Data.AssetName;
    }

    public void Consume(Food food)
    {
      //hero turn?
      //if (loot.LootKind == LootKind.Food)//IDrinkable ?
      {
        //var ac = LootManager.CreateLootGameAction(loot, "Drunk " + loot.Name);
        //PlaySound("drink");
        if (inventory.Contains(food))
        {
          Stats.IncreaseStatFactor(food.EnhancedStat);// (loot as Potion).StatKind);
          inventory.Remove(food);
          AppendAction(new LootAction(food));
        }
        //else if (loot is Hooch)
        //  Hero.AddLastingEffect(LivingEntity.EffectType.Hooch, 6);

        //return ac;
      }

      //eturn null;
    }
  }
}
