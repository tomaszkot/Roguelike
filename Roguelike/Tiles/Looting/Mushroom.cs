using Dungeons.Core;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;

namespace Roguelike.Tiles
{
  public enum MushroomKind { BlueToadstool, RedToadstool, Boletus };

  public class Mushroom : Food
  {
    public MushroomKind MushroomKind;
    public PotionKind SrcPotion { get; set; }
    public SpecialPotionKind DestPotion { get; set; }

    public Mushroom() : this(RandHelper.GetRandomEnumValue<MushroomKind>())
    {
    }

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
      Name = MushroomKind.ToDescription();
      if (MushroomKind == MushroomKind.BlueToadstool)
      {
        //TODO
        //SrcPotion = PotionKind.Mana;
        //DestPotion = SpecialPotionKind.Magic;
        
        StatKind = Attributes.EntityStatKind.Mana;
      }
      else
        StatKind = Attributes.EntityStatKind.Health;

      NegativeFactor = MushroomKind == MushroomKind.RedToadstool;

      if (MushroomKind == MushroomKind.BlueToadstool || MushroomKind == MushroomKind.RedToadstool)
        this.EffectType = Effects.EffectType.Poisoned;

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
      //string desc = "Turns " + Extensions.FirstCharToUpper(SrcPotion.ToString()) + " Potion into";
      //if (DestPotion == SpecialPotionKind.Magic)
      //  desc += " a Magic ";
      //else
      //  desc += " a Strength ";

      //desc += "Potion.";
      string desc = PartOfCraftingRecipe;
      desc+= GetConsumeDesc(" Consumable");
      PrimaryStatDescription = desc;
    }
        
    public override string ToString()
    {
      return base.ToString() + " " + MushroomKind;
    }

    //public Loot Loot => this;

    //public EntityStatKind EnhancedStat => EntityStatKind.Health;

    public override string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }
  }
}
