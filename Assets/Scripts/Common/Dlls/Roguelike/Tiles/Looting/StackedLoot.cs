﻿using System;

namespace Roguelike.Tiles.Looting
{
  public abstract class StackedLoot : Loot
  {
    private int count = 1;

    public int Count
    {
      get => count;
      set => count = value;
    }

    public virtual string GetId()
    {
      return GetType().ToString();
    }

    public StackedLoot()
    {

    }

    public StackedLoot Clone(int count)
    {
      var dest = this.MemberwiseClone() as StackedLoot;
      dest.Id = Guid.NewGuid();
      dest.Count = count;
      return dest;
    }

    public override string ToString()
    {
      return base.ToString() + " Count: "+Count;
    }
  }
}
