using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Projectiles;
using Roguelike.Attributes;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

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
    public float baseDamage = 5.0f;

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
      var ab = GetAbility();
      if (ab == null)
        return 1;
      return GetFactor(primary, ab.Level);
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

    public float Damage
    {
      get
      {
        var damage = baseDamage;
        damage += GetFactor(true);
        return damage;
      }
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
        name += primary ? Damage.ToString() : GetAuxValue(abilityLevel) + (IsPercentage(primary) ? " %" : "");

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
        

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

    public override List<LootStatInfo> GetLootStatInfo(LivingEntity caller)
    {
      var add = m_lootStatInfo == null || !m_lootStatInfo.Any();
      var res = base.GetLootStatInfo(caller);
      if (add)
      {
        if (Kind == FightItemKind.Stone)
        {
          var lsi = new LootStatInfo()
          {
            EntityStatKind = EntityStatKind.Unset,
            Kind = LootStatKind.Weapon,
            Desc = "Damage: "+Damage
          };

          res.Add(lsi);
        }
      }
      return res;
    }
  }

  public class ProjectileFightItem : FightItem, IProjectile
  {
    public ProjectileFightItem() : this(FightItemKind.Unset, null)
    {
    }

    public ProjectileFightItem(FightItemKind kind, LivingEntity caller) : base(kind)
    {
      Caller = caller;
    }

    public IObstacle Target { get; set; }

    [JsonIgnore]
    public LivingEntity Caller//req. by interface
    {
      get;
      set;
    }
  }
}
