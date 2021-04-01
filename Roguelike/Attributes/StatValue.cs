using Newtonsoft.Json;
using System;

namespace Roguelike.Attributes
{
  public class StatValue
  {
    /// <summary>
    /// Original, native Attribute Value, in case of living entity can be increased by experience points
    /// </summary>
    private float nominal = 0;

    /// <summary>
    /// Amount of values from equipment or abilities
    /// </summary>
    float factor = 0;

    /// <summary>
    /// Amount of values subtracted, e.g. gained damage or used mana
    /// </summary>
    float subtracted = 0;


    public float Nominal
    {
      get
      {
        return nominal;
      }

      set
      {
        nominal = value;
      }
    }

    public float Factor
    {
      get
      {
        return factor;
      }

      set
      {
        factor = value;
      }
    }

    public override string ToString()
    {
      return "NV: " + Nominal + ", F: " + Factor + ", Sub:" + Subtracted;
    }

    [JsonIgnore]
    public float TotalValue
    {
      get { return Nominal + Factor; }
    }

    /// <summary>
    /// When damage was gained or spell used total is reduced by subtraction
    /// </summary>
    [JsonIgnore]
    public float CurrentValue
    {
      get { return CalculateCurrentValue(Subtracted); }
    }

    public float CalculateCurrentValue(float subtracted)
    {
      return TotalValue - subtracted;
    }

    public float Subtracted
    {
      get
      {
        return subtracted;
      }

      set
      {
        subtracted = value;
      }
    }

    public void Divide(float value)
    {
      if (value != 0)
      {
        Nominal /= value;

        Factor /= value;

        Subtracted /= value;

      }
    }

    public void MakeNegative()
    {
      Nominal *= -1;
      Factor *= -1;
      Subtracted *= -1;

    }

    public void Accumulate(StatValue other)
    {
      Nominal += other.Nominal;
      Factor += other.Factor;
      Subtracted += other.Subtracted;

    }

    public void Divide(StatValue other)
    {
      Nominal /= other.Nominal;
      if (other.Factor != 0)
        Factor /= other.Factor;
      if (other.Subtracted != 0)
        Subtracted /= other.Subtracted;
    }
        
    public object Clone()
    {
      var clone = MemberwiseClone();
      return clone;
    }
  }
}
