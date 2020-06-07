using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootFactories
{
  public class ScrollsFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Scroll>> factory = 
      new Dictionary<string, Func<string, Scroll>>();

    public ScrollsFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      Func<string, Scroll> createScroll = (string tag) =>
      {
        var scroll = new Scroll();
        scroll.tag1 = tag;
        scroll.Kind = Scroll.DiscoverKindFromName(tag);
        return scroll;
      };
      var names = new[] { "FireBallScroll" , "IdentifyScroll" };
      foreach(var name in names)
        factory[name] = createScroll;
    }

    public override Loot GetRandom()
    {
      return GetRandom<Scroll>(factory);
    }

    public override Loot GetByName(string name)
    {
      return GetByTag(name);
    }

    public override Loot GetByTag(string tagPart)
    {
      var tile = factory.FirstOrDefault(i => i.Key == tagPart);
      if (tile.Key != null)
        return tile.Value(tagPart);

      return null;
    }

    //public override Loot GetByTag(string tagPart)
    //{
    //  var sc = scrolls.Where(i => i.tag1  == tagPart).SingleOrDefault();
    //  return sc.CloneAsScroll();
    //}

    public Loot GetByKind(Spells.SpellKind kind)
    {
      var tile = factory.FirstOrDefault(i => Scroll.DiscoverKindFromName(i.Key) == kind);
      if (tile.Key != null)
        return tile.Value(tile.Key);

      return null;
    }

    //protected override void Create()
    //{
    //  var loot = new Scroll();
    //  loot.tag1 = "fire_ball_scroll";
    //  loot.Kind = Spells.SpellKind.FireBall;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "identify_scroll";
    //  loot.Kind = Spells.SpellKind.Identify;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "NESW_fire_scroll";
    //  loot.Kind = Spells.SpellKind.NESWFireBall;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "cracked_stone_scroll";
    //  loot.Kind = Spells.SpellKind.CrackedStone;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "trap_stone_scroll";
    //  loot.Kind = Spells.SpellKind.Trap;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "skeleton_stone_scroll";
    //  loot.Kind = Spells.SpellKind.Skeleton;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "transform_scroll";
    //  loot.Kind = Spells.SpellKind.Transform;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "poison_ball_scroll";
    //  loot.Kind = Spells.SpellKind.PoisonBall;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "ice_ball_scroll";
    //  loot.Kind = Spells.SpellKind.IceBall;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "frighten_scroll";
    //  loot.Kind = Spells.SpellKind.Frighten;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "healing_scroll";
    //  loot.Kind = Spells.SpellKind.Healing;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "mana_shield_scroll";
    //  loot.Kind = Spells.SpellKind.ManaShield;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "telekinesis_scroll";
    //  loot.Kind = Spells.SpellKind.Telekinesis;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "mana_scroll";
    //  loot.Kind = Spells.SpellKind.Mana;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "rage_scroll";
    //  loot.Kind = Spells.SpellKind.Rage;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "weaken_scroll";
    //  loot.Kind = Spells.SpellKind.Weaken;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "iron_skin_scroll";
    //  loot.Kind = Spells.SpellKind.IronSkin;
    //  scrolls.Add(loot);
    //  //loot = new Scroll();
    //  //loot.tag = "mind_control_scroll";
    //  //loot.Kind = Spells.SpellKind.MindControl;
    //  //scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "teleport_scroll";
    //  loot.Kind = Spells.SpellKind.Teleport;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "call_merchant_scroll";
    //  loot.Kind = Spells.SpellKind.CallMerchant;
    //  scrolls.Add(loot);

    //  loot = new Scroll();
    //  loot.tag1 = "call_god_scroll";
    //  loot.Kind = Spells.SpellKind.CallGod;
    //  scrolls.Add(loot);

    //  //
    //  loot = new Scroll();
    //  loot.tag1 = "lighting_scroll";
    //  loot.Kind = Spells.SpellKind.LightingBall;
    //  scrolls.Add(loot);
    //}
  }
}
