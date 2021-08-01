using System;

namespace Roguelike.Tiles.Looting
{
  public abstract class StackedLoot : Loot
  {
    private int count = 1;
    Guid guidNoOwner = new Guid("{36B5E3F3-28F2-494D-912E-6B33B6857986}");
    public Guid OwnerId { get; set; }
    public bool Cloned { get; set; }

    public int Count
    {
      get => count;
      set => count = value;
    }

    public virtual string GetUniqueName()
    {
      return GetType().ToString();
    }

    public StackedLoot()
    {
      OwnerId = Guid.NewGuid();//guidNoOwner
    }

    public StackedLoot Clone(int count, Guid ownerId)
    {
      var dest = this.MemberwiseClone() as StackedLoot;
      dest.Id = Guid.NewGuid();
      dest.Count = count;
      dest.OwnerId = ownerId;
      dest.Cloned = true;
      return dest;
    }

    public override int GetHashCode()
    {
      return GetHashCodeString().GetHashCode();
    }

    private string GetHashCodeString()
    {
      return GetUniqueName() + "_"+OwnerId;
    }

    public override bool Equals(object obj)
    {
      var other = obj as Loot;
      if (other == null)
        return false;
      if (other is StackedLoot otherStacked)
      { 
        var id = GetHashCodeString();
        var otherId = otherStacked.GetHashCodeString();
        return id == otherId;
      }

      return false;
    }
  }
}
