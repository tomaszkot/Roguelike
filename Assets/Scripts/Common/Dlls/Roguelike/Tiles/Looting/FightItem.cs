using Dungeons.Core;
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
    ThrowingTorch = 25,

    PlainArrow = 50,
    IronArrow = 51,//Iron Arrowhead
    SteelArrow = 52,//Steel Arrowhead

    PlainBolt = 55,
    IronBolt = 56,//Iron Arrowhead
    SteelBolt = 57,//Steel Arrowhead

    PoisonArrow = 60,
    IceArrow = 61,
    FireArrow = 62,

    PoisonBolt = 66,
    IceBolt = 67,
    FireBolt = 68,

    PoisonCocktail = 70,
    WeightedNet = 75,

    CannonBall = 80,

    Smoke = 85,


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

    [JsonIgnore]
    public float baseDamage = Roguelike.LootFactories.Props.FightItemBaseDamage;

    [JsonIgnore]
    public int Duration { get; set; }

    [JsonIgnore]
    protected string primaryFactorName = "Damage";

    [JsonIgnore]
    protected string auxFactorName = "";

    [JsonIgnore]
    public string HitTargetSound = "";

    [JsonIgnore]
    public string DeactivationSound;

    [JsonIgnore]
    public bool RangeBasedCasting { get; set; } = true;

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
      Count = 1;
    }

    //can not be used as torch amount is calculated  see GetStackedCountForHotBar
    //public bool CanBeUsedDueToCount 
    //{
    //  get 
    //  {
    //    return Count > 0 || EndlessAmmo;
    //  }
    //}

    public bool EndlessAmmo { get; set; } = false;
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
        return fightItemKind.IsBowLikeAmmunition();
      }
    }
    public const int BaseCannonBallDamage = 60;

    string GetDescForBowLike(string suffix)
    {
      return Name + suffix;
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
          Price = 2;
          Count = (int)RandHelper.GetRandomFloatInRange(8, 10);
        }
        else if (fightItemKind == FightItemKind.CannonBall)
        {
          PrimaryStatDescription = "Cannon ball, can cause a sagnificant damage.";
          HitTargetSound = "punch";
          baseDamage = BaseCannonBallDamage;
          Price *= 5;
        }
        else if (fightItemKind == FightItemKind.Smoke)
        {
          PrimaryStatDescription = "Dense smoke making attackers confused.";
          baseDamage = 0;
          EndlessAmmo = true;
          RangeBasedCasting = false;
        }
        else if (fightItemKind == FightItemKind.ThrowingTorch)
        {
          PrimaryStatDescription = "Torch will most likely cause burning.";
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
          baseDamage += 2;
          Price *= 2;
          PrimaryStatDescription = GetDescForBowLike(", basic ammo for a bow");
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.IronArrow)
        {
          baseDamage += 5;
          Price *= 3;
          PrimaryStatDescription = GetDescForBowLike(", ammo with an iron head for a bow");
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.SteelArrow)
        {
          baseDamage += 10;
          Price *= 4;
          PrimaryStatDescription = GetDescForBowLike(", ammo with a steel head for a bow");
          HitTargetSound = "arrow_hit_body";
        }
        
        else if (
                 fightItemKind == FightItemKind.PoisonArrow ||
                 fightItemKind == FightItemKind.IceArrow ||
                 fightItemKind == FightItemKind.FireArrow ||
                 fightItemKind == FightItemKind.PoisonBolt ||
                 fightItemKind == FightItemKind.IceBolt ||
                 fightItemKind == FightItemKind.FireBolt
                 )
        {
          baseDamage += 5;
          Price *= 5;
          var head = fightItemKind.ToDescription();
          var arrowLikeKind = fightItemKind.ToDescription();

          var kind = head.Replace("Arrow", "").Replace("Bolt", "");
          if (kind == "Poison")
            kind = "poisonous";
          else if (kind == "Ice")
            kind = "freezing";
          else if (kind == "Fire")
            kind = "flaming";

          PrimaryStatDescription = GetDescForBowLike(", ammo with a " 
            + kind + 
            " head for a "+ (arrowLikeKind.Contains("Arrow") ? "bow" : "crossbow" ));
          HitTargetSound = "arrow_hit_body";
        }

        else if (fightItemKind == FightItemKind.PlainBolt)
        {
          baseDamage += 3;
          Price *= 2;
          PrimaryStatDescription = Name + ", basic ammo for a crossbow";
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.IronBolt)
        {
          baseDamage += 6;
          Price *= 3;
          PrimaryStatDescription = Name + ", ammo with an iron head for a crossbow";
          HitTargetSound = "arrow_hit_body";
        }
        else if (fightItemKind == FightItemKind.SteelBolt)
        {
          baseDamage += 12;
          Price *= 4;
          PrimaryStatDescription = Name + ", ammo with a steel head for a crossbow";
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
               fightItemKind == FightItemKind.CannonBall ||
               fightItemKind.IsBowLikeAmmunition() ||
               fightItemKind == FightItemKind.ThrowingTorch;
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
      { FightItemKind.CannonBall, AbilityKind.Cannon},
      { FightItemKind.Stone, AbilityKind.ThrowingStone},
      { FightItemKind.PoisonCocktail, AbilityKind.PoisonCocktail},
      { FightItemKind.ExplosiveCocktail, AbilityKind.ExplosiveCocktail},
      { FightItemKind.HunterTrap, AbilityKind.HunterTrap},
      { FightItemKind.WeightedNet, AbilityKind.WeightedNet},
      { FightItemKind.ThrowingTorch, AbilityKind.ThrowingTorch},
      { FightItemKind.Smoke, AbilityKind.Smoke},
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

    float GetFactor(bool primary)
    {
      var ab = GetAbility();
      if (ab == null)
        return 0;
      return GetFactor(primary, ab.Level);
    }

    float GetFactor(bool primary, int abilityLevel = -1)
    {
      var ab = GetAbility();
      if (abilityLevel < 0)
        abilityLevel = ab.Level;
      var fac = ab.CalcFactor(primary ? 0 : 1, abilityLevel);
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
        var fac = GetFactor(true);
        damage = Calculated.FactorCalculator.AddFactor(damage, fac);
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
          lsi.Desc = "Duration: "+ ((Duration -1) + ab.PrimaryStat.Factor);
        }

        res.Add(lsi);
        
      }
      return res;
    }

    public override bool IsMatchingRecipe(RecipeKind kind)
    {
      if (base.IsMatchingRecipe(kind))
        return true;
      if ((kind == RecipeKind.Arrows ||kind == RecipeKind.Bolts) && FightItemKind == FightItemKind.Stone)
        return true;

      return false;
    }
  }

  
}
