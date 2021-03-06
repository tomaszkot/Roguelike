using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Calculated
{
  public class AttackDescription
  {
    public float Nominal { get; set; }//strength
    public float Current { get; set; }//strength + weapon melee damage
    public float CurrentPhysical { get; set; }//Current + Extra form abilities
    Dictionary<EntityStatKind, float> NonPhysical { get; set; }//weapon  ice, fire... damages
    public float CurrentTotal { get; set; }//Current + NonPhysical

    public string Display { get; set; }

    public AttackDescription(LivingEntity ent)
    {
      Current = ent.GetCurrentValue(EntityStatKind.Attack);

      CurrentPhysical = Current;

      if (ent is AdvancedLivingEntity ale)
      {
        var wpn = ale.GetActiveEquipment()[CurrentEquipmentKind.Weapon] as Weapon;
        if (wpn != null)
        {
          if (AdvancedLivingEntity.Weapons2Esk.ContainsKey(wpn.Kind))
          {
            var extraPercentage = ent.Stats.GetCurrentValue(AdvancedLivingEntity.Weapons2Esk[wpn.Kind]);
            CurrentPhysical = FactorCalculator.CalcFactor(CurrentPhysical, extraPercentage);
          }
          //if (AdvancedLivingEntity.Weapons2Esk.ContainsKey(wpn.Kind))
          {
            //  var esk = AdvancedLivingEntity.Weapons2Esk[wpn.Kind];
            //  var ab = ale.Abilities.GetByEntityStatKind(esk, false);
            //  if (ab != null)
            //  {
            //    var extra = CurrentPhysical * ab.AuxStat.Factor / 100f;
            //    CurrentPhysical += extra;
            //  }
          }
        }
      }

      CurrentTotal = CurrentPhysical;

      NonPhysical = ent.GetNonPhysicalDamages();
      foreach (var npd in NonPhysical)
        CurrentTotal += npd.Value;

      Nominal = ent.Stats.GetStat(EntityStatKind.Attack).Value.Nominal;

      Display = Nominal + "/" + CurrentTotal;
    }
  }
}
