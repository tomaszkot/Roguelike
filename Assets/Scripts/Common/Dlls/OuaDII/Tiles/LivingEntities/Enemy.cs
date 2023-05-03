using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Drawing;

namespace OuaDII
{
  public class Pack //a few of enemies cooperating
  {

  };

  namespace Tiles.LivingEntities
  {
    public class Enemy : Roguelike.Tiles.LivingEntities.Enemy
    {
      public Pack Pack { get; set; }

      public Enemy(Container cont) : this(cont, 'e')
      { 
      }

      public Enemy(Container cont, char symbol) : this(new Point(-1,-1), symbol, cont)
      {
      }

      public Enemy(Point point, char symbol, Container cont) : base(point, symbol, cont)
      {
      }

      public override string DisplayedName 
      {
        get
        {
          if (Name.Contains("Tree monster"))
          {
            if (IsSleeping)
              return "Tree";
            else
              return "Tree Monster";
          }
          return base.DisplayedName;
        }
        set => base.DisplayedName = value.GetCapitalized(); 
      }

      
    }
  }
}