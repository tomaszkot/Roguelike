using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Tiles.Looting
{
  public abstract class FightItem : StackedLoot
  {
    private FightItemKind kind;
    public float baseDamage = 2.0f;

    protected PassiveAbilityKind abilityKind;
    protected string primaryFactorName = "Damage";
    protected string auxFactorName = "";

    public FightItem()
    {
    }

    public FightItemKind Kind { get => kind; set => kind = value; }
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

    //public override string[] GetExtraStatDescription()
    //{
    //  return GetExtraStatDescription(false, GetAbility().Level);
    //}

    [JsonIgnore]
    public LivingEntity Caller//req. by interface
    {
      //get { return GameManager.Instance.Hero; }
      get
      { return null; }
      set { }
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

  }
}
