using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Diagnostics;

namespace Roguelike.Tiles.Looting
{
  public abstract class SpellSource : StackedLoot
  {
    public virtual bool Enabled 
    {
      get { return Count > 0; }
    }

    public SpellSource(SpellKind kind)
    {
      Symbol = '?';
      Price = 20;
      Kind = kind;
      PositionInPage = -1;
    }

    string desc;
    public string GetDescription()
    {
      return desc;
    }

    SpellKind kind;
    public SpellKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;
        Name = GetNameFromKind();
        if (kind == SpellKind.CrackedStone)
          Price = (int)((float)Price / 2.0f);
        else if (kind == SpellKind.Identify)
          Price *= 2;

        SetDesc();
      }
    }

    public string GetNameFromKind()
    {
      return kind + " " + GetType().Name;
    }

    private void SetDesc()
    {
      switch (kind)
      {
        case SpellKind.FireBall:
          desc = "Inflicts fire damage, can be decreased by a related resist";
          TargetRequired = true;
          break;
        case SpellKind.CrackedStone:
          desc = "Can use to block the path has limited durability";
          TargetRequired = true;
          break;
        case SpellKind.Skeleton:
          desc = "Creates a skeleton which fights as hero ally";
          break;
        case SpellKind.Trap:
          desc = "Inflicts physical damage blocks victim for a few turns";
          TargetRequired = true;
          break;
        case SpellKind.IceBall:
          desc = "Inflicts cold damage, can be decreased by a related resist";
          TargetRequired = true;
          break;
        case SpellKind.PoisonBall:
          desc = "Inflicts poison damage, can be decreased by a related resist";
          TargetRequired = true;
          break;
        case SpellKind.Transform:
          desc = "Turns hero into a bat invisible for monsters";
          break;
        case SpellKind.Frighten:
          desc = "Monsters run away from hero for a couple of turns. ";
          break;
        case SpellKind.Healing:
          desc = "Restores some health";
          break;
        case SpellKind.ManaShield:
          desc = "Creates an aura protecting the hero";
          break;
        case SpellKind.Telekinesis:
          desc = "Allows to interact with entities from a distance";
          TargetRequired = true;
          break;
        case SpellKind.StonedBall:
          desc = "";
          TargetRequired = true;
          break;
        case SpellKind.LightingBall:
          desc = "Inflicts lighting damage, can be decreased by a related resist";
          TargetRequired = true;
          break;
        case SpellKind.Mana:
          desc = "Restores some mana by sacrificing some health";
          break;
        case SpellKind.BushTrap:
          desc = "";
          TargetRequired = true;
          break;
        case SpellKind.Rage:
          desc = "Increases the Damage statistic of the caster";
          break;
        case SpellKind.Weaken:
          desc = "Reduces the Defense statistic of the victim";
          TargetRequired = true;
          break;
        case SpellKind.NESWFireBall:
          desc = "Inflicts fire damage at Cardinal Directions";
          break;
        case SpellKind.Teleport:
          desc = "Teleports hero to a chosen point";
          TargetRequired = true;
          break;
        case SpellKind.IronSkin:
          desc = "Increases the Defense statistic of the caster";
          break;
        case SpellKind.ResistAll:
          desc = "";
          break;
        case SpellKind.Dziewanna:
          desc = "Creates a poisonous apple(s), irresisteble for many entities";
          break;
        case SpellKind.Inaccuracy:
          desc = "Reduces the Chance to Hit statistic of the victim";
          TargetRequired = true;
          break;
        //case SpellKind.CallMerchant:
        //  desc = "Teleports a merchant near to the hero";
        //  break;
        //case SpellKind.CallGod:
        //  desc = "Teleports a god near to the hero";
        //  break;
        case SpellKind.Identify:
          desc = "Reveals attributes of a magic/unique item";
          TargetRequired = true;
          break;
        case SpellKind.Portal:
          desc = "Allows to teleport to a known point of the world";
          TargetRequired = true;
          break;
        default:
          break;
      }

      PrimaryStatDescription = desc;
    }

    //->name fire_ball -> FireBall
    //->name fire_book -> FireBook
    public static SpellKind DiscoverKindFromName(string name, bool book)
    {
      name = name.Replace(book ? "_book" : "_scroll", "");
      name = name.Replace("_", "");
      return DiscoverKindFromName<SpellKind>(name);
    }

    public override string ToString()
    {
      var res = Name;
      return res + " " + base.ToString();
    }

    public T CreateSpell<T>(LivingEntity caller) where T : class, ISpell
    {
      var ispell = CreateSpell(caller);
     
      return ispell as T;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

    public bool TargetRequired
    {
      get;
      set;
    }

    public abstract ISpell CreateSpell();
    
    public virtual ISpell CreateSpell(LivingEntity caller)
    {
      ISpell spell = null;
      switch (this.Kind)
      {
        case SpellKind.FireBall:
          spell = new FireBallSpell(caller);
          break;
        case SpellKind.PoisonBall:
          spell = new PoisonBallSpell(caller);
          break;
        case SpellKind.IceBall:
          spell = new IceBallSpell(caller);
          break;
        case SpellKind.Skeleton:
          spell = new SkeletonSpell(caller, Roguelike.Generators.GenerationInfo.Difficulty);
          break;
        case SpellKind.Transform:
          spell = new TransformSpell(caller);
          break;
        case SpellKind.ManaShield:
          spell = new ManaShieldSpell(caller);
          break;
        case SpellKind.Rage:
          spell = new RageSpell(caller);
          break;
        case SpellKind.Weaken:
          spell = new WeakenSpell(caller);
          break;
        case SpellKind.Inaccuracy:
          spell = new InaccuracySpell(caller);
          break;
        case SpellKind.IronSkin:
          spell = new IronSkinSpell(caller);
          break;
        case SpellKind.Teleport:
          spell = new TeleportSpell(caller);
          break;
        case SpellKind.Portal:
          spell = new Portal(caller);
          break;
        case SpellKind.ResistAll:
          spell = new ResistAllSpell(caller);
          break;
        case SpellKind.Dziewanna:
          spell = new DziewannaSpell(caller);
          break;
        case SpellKind.Identify:
          break;//TODO ?
        default:
          Debug.Assert(false, "CreateSpell ???" + Kind);
          break;
          
      }
      if (spell is IProjectileSpell proj)
        proj.Range += spell.CurrentLevel - 1;
      return spell;
    }

    ISpell dummyForIsOffensive;
    [JsonIgnore]
    public bool IsOffensive
    {
      get
      {
        if (this.Kind == SpellKind.Skeleton)
          return true;//TODO, otherwise Container is needed

        if (dummyForIsOffensive == null)
          dummyForIsOffensive = CreateSpell(new LivingEntity());

        return dummyForIsOffensive is OffensiveSpell;
      }
    }
    public override string[] GetExtraStatDescription()
    {
      Debug.Assert(false, "call the one with (LivingEntity caller)");
      return base.GetExtraStatDescription();
    }

    public virtual string GetExtraStatDescriptionFormatted(LivingEntity caller)
    {
      var statDescCurrent = GetExtraStatDescription(caller, true);
      if (statDescCurrent == null)
        return "";
      var str = string.Join("\r\n", statDescCurrent.GetDescription(false));
      var res = "Current Level: " + statDescCurrent.Level + "\r\n" + str;

      var statDescNext = GetExtraStatDescription(caller, false);
      str = string.Join("\r\n", statDescNext.GetDescription(false));
      res += "\r\n\r\n";
      res += "Next Level (Magic Required: "+ statDescNext.MagicRequired+ "):" + "\r\n" + str;
      return res;
    }

    protected virtual ISpell GetSpell(LivingEntity caller)
    { 
      return CreateSpell(caller);
    }
    public virtual SpellStatsDescription GetExtraStatDescription(bool currentLevel)
    {
      throw new Exception("Call the one with caller");
    }

    public virtual SpellStatsDescription GetExtraStatDescription(LivingEntity caller, bool currentLevel)
    {
      var spell = GetSpell(caller);
      if (spell == null)
        return null;
      var spellStatsDescription = spell.CreateSpellStatsDescription(currentLevel);

      return spellStatsDescription;
    }
  }
}
