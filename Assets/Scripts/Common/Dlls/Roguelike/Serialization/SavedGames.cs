using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Roguelike.Core.Serialization
{
  public class SavedGames
  {
    public static List<string> GetSavedGamesList()
    {
      List<string> saves = new List<string>();

      string path = System.IO.Path.GetTempPath() + "Roguelike";
      string[] folders = Directory.GetDirectories(path);
      saves.AddRange(folders);

      return saves;
    }
  }
}
