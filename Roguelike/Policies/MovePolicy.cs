using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Policies
{
  public class MovePolicy : Policy
  {
    LivingEntity entity;
    
    AbstractGameLevel level;
    Point newPos;

    public Point NewPos { get => newPos; set => newPos = value; }
    public AbstractGameLevel Level { get => level; set => level = value; }
    public LivingEntity Entity { get => entity; set => entity = value; }

    public MovePolicy()
    {

    }

    public bool Apply(AbstractGameLevel level, LivingEntity entity, Point newPos)
    {
      if (newPos.X >= level.Width)
        return false;
      if (newPos.X < 0)
        return false;

      if (newPos.Y >= level.Height)
        return false;
      if (newPos.Y < 0)
        return false;

      this.Entity = entity;
      this.NewPos = newPos;
      this.Level = level;
      entity.State = EntityState.Moving;

      if (level.SetTile(entity, newPos))
      {
        ReportApplied(entity);
        return true;
      }
      else
        entity.State = EntityState.Idle;

      return false;
    }

  }
}
