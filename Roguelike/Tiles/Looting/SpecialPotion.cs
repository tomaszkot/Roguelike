using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public enum SpecialPotionKind { Unknown, Strength, Magic,
    Attack //deprecated, to enable old saves being loadable
  }

  [Serializable]
  public class SpecialPotion : Loot
  {
    SpecialPotionKind kind;
    public bool BigPotion = true;

    public SpecialPotion() : this(SpecialPotionKind.Unknown, true)
    {
    }

    public SpecialPotion(SpecialPotionKind kind, bool big)
    {
      BigPotion = big;
      Kind = kind;
      Price = big ? 100 : 25;
      Symbol = '`';

    }

    public SpecialPotionKind Kind
    {
      get
      {
        return kind;
      }

      set
      {
        kind = value;
        if (value == SpecialPotionKind.Strength)
        {
          tag1 = "attack_potion";//HACK
          Name = "Strength Potion";
        }
        else if (value == SpecialPotionKind.Magic)
        {
          tag1 = "magic_potion";
          Name = "Magic Potion";
        }

        if (!BigPotion)
        {
          tag1 += "_small";
          Name = "Small " + Name;
        }
      }
    }
    bool used = false;
    public void Apply(LivingEntity ent)
    {
      if (used)
        return;

      //TODO
      //EntityStatKind esk = GetDestStat();
      //var enh = GetEnhValue();
      //ent.Stats[esk].NominalValue += enh;
      //for (int i = 0; i < enh; i++)
      //  ent.Stats.EmitStatsLeveledUp(GetDestStat());
      //GameManager.Instance.AppendAction(new LootAction() { Info = ent.Name + " drunk " + Name, Loot = this, KindValue = LootAction.Kind.SpecialDrunk });
      //used = true;
    }

    private int GetEnhValue()
    {
      return BigPotion ? 5 : 1;
    }



    public EntityStatKind GetDestStat()
    {
      return Kind == SpecialPotionKind.Strength ? EntityStatKind.Strength : EntityStatKind.Magic;
    }

    public string GetPrimaryStatDescription()
    {
      var desc = "Permamently adds " + GetEnhValue() + " to " + GetDestStat();// + ".";
      return desc;
    }

    public override string[] GetExtraStatDescription()
    {
      if (extraStatDescription == null)
      {
        var desc = "";
        //if (GameManager.Instance.GameSettings.GameControllingMode == GameControllingMode.MouseAndKeyboard)
        //  desc += "Press X in the inventory\r\nto consume";
        //else
          desc += "Double Tap it in the inventory\r\nto consume";
        extraStatDescription = new string[1];
        extraStatDescription[0] = desc;
      }
      return extraStatDescription;
    }

    public override string ToString()
    {
      return this.Name;
    }
  }
}
