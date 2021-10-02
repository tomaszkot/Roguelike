using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Projectiles;
using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum RangedWeaponAmmoKind
  {
    Unset = 0,
    PlainArrow = 1,
    PlainBolt = 10
  }

  public enum FightItemKind
  {
    Unset,
    ExplosiveCocktail, //ExplodePotion,
    ThrowingKnife,
    HunterTrap,
    Stone
  }

  public enum FightItemState
  {
    Unset,
    Moving,
    Activated,
    Busy,
    Deactivated
  }

  public class FightItem : StackedLoot
  {
    private FightItemKind fightItemKind;
    public FightItemState FightItemState { get; set; }
    public float baseDamage = 5.0f;
    public int TurnLasting { get; set; }

    protected string primaryFactorName = "Damage";
    protected string auxFactorName = "";
    public string HitTargetSound;

    [JsonIgnore]
    public EventHandler<FightItemState> StateChanged { get; set; }

    [JsonIgnore]
    public LivingEntity Caller//req. by interface
    {
      get;
      set;
    }

    public FightItem() : this(FightItemKind.Unset)
    {
    }

    public FightItem(FightItemKind kind)
    {
      this.FightItemKind = kind;
      this.LootKind = LootKind.FightItem;
      
    }

    public void SetState(FightItemState state)
    {
      FightItemState = state;
      SendStateChanged();
    }

    private void SendStateChanged()
    {
      StateChanged?.Invoke(this, FightItemState);
    }

    //public void Deactivate()
    //{
    //  FightItemState = FightItemState.Deactivated;
    //  SendStateChanged();
    //}

    public FightItemKind FightItemKind
    {
      get { return fightItemKind; }
      set
      {
        fightItemKind = value;
        Name = fightItemKind.ToDescription();
        tag1 = fightItemKind.ToString();
        if (fightItemKind == FightItemKind.Stone)
        {
          PrimaryStatDescription = "Stone, can make a harm if thrown by a skilled man.";
          HitTargetSound = "punch";
        }
        else if (fightItemKind == FightItemKind.ThrowingKnife)
        {
          PrimaryStatDescription = Name+", very sharp, likely to cause bleeding";
          baseDamage += 3;
          Price *= 2;
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.ExplosiveCocktail)
        {
          baseDamage -= 1;
          Price *= 3;
          PrimaryStatDescription = Name + ", explodes hurting the victim and nearby entities with fire";
          HitTargetSound = "SHATTER_Glass1";
          TurnLasting = 3;
          //baseDamage += 2;
        }
        else if (fightItemKind == FightItemKind.HunterTrap)
        {
          baseDamage += 1;
          Price *= 2;
          PrimaryStatDescription = Name + ", clinch victim and causes bleeding";
          HitTargetSound = "trap";
          TurnLasting = 3;
        }
      }
    }

    public bool RequiresEnemyOnCast
    {
      get
      {
        return fightItemKind == FightItemKind.ThrowingKnife || 
               fightItemKind == FightItemKind.ExplosiveCocktail ||
               fightItemKind == FightItemKind.Stone;
      }
    }

    public AbilityKind AbilityKind
    {
      get
      {
        if (fightItemKind == FightItemKind.ThrowingKnife)
          return AbilityKind.ThrowingKnifeMastering;
        if (fightItemKind == FightItemKind.ExplosiveCocktail)
          return AbilityKind.ExplosiveMastering;
        if (fightItemKind == FightItemKind.Stone)
          return AbilityKind.ThrowingStoneMastering;
        if (fightItemKind == FightItemKind.HunterTrap)
          return AbilityKind.HunterTrapMastering;
        return AbilityKind.Unset;
      }
    }

    public bool RequiresEmptyCellOnCast
    {
      get
      {
        return fightItemKind == FightItemKind.HunterTrap;
      }
    }

    public virtual void PlayStartSound()
    {

    }

    public virtual void PlayEndSound()
    {

    }

    public bool AlwaysCausesEffect { get; set; }

    public ActiveAbility GetAbility()
    {
      var hero = Caller as Hero;
      if(hero != null)
        return hero.GetActiveAbility(AbilityKind);
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
      return base.GetId() + "_" + FightItemKind;
    }

    public override List<LootStatInfo> GetLootStatInfo(LivingEntity caller)
    {
      Caller = caller;//Damage needs it!
      var res = base.GetLootStatInfo(caller);
      var add = true;//buggy :  m_lootStatInfo == null || !m_lootStatInfo.Any();
      res.Clear();
      if (add)
      {
        //if (
        //  //FightItemKind == FightItemKind.Stone || 
        //  // FightItemKind == FightItemKind.ThrowingKnife ||
        //  // FightItemKind == FightItemKind.HunterTrap ||
        //  // FightItemKind == FightItemKind.ex
        //   )
        {
          var esk = EntityStatKind.Unset;
          var lsk = LootStatKind.Weapon;
          var preffix = "";
          if (FightItemKind == FightItemKind.ExplosiveCocktail)
          {
            esk = EntityStatKind.FireAttack;
            lsk = LootStatKind.Unset;
            preffix = "Fire ";
          }
          var lsi = new LootStatInfo()
          {
            EntityStatKind = esk,
            Kind = lsk,
            Desc = preffix +"Damage: " + Damage + " " + FormatTurns(this.TurnLasting) 
          };

          res.Add(lsi);
        }
      }
      return res;
    }
  }

  public class ProjectileFightItem : FightItem, IProjectile
  {
    public const int DefaultMaxDistance = 6;

    public ProjectileFightItem() : this(FightItemKind.Unset, null)
    {
    }
    
    public ProjectileFightItem(FightItemKind kind, LivingEntity caller = null) : base(kind)
    {
      Caller = caller;
    }

    [JsonIgnore]
    public Tile Target { get; set; }

    public override bool IsCollectable
    {
      get 
      { 
        if(this.FightItemKind != FightItemKind.HunterTrap)
          return true;

        return FightItemState == FightItemState.Deactivated ||
               FightItemState == FightItemState.Unset;
      }
    }

  }
}
