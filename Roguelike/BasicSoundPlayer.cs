using Roguelike.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class BasicSoundPlayer : ISoundPlayer
  {
    public void PlaySound(string soundFileName)
    {
      string filePath = "";
      try
      {
        //return;
        var sp = new SoundPlayer();
        var pathDir = Path.Combine(Environment.CurrentDirectory, "Sounds\\" + soundFileName);
        filePath = pathDir + ".wav"; 
        var ex = File.Exists(filePath);
        if(!ex)
          filePath = pathDir + ".mp3";
        sp.SoundLocation = filePath;
        sp.Play();
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message+ " "+ filePath);
      }
      Debug.WriteLine("Played snd: " + filePath);
    }

    public void StopSound() { }
  }
}
