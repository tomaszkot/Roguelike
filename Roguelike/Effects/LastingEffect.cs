using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Xml.Serialization;

namespace Roguelike.Effects
{
  public class LastingEffect
  {
    public EffectType Type;
    public EntityStatKind StatKind;
    public int PendingTurns = 3;
    public float DamageAmount = 0;
    public float Subtraction 
    {
      get; 
      set; 
    }
    //public bool FromTrapSpell { get; internal set; }
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

    public string GetDescription(LivingEntity owner)
    {
      string res = Type.ToDescription();

      var damage = owner.CalcDamageAmount(this);// Owner.LivingEntityTile.CalcDamageAmount(le);
      var subtraction = Subtraction;

      var spellKind = SpellConverter.SpellKindFromEffectType(Type);
      if (Type == EffectType.Bleeding)
      {
        res += ", -" + damage + " Health (per turn)";
      }
      else if (Type == EffectType.ResistAll)
      {
        res += " +" + Subtraction;
      }
      else if (spellKind != SpellKind.Unset)
      {
        //Scroll.CreateSpell(spellKind, )
        string preffix = "+";
        if (Type == EffectType.Weaken || Type == EffectType.Inaccuracy)
          preffix = "-";
        res += ", " + preffix + subtraction + " to " + this.StatKind.ToDescription();
      }
      return res;
    }

  }
}
