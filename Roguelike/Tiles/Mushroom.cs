using Dungeons.Core;
using Roguelike.Tiles.Looting;

namespace Roguelike.Tiles
{
  public enum MushroomKind { BlueToadstool, RedToadstool, Boletus };
  public enum SpecialPotionKind
  {
    Unknown, Strength, Magic
  }

  public class Mushroom : Loot
  {
    public MushroomKind MushroomKindValue;
    public PotionKind SrcPotion { get; set; }
    public SpecialPotionKind DestPotion { get; set; }

    public Mushroom() : this(RandHelper.GetRandomEnumValue<MushroomKind>())
    {
    }

    public Mushroom(MushroomKind kind)
    {
      Symbol = '-';
      LootKind = LootKind.Food;
      SetKind(kind);
      Price = 15;
      StackedInInventory = true;
    }

    public void SetKind(MushroomKind kind)
    {
      MushroomKindValue = kind;
      Name = kind.ToString();
      //this.AssetName = "red_toadstool";
      Name += " of " + MushroomKindValue;
      if (MushroomKindValue == MushroomKind.BlueToadstool)
      {
        SrcPotion = PotionKind.Mana;
        DestPotion = SpecialPotionKind.Magic;
        //this.AssetName = "blue_toadstool";
      }

      DisplayedName = Name;
      SetPrimaryStatDesc();
      tag1 = "mash3";
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
