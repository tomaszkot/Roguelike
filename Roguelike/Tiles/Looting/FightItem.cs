using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Projectiles;
using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum FightItemKind
  {
    Unset = 0,
    ExplosiveCocktail = 5, //ExplodePotion,
    ThrowingKnife = 10,
    HunterTrap = 15,
    Stone = 20,

    PlainArrow = 50,
    PlainBolt = 55,

    PoisonCocktail = 70,
    WeightedNet = 75
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
    public float baseDamage = Roguelike.LootFactories.Props.FightItemBaseDamage;
    public int Duration { get; set; }

    protected string primaryFactorName = "Damage";
    protected string auxFactorName = "";
    public string HitTargetSound;
    public string DeactivationSound;

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

    public int DeactivatedCount { get; set; }
    public void SetState(FightItemState state)
    {
      if (state == FightItemState.Deactivated)
        DeactivatedCount++;
      FightItemState = state;
      SendStateChanged();
    }

    private void SendStateChanged()
    {
      StateChanged?.Invoke(this, FightItemState);
    }

    public bool IsBowLikeAmmo
    {
      get
      {
        return fightItemKind == FightItemKind.PlainArrow || fightItemKind == FightItemKind.PlainBolt;
      }
    }

    public virtual FightItemKind FightItemKind
    {
      get { return fightItemKind; }
      set
      {
        fightItemKind = value;
        Name = fightItemKind.ToDescription();
        tag1 = fightItemKind.ToString();


        if (fightItemKind == FightItemKind.Stone)
        {
          PrimaryStatDescription = "Stone, can cause harm if thrown by a skilled man.";
          HitTargetSound = "punch";
        }
        else if (fightItemKind == FightItemKind.ThrowingKnife)
        {
          PrimaryStatDescription = Name + ", very sharp, likely to cause bleeding";
          baseDamage += 2;//total damage will be lower than from bow as bows adds to damage 
          Price *= 2;
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.PlainArrow)
        {
          baseDamage += 1;
          Price *= 2;
          PrimaryStatDescription = Name + ", basic ammo for a bow";
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.PlainBolt)
        {
          baseDamage += 2;
          Price *= 2;
          PrimaryStatDescription = Name + ", basic ammo for a crossbow";
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.HunterTrap)
        {
          baseDamage += 2;
          Price *= 4;
          PrimaryStatDescription = Name + ", clinch victim and causes bleeding";
          HitTargetSound = "trap";
          Duration = 3;
          DeactivationSound = "trap_off";
        }
        else if (fightItemKind == FightItemKind.ExplosiveCocktail)
        {
          baseDamage += 1;
          Price *= 3;
          PrimaryStatDescription = Name + ", explodes hurting the victim and nearby entities with fire";
          HitTargetSound = "SHATTER_Glass1";
          Duration = 4;
        }
        else if (fightItemKind == FightItemKind.PoisonCocktail)
        {
          baseDamage += 1;
          Price *= 3;
          PrimaryStatDescription = Name + ", explodes spreading a poison on the victim and nearby entities";
          HitTargetSound = "SHATTER_Glass1";
          Duration = 4;
        }
        else if (fightItemKind == FightItemKind.WeightedNet)
        {
          baseDamage += 1;
          Price *= 3;
          PrimaryStatDescription = Name + ", prevents victim from moving";
          HitTargetSound = "cloth";
          Duration = 4;
        }

      }
    }
    public bool RequiresEnemyOnCast
    {
      get
      {
        return fightItemKind == FightItemKind.ThrowingKnife || 
               fightItemKind == FightItemKind.ExplosiveCocktail ||
               fightItemKind == FightItemKind.PoisonCocktail ||
               fightItemKind == FightItemKind.Stone ||
               fightItemKind == FightItemKind.PlainArrow ||
               fightItemKind == FightItemKind.PlainBolt;
      }
    }

    List<IDestroyable> hits;
    public int RegisterHit(IDestroyable dest)
    {
      if (hits == null)
        hits = new List<IDestroyable>();
      if (!hits.Contains(dest))
        hits.Add(dest);
      return hits.Count;
    }

    public bool WasHit(IDestroyable dest)
    {
      if (hits == null)
        return false;
      return hits.Contains(dest);
    }

    public int Hits 
    {
      get 
      {
        if (hits == null)
          return 0;
        return hits.Count; 
      }
      
    }

    static Dictionary<FightItemKind, AbilityKind> fi2Ab = new Dictionary<FightItemKind, AbilityKind>()
    {
      { FightItemKind.ThrowingKnife, AbilityKind.ThrowingKnife},
      { FightItemKind.Stone, AbilityKind.ThrowingStone},
      { FightItemKind.PoisonCocktail, AbilityKind.PoisonCocktail},
      { FightItemKind.ExplosiveCocktail, AbilityKind.ExplosiveCocktail},
      { FightItemKind.HunterTrap, AbilityKind.HunterTrap},
      { FightItemKind.WeightedNet, AbilityKind.WeightedNet},
    };

    public static AbilityKind GetAbilityKind(FightItem item)
    {
      return GetAbilityKind(item.FightItemKind);
    }

    public static AbilityKind GetAbilityKind(FightItemKind fik)
    {
      if (fi2Ab.ContainsKey(fik))
      {
        //var ab = ale.GetActiveAbility(fi2Ab[item.FightItemKind]);
        //if (ab != null)
        return fi2Ab[fik];
      }

      return AbilityKind.Unset;
    }

    public AbilityKind AbilityKind
    {
      get
      {
        return GetAbilityKind(this);
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
      var hero = Caller as AdvancedLivingEntity;
      if(hero != null)
        return hero.GetActiveAbility(AbilityKind);
      return null;
    }

    public float GetFactor(bool primary)
    {
      var ab = GetAbility();
      if (ab == null)
        return 0;
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
        var esk = EntityStatKind.Unset;
        var lsk = LootStatKind.Weapon;
        var preffix = "";
        if (FightItemKind == FightItemKind.ExplosiveCocktail)
        {
          esk = EntityStatKind.FireAttack;
          lsk = LootStatKind.Unset;
          preffix = "Fire ";
        }
        else if (FightItemKind == FightItemKind.PoisonCocktail)
        {
          esk = EntityStatKind.PoisonAttack;
          lsk = LootStatKind.Unset;
          preffix = "Poison ";
        }

        var lsi = new LootStatInfo()
        {
          EntityStatKind = esk,
          Kind = lsk,
          Desc = preffix +"Damage: " + Damage + " " + FormatTurns(this.Duration) 
        };

        if (FightItemKind == FightItemKind.WeightedNet)
        {
          var ab = GetAbility();
          lsi.Desc = "Duaration: "+ ab.PrimaryStat.Factor;
        }

        res.Add(lsi);
        
      }
      return res;
    }
  }

  
}
