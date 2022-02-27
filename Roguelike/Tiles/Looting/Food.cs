using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;

namespace Roguelike.Tiles
{
  public enum FoodKind { Unset, Mushroom, Plum, /*Herb,*/ Meat, Fish, Apple }

  public class Food : Consumable
  {
    public FoodKind Kind;
    public static readonly FoodKind[] FoodKinds;

    static Food()
    {
      FoodKinds = Enum.GetValues(typeof(FoodKind)).Cast<FoodKind>().ToArray();
    }

    public Food() : this(RandHelper.GetRandomEnumValue<FoodKind>(new[] { FoodKind.Unset, FoodKind.Mushroom }))//Mushroom has it's own c-tor
    {

    }

    public Food(FoodKind kind)
    {
      ConsumptionSteps = 5;
      Symbol = '-';
      LootKind = LootKind.Food;
      SetKind(kind);
      Price = 15;
    }

    protected override string StatToDescription()
    {
      if (EffectType == Effects.EffectType.Poisoned)
      {
        return "Poison Damage: " + Math.Abs(StatKindEffective.Value);
      }
      return base.StatToDescription();
    }

    public void SetPoisoned()
    {
      EffectType = Effects.EffectType.Poisoned;
      NegativeFactor = true;
      PercentageStatIncrease = false;
      StatKind = EntityStatKind.PoisonAttack;
      defaultEffectiveFactor = 3;

      SetPrimaryStatDesc();
    }

    public override string ToString()
    {
      return base.ToString() + " " + Kind;
    }

    public void DiscoverKind(string kind)
    {
      if (!kind.Trim().Any())
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
      Price *= 2;
      SetKindRelatedMembers(Kind);
    }

    bool IsRoastable(FoodKind kind)
    {
      return kind == FoodKind.Fish || kind == FoodKind.Meat;
    }

    public void SetKind(FoodKind kind)
    {
      Kind = kind;
      SetKindRelatedMembers(kind);
    }

    private void SetKindRelatedMembers(FoodKind kind)
    {
      SetName();
      DisplayedName = GetNameOrDisplayedName(false);
      SetPrimaryStatDesc();
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
          if (Roasted)
            tag1 = "meat_roasted";
          break;
        case FoodKind.Fish:
          tag1 = "fish_raw";
          if (Roasted)
            tag1 = "fish_roasted";
          break;
        case FoodKind.Apple:
          tag1 = "apple";
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
        desc = "Sweet, delicious fruit";
      }
      else if (Kind == FoodKind.Meat || Kind == FoodKind.Fish)
      {
        if (Roasted)
          desc = "Roasted, delicious piece of " + Kind.ToString().ToLower();
        else
          desc = "Raw, yet nutritious piece of " + Kind.ToString().ToLower();
      }
      else if (Kind == FoodKind.Apple)
      {
        if (EffectType == Effects.EffectType.Poisoned)
          desc = "juicy, poisonous fruit";
        else
          desc = "juicy, delicious fruit";
      }
      desc = GetConsumeDesc(desc);
      PrimaryStatDescription = desc;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind + "_" + Roasted;
    }
  }
}
