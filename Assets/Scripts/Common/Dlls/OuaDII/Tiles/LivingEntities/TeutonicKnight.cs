using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OuaDII.Tiles.LivingEntities
{
  public class TeutonicKnight : LivingEntity
  {
    public TeutonicKnight(Container cont) : base(Point.Empty, '!', cont)
    {
      Name = "Teutonic Knight";
    }
  }
}
