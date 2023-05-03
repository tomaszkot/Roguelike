using Roguelike.Abstract.Multimedia;
using System;
using System.IO;
//using System.Media;

namespace Roguelike.Multimedia
{
  public class BasicSoundPlayer : ISoundPlayer
  {
    //SoundPlayer sp = new SoundPlayer();

    public void PlaySound(string soundFileName)
    {
      string filePath = "";
      try
      {
        //return;
        var pathDir = Path.Combine(Environment.CurrentDirectory, "Sounds\\" + soundFileName);
        filePath = pathDir + ".wav";
        var ex = File.Exists(filePath);
        if (!ex)
          filePath = pathDir + ".mp3";

        ex = File.Exists(filePath);
        if (!ex)
          return;

        //sp.SoundLocation = filePath;
        //sp.Play();
      }
      catch (Exception /*ex*/)
      {
        //Debug.WriteLine("PlaySound: " + ex.Message+ " "+ filePath);
      }
      //Debug.WriteLine("Played snd: " + filePath);
    }

    public void StopSound() { }
  }
}
