namespace Roguelike.Factors
{
  public class Factor
  {
    protected float value;

    public Factor(float val)
    {
      value = val;
    }

    public override string ToString()
    {
      var sign = value >= 0 ? "+" : "";
      return sign + value.ToString();
    }
  }

  public class PercentageFactor : Factor
  {
    public PercentageFactor(float val) : base(val)
    {
    }

    //percentage value deducted/added to a stat
    public float Value
    {
      get { return value; }
    }

    public override string ToString()
    {
      return base.ToString() + "%";
    }
  }
}
