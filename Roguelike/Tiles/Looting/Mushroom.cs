using Dungeons.Core;
using Roguelike.Tiles.Looting;

namespace Roguelike.Tiles
{
  public enum MushroomKind { BlueToadstool, RedToadstool, Boletus };
  public enum SpecialPotionKind
  {
    Unknown, Strength, Magic
  }

  public class Mushroom : Food
  {
    public MushroomKind MushroomKind;
    public PotionKind SrcPotion { get; set; }
    public SpecialPotionKind DestPotion { get; set; }

    public Mushroom() : this(RandHelper.GetRandomEnumValue<MushroomKind>())
    {
    }

    //public float GetStatIncrease(LivingEntity caller)
    //{
    //  var divider = 6;
    //  var inc = caller.Stats[EnhancedStat].TotalValue / divider;
    //  return inc;
    //}

    public Mushroom(MushroomKind kind) : base(FoodKind.Mushroom)
    {
      Symbol = '-';
      SetMushroomKind(kind);
      Price = 15;
    }

    public override string GetId()
    {
      return base.GetId() + "_"+ MushroomKind.ToString();
    }

    public void SetMushroomKind(MushroomKind kind)
    {
      MushroomKind = kind;
      Name = kind.ToString();
            
      Name += " of " + MushroomKind;
      if (MushroomKind == MushroomKind.BlueToadstool)
      {
        SrcPotion = PotionKind.Mana;
        DestPotion = SpecialPotionKind.Magic;
      }

      if (MushroomKind == MushroomKind.BlueToadstool || MushroomKind == MushroomKind.RedToadstool)
        this.EffectType = EffectType.Poisoned;

      DisplayedName = Name;
      SetPrimaryStatDesc();
      SetDefaultTagFromKind();
    }

    protected virtual void SetDefaultTagFromKind()
    {
      if (MushroomKind == MushroomKind.BlueToadstool)
        tag1 = "mash_BlueToadstool1";
      else if (MushroomKind == MushroomKind.RedToadstool)
        tag1 = "mash_RedToadstool1";
      else
        tag1 = "mash_Boletus";
    }

    //public bool ApplyTo(Potion potion, out string error)
    //{
    //  error = "";
    //  return false;
    //}

    //public override LootBase CreateCrafted(LootBase other)
    //{
    //  if (other is HealthPotion)
    //  {
    //    return new SpecialPotion(SpecialPotionKind.Strength, false);
    //  }
    //  else if (other is ManaPotion)
    //  {
    //    return new SpecialPotion(SpecialPotionKind.Magic, false);
    //  }

    //  return null;
    //}

    //public override bool IsCraftableWith(LootBase other)
    //{
    //  if (other is Potion)
    //  {
    //    return true;
    //  }

    //  return false;
    //}
    void SetPrimaryStatDesc()
    {
      string desc = "Turns " + Extensions.FirstCharToUpper(SrcPotion.ToString()) + " Potion into";
      if (DestPotion == SpecialPotionKind.Magic)
        desc += " a Magic ";
      else
        desc += " a Strength ";

      desc += "Potion";
      primaryStatDesc = desc;
    }

    public override string PrimaryStatDescription
    {
      get { return primaryStatDesc; }
    }

    public override string ToString()
    {
      return base.ToString() + " " + MushroomKind;
    }

    //public Loot Loot => this;

    //public EntityStatKind EnhancedStat => EntityStatKind.Health;

    public override string[] GetExtraStatDescription()
    {
     
      //var potion = Extensions.FirstCharToUpper(SrcPotion.ToString()) + " Potion";
      //var gm = GameManager.Instance;
      //if (gm.GameSettings.AllowInPlaceInventoryCrafting)
      //{
      //  if (extraStatDescription == null || extraStatDescription[0].StartsWith("Use it"))//TODO
      //  {
      //    extraStatDescription = new string[1];
      //    extraStatDescription[0] = "Drop it on the " + potion + "\r\n  in the Inventory";
      //  }
      //}
      //else
      //{
      //  if (extraStatDescription == null || extraStatDescription[0].StartsWith("Drop it"))
      //  {
      //    extraStatDescription = new string[1];
      //    extraStatDescription[0] = "Use it on Crafting Panel along with\r\n" + potion + " and the Custom Recipe";
      //  }
      //}
      return extraStatDescription;
    }
  }
}
