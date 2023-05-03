using Dungeons.Core;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Core.Managers
{
  public class LeverSet
  {
    List<Lever> levers = new List<Lever>();
    List<bool> openingSequence = new List<bool>();
    public event EventHandler<bool> StateChanged;
    Container container;

    public LeverSet(Container container)
    {
      this.container = container;
    }

    public List<Lever> Levers { get => levers; }

    public void AddLever(Lever lever)
    {
      levers.Add(lever);

      lever.Interaction += Lever_Interaction;
    }

    private void Lever_Interaction(object sender, EventArgs e)
    {
      var lever = sender as Lever;
      //if (IsOpened())
      {
        if (StateChanged != null)
        {
          StateChanged(this, IsOpened());
        }
      }
    }

    public void GenerateLevers()
    {
      int max = 3;
      var openingSequence = new List<bool>();
      for (int i = 0; i < max; i++)
      {
        AddLever(new Lever(container));
        openingSequence.Add(RandHelper.GetRandomDouble() > 0.5f);
      }
      if (openingSequence.All(i => !i))
        openingSequence[RandHelper.GetRandomInt(openingSequence.Count)] = true;
      OpeningSequence = openingSequence;

    }

    public List<bool> OpeningSequence 
    { 
      get => openingSequence; 
      set => openingSequence = value; 
    }

    public bool IsOpened()
    {
      if (openingSequence.Count != Levers.Count)
        return false;

      for(int leverIndex = 0; leverIndex<Levers.Count; leverIndex++)
      {
        if (Levers[leverIndex].IsOn !=  openingSequence[leverIndex])
          return false;
      }

      return true;

    }

    public string GetStatus()
    {
      string str = "OpeningSequence: ";
      for (int leverIndex = 0; leverIndex < Levers.Count; leverIndex++)
      {
        str += openingSequence[leverIndex] + " ";

      }
      str += " Levers: ";
      for (int leverIndex = 0; leverIndex < Levers.Count; leverIndex++)
      {
        str += Levers[leverIndex].IsOn + " ";
      }
      return str;
    }
  }
}
