using Roguelike.LootContainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Effects
{
  public interface IEquipable
  {
    CurrentEquipment CurrentEquipment { get; set ; }
  }
}
