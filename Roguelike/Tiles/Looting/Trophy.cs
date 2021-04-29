

using Roguelike.Attributes;

namespace Roguelike.Tiles.Looting
{
  public enum TrophyKind
  {
    Unset, BatHead, RatHead, DragonClaw, DarkWizardHand, FallenOneHead, GriffinHead, Scorpion,
    SkeletonHead, SpiderHead, VampireHead, WolfHead, HydraHead, SkeletonChemp
  }

  public class Trophy : Equipment
  {
    public TrophyKind Kind { get; set; }

    public Trophy() : base(EquipmentKind.Trophy)
    {
      Symbol = '&';
#if ASCII_BUILD
      color = GoldColor;
#endif
      Price = 10;
      Class = EquipmentClass.Unique;
      primaryStatDescription = "Rare trophy";
    }

    public void SetKind(TrophyKind kind)
    {
      Kind = kind;
      Name = Kind.ToString();

      var st = new EntityStat();
      bool makeEnchantable = false;
      switch (Kind)
      {
        case TrophyKind.Unset:
          break;
        case TrophyKind.SkeletonChemp:
          tag1 = "skeleton_chemp_trophy";
          Name = "Head of Skeleton";
          primaryStatDescription = "Trophy";
          Class = EquipmentClass.Magic;
          break;
        case TrophyKind.BatHead:
          tag1 = "bat_boss_trophy";
          Name = "Head of Bat's Boss";
          st = new EntityStat(EntityStatKind.Attack, 0);
          st.Factor = 3;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Defense, 0);
          st.Factor = 3;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Health, 0);
          st.Factor = 5;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.LifeStealing, 0);
          st.Factor = 3;
          SetMagicStat(st.Kind, st);

          break;
        case TrophyKind.RatHead:
          tag1 = "rat_boss_trophy";
          Name = "Head of Rat's Boss";

          st = new EntityStat(EntityStatKind.Magic, 0);
          st.Factor = 4;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Mana, 0);
          st.Factor = 4;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ManaStealing, 0);
          st.Factor = 4;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.LightPower, 0);
          st.Factor = 4;
          SetMagicStat(st.Kind, st);

          break;

        case TrophyKind.SkeletonHead://skeleton_king 3rd level
          tag1 = "sk_boss_head";
          Name = "Head of Skeleton's Boss";
          st = new EntityStat(EntityStatKind.ChanceToHit, 0);
          st.Factor = 5;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Health, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          //st = new EntityStat(EntityStatKind.MagicAttackDamageReduction, 0);
          //st.Factor = 5;
          //SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToEvadeMagicAttack, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          break;
        case TrophyKind.WolfHead://wolf_king
          tag1 = "wolf_boss_trophy";
          Name = "Head of Wolf's Boss";

          st = new EntityStat(EntityStatKind.Magic, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Mana, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToCastSpell, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          //st = new EntityStat(EntityStatKind.MeleeAttackDamageReduction, 0);
          //st.Factor = 10;
          //SetMagicStat(st.Kind, st);
          break;
        case TrophyKind.DragonClaw:
          tag1 = "dragon_claw";
          Name = "Dragon's Claw";
          st = new EntityStat(EntityStatKind.FireAttack, 0);
          st.Factor = 5;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ResistFire, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          makeEnchantable = true;
          break;
        case TrophyKind.Scorpion://scorpion_king , also dragon on 5th
          tag1 = "scorpion_boss_trophy";
          Name = "Stinger of Scorpion's Boss";

          st = new EntityStat(EntityStatKind.PoisonAttack, 0);
          st.Factor = 6;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ResistPoison, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.LifeStealing, 0);
          st.Factor = 5;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Health, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);
          break;
        case TrophyKind.SpiderHead://spider_king
          tag1 = "spider_boss_trophy";
          Name = "Head of Spider's Boss";

          st = new EntityStat(EntityStatKind.ChanceToCauseBleeding, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          //st = new EntityStat(EntityStatKind.MeleeAttackDamageReduction, 0);
          //st.Factor = 15;
          //SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToHit, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToEvadeMagicAttack, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);
          break;
        case TrophyKind.HydraHead://hydra_king
          tag1 = "hydra_boss_trophy";
          Name = "Head of Hydra's Boss";

          st = new EntityStat(EntityStatKind.ChanceToCauseStunning, 0);
          st.Factor = 5;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToHit, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Defense, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Health, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          break;
        case TrophyKind.GriffinHead://griffin_king
          tag1 = "griffin_boss_trophy";
          Name = "Head of Griffin's Boss";

          st = new EntityStat(EntityStatKind.ChanceToEvadeMeleeAttack, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ManaStealing, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Mana, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Magic, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);
          break;
        case TrophyKind.VampireHead://vampire_king
          tag1 = "wampire_boss_trophy";
          Name = "Head of Vampire's Boss";

          st = new EntityStat(EntityStatKind.LifeStealing, 0);
          st.Factor = 5;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToCauseBleeding, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToCastSpell, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToEvadeMagicAttack, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          break;
        case TrophyKind.DarkWizardHand://wizard_king
          tag1 = "dark_wizard_boss_trophy";
          Name = "Hand of Warlock's Boss";

          st = new EntityStat(EntityStatKind.Health, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          //st = new EntityStat(EntityStatKind.MagicAttackDamageReduction, 0);
          //st.Factor = 15;
          //SetMagicStat(st.Kind, st);

          //st = new EntityStat(EntityStatKind.MeleeAttackDamageReduction, 0);
          //st.Factor = 15;
          //SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToEvadeMagicAttack, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ChanceToEvadeMeleeAttack, 0);
          st.Factor = 15;
          SetMagicStat(st.Kind, st);

          break;
        case TrophyKind.FallenOneHead://fallen_one
          tag1 = "fallen_one_trophy";
          Name = "Head of Fallen One";

          st = new EntityStat(EntityStatKind.LifeStealing, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.ManaStealing, 0);
          st.Factor = 10;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.Defense, 0);
          st.Factor = 30;
          SetMagicStat(st.Kind, st);

          st = new EntityStat(EntityStatKind.LightPower, 0);
          st.Factor = -10;
          SetMagicStat(st.Kind, st);

          makeEnchantable = true;
          break;
        default:
          break;
      }

      if (makeEnchantable)
        MakeEnchantable();//it also increases price
    }
    string primaryStatDescription;
    public string GetPrimaryStatDescription()
    {
      return primaryStatDescription;
    }
  }
}
