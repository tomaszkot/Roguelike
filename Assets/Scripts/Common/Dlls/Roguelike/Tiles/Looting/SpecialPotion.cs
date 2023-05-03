using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Factors;
using Roguelike.Tiles.LivingEntities;
using System;

namespace Roguelike.Tiles.Looting
{
  public enum SpecialPotionKind { Unset, Strength, Magic }
  public enum SpecialPotionSize { Small, Medium, Big }

  [Serializable]
  public class SpecialPotion : Potion
  {
    SpecialPotionKind specialPotionKind;
    SpecialPotionSize size = SpecialPotionSize.Small;
    bool used = false;

    public SpecialPotion() : this(SpecialPotionKind.Unset, SpecialPotionSize.Small)
    {

    }

    public SpecialPotion(SpecialPotionKind kind, SpecialPotionSize size) : base(PotionKind.Special)
    {
      PercentageStatIncrease = false;
      SpecialPotionKind = kind;
      Price = 50;
      this.size = size;
      if (size == SpecialPotionSize.Medium)
        Price = 100;
      else if (size == SpecialPotionSize.Big)
        Price = 200;
      Symbol = '`';
    }

    public SpecialPotionKind SpecialPotionKind
    {
      get
      {
        return specialPotionKind;
      }

      set
      {
        specialPotionKind = value;
        if (value == SpecialPotionKind.Strength)
        {
          tag1 = "strength_potion";//HACK
          Name = "Strength Potion";
          StatKind = EntityStatKind.Strength;
        }
        else if (value == SpecialPotionKind.Magic)
        {
          tag1 = "magic_potion";
          Name = "Magic Potion";
          StatKind = EntityStatKind.Magic;
        }

        //tag1 += "_"+size.ToString();maybe UI can scale ?
        Name = size.ToString().ToUpperFirstLetter() + " " + Name;
        PrimaryStatDescription = "Permamently increases " + GetDestStat();
      }
    }

    public bool Apply(AdvancedLivingEntity ent)
    {
      if (used)
        return false;

      EntityStatKind esk = GetDestStat();
      var enh = GetEnhValue();
      ent.Stats[esk].Nominal += enh;
      for (int i = 0; i < enh; i++)
        ent.EmitStatsLeveledUp(GetDestStat());
      //GameManager.Instance.AppendAction(new LootAction() { Info = ent.Name + " drunk " + Name, Loot = this, KindValue = LootAction.Kind.SpecialDrunk });
      used = true;
      return true;
    }

    public int GetEnhValue()
    {
      var value = 1;
      if (size == SpecialPotionSize.Medium)
        value = 3;
      else if (size == SpecialPotionSize.Big)
        value = 5;

      return value;
    }
        
    public override EffectiveFactor StatKindEffective
    {
      get { return new EffectiveFactor(GetEnhValue()); }
    }

    public EntityStatKind GetDestStat()
    {
      return SpecialPotionKind == SpecialPotionKind.Strength ? EntityStatKind.Strength : EntityStatKind.Magic;
    }

    public SpecialPotionSize Size { get => size; set => size = value; }

    public override string[] GetExtraStatDescription()
    {
      if (extraStatDescription == null)
      {
        var desc = "";
        extraStatDescription[0] = desc;
      }
      return extraStatDescription;
    }

    public override string ToString()
    {
      return this.Name;
    }
  }
}
