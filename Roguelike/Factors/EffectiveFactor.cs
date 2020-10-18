namespace Roguelike.Factors
{
  public class EffectiveFactor : Factor
  {
    public EffectiveFactor(float val) : base(val) { }

    //effective value deducted/added to a stat
    public double Value
    {
      get { return value; }
    }
  }
}
