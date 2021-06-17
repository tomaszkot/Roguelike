using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;

namespace Roguelike.Tiles.Looting
{
  public class SpellSource : StackedLoot
  {
    public SpellSource()
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
        //spell = CreateSpell(dummy);
        if (kind == SpellKind.CrackedStone)
          Price = (int)((float)Price / 2.0f);
        else if (kind == SpellKind.Identify)
          Price *= 2;

        SetDesc();
      }
    }

    public  string GetNameFromKind()
    {
      return kind + " " +  GetType().Name;
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

    //public float ManaCost
    //{
    //  //get { return spell.ManaCost; }
    //  get { return 5; }
    //}

    //public void UpdateLevel(LivingEntity le)
    //{
    //  spell = CreateSpell(le);
    //}

    //public bool IsDefensive { get { return spell is DefensiveSpell; } }

    //public string[] GetFeatures(LivingEntity caller)
    //{
    //  spell = CreateSpell(caller);
    //  var feat = spell.GetFeatures();
    //  return feat;
    //}

    public ISpell CreateSpell(LivingEntity caller)
    {
      return CreateSpell(Kind, caller);
    }

    public T CreateSpell<T>(LivingEntity caller) where T : class, ISpell
    {
      var ispell = CreateSpell(Kind, caller);
      return ispell as T;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

    public static ISpell CreateSpell(SpellKind Kind, LivingEntity caller)
    {
      switch (Kind)
      {
        case SpellKind.FireBall:
          return new FireBallSpell(caller);
        //  case SpellKind.NESWFireBall:
        //    return new NESWFireBallSpell(caller);
        //  case SpellKind.CrackedStone:
        //    return new CrackedStoneSpell(caller);
        //  case SpellKind.Trap:
        //    return new TrapSpell(caller);
        case SpellKind.PoisonBall:
          return new PoisonBallSpell(caller);
        case SpellKind.IceBall:
          return new IceBallSpell(caller);
        case SpellKind.Skeleton:
          return new SkeletonSpell(caller);
        case SpellKind.Transform:
          return new TransformSpell(caller);
        //  case SpellKind.Frighten:
        //    return new FrightenSpell(caller);
        //  case SpellKind.Healing:
        //    return new HealingSpell(caller);
        case SpellKind.ManaShield:
          return new ManaShieldSpell(caller);
        //case SpellKind.Telekinesis:
        //  return new TelekinesisSpell(caller);
        //  case SpellKind.StonedBall:
        //    return new StonedBallSpell(caller);
        //  //case SpellKind.MindControl:
        //  //	return new MindControlSpell(caller);
        //  case SpellKind.Mana:
        //    return new ManaSpell(caller);
        //  case SpellKind.BushTrap:
        //    return new BushTrapSpell(caller);
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
        //  case SpellKind.CallMerchant:
        //    return new CallMerchantSpell(caller);
        //  case SpellKind.CallGod:
        //    return new CallGodSpell(caller);
        //  case SpellKind.LightingBall:
        //    return new LightingBallSpell(caller);
        case SpellKind.ResistAll:
          return new ResistAllSpell(caller);
        default:
          break;
          throw new Exception("CreateSpell ???" + Kind);
      }
      return null;
    }
  }
  
  
  public class Scroll : SpellSource
  {
    //public bool EnemyRequired { get { return spell.EnemyRequired; } }
    //public bool EntityRequired { get { return spell.EntityRequired; } }
    //Spell spell;

    //public int Level
    //{
    //  get { return spell.GetCurrentLevel(); }
    //}
        

    public Scroll() : this(SpellKind.Unset)
    {

    }

    public Scroll(SpellKind kind = SpellKind.Unset) : base()
    {
      //dummy.Stats.SetNominal(EntityStatKind.Magic, LivingEntity.BaseMagic.TotalValue);
   
      LootKind = LootKind.Scroll;
    }

    public static SpellKind DiscoverKindFromName(string name)//->name fire_ball -> FireBall
    {
      return DiscoverKindFromName(name, false);
    }

    

  }
}
