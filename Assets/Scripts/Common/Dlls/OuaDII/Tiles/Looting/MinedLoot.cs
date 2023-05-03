using Roguelike;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public enum MinedLootKind { Unset, SilesiaCoal, IronOre, Sulfur }

  public class MinedLoot : StackedLoot
  {
    public MinedLootKind Kind { get; set; }
    public static readonly MinedLootKind[] MinedLootKinds;

    static MinedLoot()
    {
      MinedLootKinds = Enum.GetValues(typeof(MinedLootKind)).Cast<MinedLootKind>().ToArray();
    }

    public MinedLoot()
    {
    }

    public MinedLoot(MinedLootKind kind)
    {
      Symbol = '&';
      SetKind(kind);
    }

    public void SetKind(MinedLootKind kind)
    {
      Kind = kind;
      Name = Kind.ToDescription();
      if (Kind == MinedLootKind.SilesiaCoal)
      {
        PrimaryStatDescription = "High quality coal. Steel made of it is highly valuable.";
        tag1 = "silesia_coal";
      }
      else if (Kind == MinedLootKind.IronOre)
      {
        PrimaryStatDescription = "High quality iron ore. Steel made of it is highly valuable.";
        tag1 = "iron_ore";
      }
      else if (Kind == MinedLootKind.Sulfur)
      {
        PrimaryStatDescription = "High quality sulfur nugget. "+ Roguelike.Tiles.Strings.PartOfCraftingRecipe;
        tag1 = "sulfur";
      }

      Price = 10;
    }

    public override string ToString()
    {
      return base.ToString() + " " + Kind;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }

    public void DiscoverKind(string kind)
    {
      if (!kind.Trim().Any())
        throw new Exception("DiscoverKind empty kind");

      var parts = kind.Replace("_", "");
      //var parts = kind.Split("_".ToCharArray());
      //if (!parts.Any())
      //  parts = new string[] { kind };

      var fk = MinedLootKinds.FirstOrDefault(i => i.ToString().ToLower() == parts.ToLower());
      if (fk == MinedLootKind.Unset)
      {
        throw new Exception("DiscoverKind failed for " + kind);
      }
      SetKind(fk);

    }
  }
}
