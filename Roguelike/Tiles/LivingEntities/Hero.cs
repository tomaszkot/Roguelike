#define ASCII_BUILD  
using Dungeons.Core;
using Roguelike.Abstract.Inventory;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.LootContainers;
using Roguelike.Quests;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public class Hero : AdvancedLivingEntity
  {
    List<Quest> quests = new List<Quest>();
    public List<Quest> Quests
    {
      get => quests;
      set => quests = value;
    }

    public LootContainers.Crafting Crafting { get; set; }

    public Hero(Container container) : base(container, new Point().Invalid(), '@')
    {
      canAdvanceInExp = true;

      Stats.SetNominal(EntityStatKind.Health, 40);//level up +2 // 40 -> 140
      // Character.Mana = 40;
      StartStrength += 5;
      Stats.SetNominal(EntityStatKind.Strength, StartStrength);
      Stats.SetNominal(EntityStatKind.MeleeAttack, StartStrength);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defense, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      //Inventory.InvOwner = InvOwner.Hero;
      Inventory.InvBasketKind = InvBasketKind.Hero;
      CurrentEquipment.InvBasketKind = InvBasketKind.HeroEquipment;

      Crafting = Container.GetInstance<Roguelike.LootContainers.Crafting>();

      Revealed = true;

#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    protected override void InitResistance()
    {
    }

    public bool Identify(Equipment eq)
    {
      var scroll = Inventory.GetItems<Scroll>().Where(i => i.Kind == Spells.SpellKind.Identify).FirstOrDefault();
      if (scroll != null)
      {
        if (eq.Identify())
        {
          Inventory.Remove(scroll);
          if (CurrentEquipment.PrimaryEquipment.Values.Contains(eq) ||
            CurrentEquipment.SpareEquipment.Values.Contains(eq))
            RecalculateStatFactors(false);
          return true;
        }
      }
      return false;
    }

    public override string ToString()
    {
      return base.ToString();
    }

    public virtual void OnContextSwitched(Container container)
    {
      //this.Container = container;
      Inventory.Owner = this;
      Crafting.InvItems.Inventory.Owner = this;
      Crafting.Recipes.Inventory.Owner = this;
    }

    protected override float GetStrengthIncrease()
    {
      return Stats.GetCurrentValue(EntityStatKind.Strength) - StartStrength;
    }


    public Tuple<int, int> GetTotalAttackValues()
    {
      Tuple<int, int> res;
      var attack = GetMelleeHitAttackValue(false);

      var nonPhysicals = GetNonPhysicalDamages();
      foreach (var stat in nonPhysicals)
        attack += stat.Value;

      var intAttack = (int)attack;
      var variation = (int)GetAttackVariation();
      if (variation != 0)
        res = new Tuple<int, int>(intAttack - variation, intAttack + variation);
      else
        res = new Tuple<int, int>(intAttack, intAttack);

      return res;
    }

    //for UI
    public string GetTotalAttackValue()
    {
      var attack = GetTotalAttackValues();
      var value = attack.Item1.ToString();
      if (attack.Item1 != attack.Item2)
      {
        value = attack.Item1.ToString() + "-" + attack.Item2.ToString();
      }
      return value;
    }

    internal bool HasKey(string keyName)
    {
      return GetKey(keyName) != null;
    }

    internal Key GetKey(string keyName)
    {
      return this.Inventory.GetItems<Key>().Where(i => i.KeyName == keyName).SingleOrDefault();
    }

    public override string GetFormattedStatValue(EntityStatKind kind, bool round)
    {
      var value = base.GetFormattedStatValue(kind, round);
      if (kind == EntityStatKind.MeleeAttack)
      {
        value = GetTotalAttackValue();
      }
      return value;
    }

    internal void RemoveLoot(Loot loot)
    {
      Inventory.Remove(loot);
    }

    public override float GetMelleeHitAttackValue(bool withVariation)
    {
      var ad = new AttackDescription(this, AttackKind.Melee);
      var att = ad.CurrentPhysical;
      if (withVariation)//GUI is not meant to have it changed on character panel
      {
        var variation = GetAttackVariation();
        var sign = RandHelper.Random.NextDouble() > .5f ? -1 : 1;

        att += sign * variation * (float)RandHelper.Random.NextDouble();
      }
      return att;
    }

    

    public override float GetAttackVariation()
    {
      var currentWpn = GetActiveWeapon();
      if (currentWpn != null)
      {
        return currentWpn.GetPrimaryDamageVariation();
      }

      return 0;
    }

    internal void PrepareForSave()
    {
      Inventory.GetItems<Equipment>().ToList().ForEach(i => i.PrepareForSave());
      CurrentEquipment.PrimaryEquipment.ToList().ForEach
      (
        i =>
        {
          if (i.Value != null)
            i.Value.PrepareForSave();
        }
      );
    }

    public override bool CanCauseBleeding()
    {
      return CurrentEquipment.GetWeapon() != null;
    }

    public override bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      var getGold = base.GetGoldWhenSellingTo(dest);
      if (dest is Roguelike.Abstract.Tiles.IAlly && !(dest is Roguelike.Tiles.LivingEntities.Merchant))
        getGold = false;

      if (dest.Inventory.InvBasketKind == InvBasketKind.CraftingInvItems || dest.Inventory.InvBasketKind == InvBasketKind.CraftingRecipe)
        getGold = false;
      if (dest.Inventory.InvBasketKind == InvBasketKind.HeroChest)
        getGold = false;
      return getGold;
    }

    public override bool SetLevel(int level, Difficulty? diff = null)
    {
      Level = level;
      return true;
    }

    protected override float GetDamageAddition(ProjectileFightItem pfi)
    {
      if (pfi.FightItemKind == FightItemKind.PlainArrow ||
        pfi.FightItemKind == FightItemKind.PlainBolt)
      {
        var wpn = GetActiveWeapon();
        if (wpn != null && wpn.IsBowLike)
          return wpn.Damage; //GetMelleeHitAttackValue(false);
      }
      return base.GetDamageAddition(pfi);
    }
  }
}
