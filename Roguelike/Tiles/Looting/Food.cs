using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Tiles.Abstract;
using System;
using System.Linq;

namespace Roguelike.Tiles
{
  public enum FoodKind { Unset, Mushroom, Plum, /*Herb,*/ Meat, Fish}

  public class Food : Looting.StackedLoot, IConsumable
  {
    public FoodKind Kind;
    EntityStatKind enhancedStat = EntityStatKind.Unset;
    public static readonly FoodKind[] FoodKinds;
    public bool Roasted { get; set; }

    static Food()
    {
      FoodKinds = Enum.GetValues(typeof(FoodKind)).Cast<FoodKind>().ToArray();
    }

    public Food() : this(RandHelper.GetRandomEnumValue<FoodKind>(new[] { FoodKind.Unset, FoodKind.Mushroom }))//Mushroom has it's own c-tor
    {

    }
        
    public Food(FoodKind kind) 
    {
      Symbol = '-';
      LootKind = LootKind.Food;
      SetKind(kind);
      Price = 15;
    }

    public override string ToString()
    {
      return base.ToString() + " " + Kind;
    }

    public void DiscoverKind(string kind)
    {
      if(!kind.Trim().Any())
        throw new Exception("DiscoverKind empty kind");

      var parts = kind.Split("_".ToCharArray());
      if (!parts.Any())
        parts = new string[] { kind };

      var fk = FoodKinds.FirstOrDefault(i => i.ToString().ToLower() == parts[0].ToLower());
      if (fk == FoodKind.Unset)
      {
        throw new Exception("DiscoverKind failed for " + kind);
      }
      SetKind(fk);
      if (parts.Any(i => i.ToLower() == "roasted"))
      {
        MakeRoasted();
      }
    }

    public void MakeRoasted()
    {
      this.Roasted = true;
      SetName();
      SetPrimaryStatDesc();
      Price *= 2;
      SetDefaultTagFromKind(Kind);
    }

    public EffectType EffectType { get; set; }

    public float GetStatIncrease(LivingEntity caller)
    {
      var divider = 4;
      if (Kind == FoodKind.Mushroom)
        divider = 10;
      if (Kind == FoodKind.Plum)
        divider = 8;
      var inc = caller.Stats[EnhancedStat].TotalValue / divider;
      return inc;
    }

    public EntityStatKind EnhancedStat { get => enhancedStat; set => enhancedStat = value; }
    public Loot Loot { get => this; }

    bool IsRoastable(FoodKind kind)
    {
      return kind == FoodKind.Fish || kind == FoodKind.Meat;
    }

    public void SetKind(FoodKind kind)
    {
      Kind = kind;
      SetName();

      DisplayedName = GetNameOrDisplayedName(false);
      SetPrimaryStatDesc();
      enhancedStat = EntityStatKind.Health;
      SetDefaultTagFromKind(kind);
    }

    private string GetNameOrDisplayedName(bool settingName)
    {
      var name = settingName ? Kind.ToString() : Kind.ToDescription();
      if (IsRoastable(Kind))
      {
        if (Roasted)
          name = "Roasted " + name;
        else
          name = "Raw " + name;
      }

      return name;
    }

    private void SetName()
    {
      Name = GetNameOrDisplayedName(true);
    }

    protected virtual void SetDefaultTagFromKind(FoodKind kind)
    {
      //if (!string.IsNullOrEmpty(tag1))
      //  return;
      switch (Kind)
      {
        case FoodKind.Unset:
          break;
        case FoodKind.Plum:
          tag1 = "plum_mirabelka";
          break;
        //case FoodKind.Herb:
        //  tag1 = "szczaw";
        //  break;

        case FoodKind.Meat:
          tag1 = "meat_raw";
          if(Roasted)
            tag1 = "meat_roasted";
          break;
        case FoodKind.Fish:
          tag1 = "fish_raw";
          if (Roasted)
            tag1 = "fish_roasted";
          break;
        default:
          break;
      }
    }

    void SetPrimaryStatDesc()
    {
      string desc = "";
      
      if (Kind == FoodKind.Plum)
      {
        desc = "Sweat, delicious friut";
      }
      else if (Kind == FoodKind.Meat || Kind == FoodKind.Fish)
      {
        if(Roasted)
          desc = "Roasted, delicious piece of "+Kind.ToString().ToLower();
        else
          desc = "Raw yet nutritious piece of "+ Kind.ToString().ToLower();
      }
      primaryStatDesc = desc;
    }

    public override string PrimaryStatDescription => primaryStatDesc;

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }
  }
}
