#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;
using System;
using Roguelike.Events;
using Roguelike.Attributes;
using Roguelike.Managers;
using Dungeons;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Linq;

namespace Roguelike.Tiles
{
  public class Hero : AdvancedLivingEntity
  {
    public static int FirstNextLevelExperienceThreshold = 15;
    protected Container container;

    public Hero(): base(new Point().Invalid(), '@')
    {
      canAdvanceInExp = true;
      Stats.SetNominal(EntityStatKind.Health, 150);//level up +2
      // Character.Mana = 40;
      var str = 15;
      Stats.SetNominal(EntityStatKind.Strength, str);//15
      Stats.SetNominal(EntityStatKind.Attack, str);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defence, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      NextLevelExperience = FirstNextLevelExperienceThreshold;

      CreateInventory();

      Dirty = true;//TODO

      
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
#if UNITY_EDITOR
      
#endif
    }

    public bool Identify(Equipment eq)
    {
      var scroll = Inventory.GetItems<Scroll>().Where(i=> i.Kind == Spells.SpellKind.Identify).FirstOrDefault();
      if (scroll != null)
      {
        Inventory.Remove(scroll);
        if (eq.Identify())
        {
          return true;
        }
      }
      return false;
    }

    public override string ToString()
    {
      return base.ToString();// + Data.AssetName;
    }

    public virtual void OnContextSwitched(EventsManager eventsManager, Container container)
    {
      this.container = container;
      Inventory.EventsManager = eventsManager;//TODO
    }

    
  }
}
