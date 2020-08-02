using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Tiles;
using System.Xml.Serialization;

namespace Roguelike.Effects
{
  public class LastingEffect
  {
    public EffectType Type;
    public EntityStatKind StatKind;
    public int PendingTurns = 3;
    public float DamageAmount = 0;
    public bool FromTrapSpell { get; internal set; }
    ILastingEffectOwner owner;

    [XmlIgnore]
    [JsonIgnore]
    public ILastingEffectOwner Owner
    {
      get
      {
        return owner;
      }

      set
      {
        owner = value;
        if (owner != null)
          owner.OnEffectStarted(Type);
      }
    }

    public LastingEffect() { }
    public LastingEffect(EffectType type, ILastingEffectOwner owner)
    {
      this.Type = type;
      this.Owner = owner;
    }

    internal void Dispose()
    {
      if (Owner != null)
        Owner.OnEffectFinished(Type);
    }
  }
}
