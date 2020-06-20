using Roguelike.Managers;
using System;
using System.Collections.Generic;
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
      try
      {
        return;
        //var sp = new SoundPlayer();
        //var filePath = Path.Combine(Environment.CurrentDirectory, "sounds\\" + soundFileName + ".wav");
        //var ex = File.Exists(filePath);
        //sp.SoundLocation = filePath;
        //sp.Play();
      }
      catch (Exception )
      {
        //Debug.WriteLine(ex.Message);
      }
    }

    public void StopSound() { }
  }
}
