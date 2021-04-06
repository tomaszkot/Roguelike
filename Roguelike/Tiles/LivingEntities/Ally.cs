using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.LivingEntities
{
  public abstract class Ally : LivingEntities.AdvancedLivingEntity, IAlly
  {
    public Ally(char symbol = '!') : base(new Point().Invalid(), symbol)
    {
      Stats.SetNominal(EntityStatKind.Health, 15);
      var str = 15;
      Stats.SetNominal(EntityStatKind.Strength, str);//15
      Stats.SetNominal(EntityStatKind.Attack, str);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defense, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      CreateInventory(null);

      Dirty = true;//TODO
    }

    public bool Active { get; set; }

    public AllyKind Kind
    {
      get
      {
        return AllyKind.Enemy;
      }
      set { }
    }

    public override void SetLevel(int level)
    {
      base.SetLevel(level);
    }

    public static Ally Spawn<T>(char symbol, int level) where T : Ally, new()
    {
      var ally = new T();
      ally.Symbol = symbol;
      ally.SetLevel(level);
      ally.SetTag();
      ally.Revealed = true;
      ally.Active = true;

      return ally;
    }

    public abstract void SetTag();

    public override bool GetGoldWhenSellingTo(IAdvancedEntity dest)
    {
      return false;
    }
  }
    
}
