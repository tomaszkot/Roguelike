using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Multimedia
{
  public interface ISoundPlayer
  {
    void PlaySound(string sound);
    void StopSound();
  }
}
