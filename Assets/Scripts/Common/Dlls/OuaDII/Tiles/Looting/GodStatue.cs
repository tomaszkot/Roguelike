using Roguelike.Attributes;
using Roguelike.Tiles;
using System;

namespace OuaDII.Tiles
{
  public enum GodKind { Unset, Swarog, Perun, Dziewanna, Jarowit, Swiatowit, Wales }
}

namespace OuaDII.Tiles.Looting
{
  public class GodStatue : Roguelike.Tiles.Looting.Equipment
  {
    public event EventHandler<bool> PowerActivated;

    public GodKind godKind;
    public GodKind GodKind
    {
      get { return godKind; }
      set
      {
        godKind = value;
        var es = new EntityStats();
        switch (godKind)
        {
          case GodKind.Unset:
            break;
          case GodKind.Swarog:
            PrimaryStatDescription = "God of Sun, Fire and Smithing";//Let there be dark, Darkness surround us!
            tag1 = "Swarog";
            DisplayedName = "Swarog";
            es.SetFactor(EntityStatKind.FireAttack, 5);
            es.SetFactor(EntityStatKind.ResistFire, 10);
            es.SetFactor(EntityStatKind.Defense, 10);
            AttackText = "Let there be dark!";//Darkness surround us!
            break;
          case GodKind.Perun:
            PrimaryStatDescription = "God of Lightning";
            tag1 = "Perun";
            DisplayedName = "Perun";

            AttackText = "Discover power of thunders!";
            es.SetFactor(EntityStatKind.LightingAttack, 5);
            es.SetFactor(EntityStatKind.ResistLighting, 10);

            break;
          case GodKind.Dziewanna:
            PrimaryStatDescription = "God of Nature";
            tag1 = "Dziewanna";
            DisplayedName = "Dziewanna";
            es.SetFactor(EntityStatKind.LifeStealing, 4);
            es.SetFactor(EntityStatKind.ManaStealing, 4);
            AttackText = "Taste my juicy fruits!";
            break;
          case GodKind.Jarowit:
            PrimaryStatDescription = "God of War";
            tag1 = "Jarowit";
            DisplayedName = "Jarowit";

            es.SetFactor(EntityStatKind.ChanceToStrikeBack, 10);
            es.SetFactor(EntityStatKind.ChanceToBulkAttack, 10);
            es.SetFactor(EntityStatKind.Strength, 5);
            AttackText = "It's time of the Chosen One!";
            break;
          case GodKind.Swiatowit:
            PrimaryStatDescription = "Highest God";
            tag1 = "Swiatowit";
            DisplayedName = "Swiatowit";

            es.SetFactor(EntityStatKind.ChanceToEvadeElementalProjectileAttack, 10);
            es.SetFactor(EntityStatKind.ChanceToEvadeMeleeAttack, 10);
            es.SetFactor(EntityStatKind.Health, 10);
            es.SetFactor(EntityStatKind.Magic, 10);
            AttackText = "Obey my will!";
            break;
          case GodKind.Wales:
            //summon golem, //summon weeds trap
            PrimaryStatDescription = "God of earth, water, and the underworld";
            tag1 = "Wales";
            DisplayedName = "Wales";

            es.SetFactor(EntityStatKind.Defense, 5);
            es.SetFactor(EntityStatKind.Health, 5);
            es.SetFactor(EntityStatKind.Strength, 5);
            es.SetFactor(EntityStatKind.Magic, 5);
            es.SetFactor(EntityStatKind.Dexterity, 5);
            AttackText = "";
            break;
          default:
            break;
        }

        //es.SetFactor(EntityStatKind.ResistLighting, 10);
        MakeEnchantable(1);
        SetClass(EquipmentClass.Unique, 1, es);
        Identify();
        //IsIdentified = true;
        RequiredLevel = -1;
        //RequiredStats.GetStats().Clear();
      }
    }

    public GodStatue() //: base('%')
    {
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif

      EquipmentKind = EquipmentKind.God;
      Price = -1;
      RequiredStats.GetStats().Clear();
    }
        
    public bool PowerActive { get; private set; }

    public const int PowerCoolDownCounterSteps = 5;
    public int PowerCoolDownCounter { get; set; } = PowerCoolDownCounterSteps;
    public string AttackText { get; set; }

    public void SetPowerActive(bool ac)
    {
      PowerActive = ac;
      if (PowerActivated!=null)
        PowerActivated(this, PowerActive);
    }
  }
}
