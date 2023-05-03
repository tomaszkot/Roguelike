using System;
using System.Drawing;

namespace Dungeons.Core
{
  public struct Vector2D
  {
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2D(float x, float y)
    {
      X = x;
      Y = y;
    }

    public override bool Equals(Object obj)
    {
      return (obj is Vector2D) && ((Vector2D)obj).X == X && ((Vector2D)obj).Y == Y;
    }

    public static bool operator==(Vector2D obj1, Vector2D obj2)
    {
      return obj1.Equals(obj2);
    }

    public static bool operator!=(Vector2D obj1, Vector2D obj2)
    {
      return !obj1.Equals(obj2);
    }

    // For lazyness reasons we (incorrectly) use the age as the hash code.
    public override int GetHashCode()
    {
      unchecked // Overflow is fine, just wrap
      {
        int hash = 17;
        // Suitable nullity checks etc, of course :)
        hash = hash * 23 + X.GetHashCode();
        hash = hash * 23 + Y.GetHashCode();
        return hash;
      }
    }

    public override string ToString()
    {
      return base.ToString() + X+","+Y;
    }

    public Point ToPoint()
    {
      return new Point((int)X, (int)Y);
    }

    public Vector2D MoveBy(float x, float y)
    {
      return new Vector2D(X+x, Y +y);
    }
  }
}
