using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Diagnostics;

namespace Roguelike.Tiles.Looting
{
  public class WeaponSpellSource : SpellSource
  {
    public int Level { get { return this.Weapon.LevelIndex; } }
    int initChargesCount = 0;
    public int RestoresCount { get; set; }

    public WeaponSpellSource(Weapon weapon, SpellKind kind, int chargesCount = 15) : base(kind)
    {
      this.Weapon = weapon;
      InitChargesCount = chargesCount;
    }

    public void Restore()
    {
      RestoresCount++;
      RestoredChargesCount = initChargesCount - 2 * RestoresCount;
      Count = RestoredChargesCount;
    }

    public int InitChargesCount 
    { 
      get => initChargesCount; 
      set
      {
        initChargesCount = value;
        Count = value;
        RestoredChargesCount = Count;
      }
    }
    public int RestoredChargesCount { get; set; }
    
  }

  public class SpellSource : StackedLoot
  {
    protected Weapon Weapon { get; set; }

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
          break;
        case SpellKind.CrackedStone:
          desc = "Can use to block the path has limited durability";
          break;
        case SpellKind.Skeleton:
          desc = "Creates a skeleton which fights as hero ally";
          break;
        case SpellKind.Trap:
          desc = "Inflicts physical damage blocks victim for a few turns";
          break;
        case SpellKind.IceBall:
          desc = "Inflicts cold damage, can be decreased by a related resist";
          break;
        case SpellKind.PoisonBall:
          desc = "Inflicts poison damage, can be decreased by a related resist";
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
          break;
        case SpellKind.StonedBall:
          desc = "";
          break;
        case SpellKind.LightingBall:
          desc = "Inflicts lighting damage, can be decreased by a related resist";
          break;
        case SpellKind.Mana:
          desc = "Restores some mana by sacrificing some health";
          break;
        case SpellKind.BushTrap:
          desc = "";
          break;
        case SpellKind.Rage:
          desc = "Increases the Damage statistic of the caster";
          break;
        case SpellKind.Weaken:
          desc = "Reduces the Defense statistic of the victim";
          break;
        case SpellKind.NESWFireBall:
          desc = "Inflicts fire damage at Cardinal Directions";
          break;
        case SpellKind.Teleport:
          desc = "Teleports hero to a chosen point";
          break;
        case SpellKind.IronSkin:
          desc = "Increases the Defense statistic of the caster";
          break;
        case SpellKind.ResistAll:
          desc = "";
          break;
        case SpellKind.Inaccuracy:
          desc = "Reduces the Chance to Hit statistic of the victim";
          break;
        //case SpellKind.CallMerchant:
        //  desc = "Teleports a merchant near to the hero";
        //  break;
        //case SpellKind.CallGod:
        //  desc = "Teleports a god near to the hero";
        //  break;
        case SpellKind.Identify:
          desc = "Reveals attributes of a magic/unique item";
          break;
        case SpellKind.Portal:
          desc = "Allows to teleport to a known point of the world";
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

    //public ISpell CreateSpell(LivingEntity caller, Weapon weapon = null)
    //{
    //  return CreateSpell(Kind, caller, weapon);
    //}

    public T CreateSpell<T>(LivingEntity caller) where T : class, ISpell
    {
      var ispell = CreateSpell( caller);
      return ispell as T;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }
        
    public ISpell CreateSpell(LivingEntity caller)
    {
      var weapon = Weapon;
      switch (this.Kind)
      {
        case SpellKind.FireBall:
          return new FireBallSpell(caller, weapon);
        case SpellKind.PoisonBall:
          return new PoisonBallSpell(caller, weapon);
        case SpellKind.IceBall:
          return new IceBallSpell(caller, weapon);
        case SpellKind.Skeleton:
          return new SkeletonSpell(caller);
        case SpellKind.Transform:
          return new TransformSpell(caller);
        case SpellKind.ManaShield:
          return new ManaShieldSpell(caller);
        case SpellKind.Rage:
          return new RageSpell(caller);
        case SpellKind.Weaken:
          return new WeakenSpell(caller);
        case SpellKind.Inaccuracy:
          return new InaccuracySpell(caller);
        case SpellKind.IronSkin:
          return new IronSkinSpell(caller);
        case SpellKind.Teleport:
          return new TeleportSpell(caller);
        case SpellKind.Portal:
          return new Portal(caller);
        case SpellKind.ResistAll:
          return new ResistAllSpell(caller);
        default:
          break;
          throw new Exception("CreateSpell ???" + Kind);
      }
      return null;
    }

    public override string[] GetExtraStatDescription()
    {
      Debug.Assert(false, "call the one with (LivingEntity caller)");
      return base.GetExtraStatDescription();
    }

    public string GetExtraStatDescriptionFormatted(LivingEntity caller)
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

    public SpellStatsDescription GetExtraStatDescription(LivingEntity caller, bool currentLevel)
    {
      ISpell spell = CreateSpell(caller);
      if (spell == null)
        return null;
      var spellStatsDescription = spell.CreateSpellStatsDescription(currentLevel);

      return spellStatsDescription;
    }
  }
}
