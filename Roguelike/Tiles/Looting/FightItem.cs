using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Tiles.Looting
{
  public enum FightItemKind
  {
    Unset,
    ExplodePotion,
    Knife,
    Trap,
    Stone
  }

  public class FightItem : StackedLoot
  {
    private FightItemKind kind;
    public float baseDamage = 2.0f;

    protected PassiveAbilityKind abilityKind;
    protected string primaryFactorName = "Damage";
    protected string auxFactorName = "";

    public FightItem() : this(FightItemKind.Unset)
    {
    }

    public FightItem(FightItemKind kind)
    {
      this.kind = kind;
      Name = "Stone";
      PrimaryStatDescription = "Stone, can make a harm if thrown by a skilled man.";
    }

    public FightItemKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;
        if (kind == FightItemKind.Stone)
          tag1 = "stone";
      }
    }

    public bool RequiresEnemyOnCast
    {
      get
      {
        return kind == FightItemKind.Knife || kind == FightItemKind.ExplodePotion;
      }
    }

    public bool RequiresEmptyCellOnCast
    {
      get
      {
        return kind == FightItemKind.Trap;
      }
    }

    public virtual void PlayStartSound()
    {

    }

    public virtual void PlayEndSound()
    {

    }

    public bool AlwaysCausesEffect { get; set; }

    public PassiveAbility GetAbility()
    {
      //var hero = Caller as Hero;
      //return hero.GetAbility(abilityKind);
      return null;
    }

    public float GetFactor(bool primary)
    {
      return GetFactor(primary, GetAbility().Level);
    }

    public float GetFactor(bool primary, int abilityLevel = -1)
    {
      var ab = GetAbility();
      if (abilityLevel < 0)
        abilityLevel = ab.Level;
      var fac = ab.CalcFactor(primary, abilityLevel);
      if (primary)
        return fac;
      return GetAuxFactor(fac);
    }

    protected virtual float GetAuxFactor(float fac)
    {
      return AlwaysCausesEffect ? 100 : fac;
    }

    public float GetDamage()
    {
      var damage = baseDamage;
      damage += GetFactor(true);
      return damage;
    }

    public virtual float GetAuxValue(int abilityLevel)
    {
      return GetFactor(false, abilityLevel);
    }

    public virtual string GetStatDesc(bool primary, bool forAbility, int abilityLevel)
    {
      var name = primary ? primaryFactorName + ": " : auxFactorName + ": ";
      if (forAbility)
      {
        name += "+" + GetFactor(primary, abilityLevel);
        if (name.Contains("Chance"))//HACK
          name += " %";
      }
      else
        name += primary ? GetDamage().ToString() : GetAuxValue(abilityLevel) + (IsPercentage(primary) ? " %" : "");

      return name;
    }

    public virtual bool IsPercentage(bool primary)
    {

      return primary ? false : true;
    }


    protected bool hasAuxStat = true;
    public string[] GetExtraStatDescription(bool forAbility, int abilityLevel)
    {
      var desc = new List<string>();
      desc.Add(GetStatDesc(true, forAbility, abilityLevel));
      if (hasAuxStat)
        desc.Add(GetStatDesc(false, forAbility, abilityLevel));
      return desc.ToArray();
    }

    //[JsonIgnore]
    //public LivingEntity Caller//req. by interface
    //{
    //  get
    //  { 
    //    return null; 
    //  }
    //  set { }
    //}

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

  }
}
