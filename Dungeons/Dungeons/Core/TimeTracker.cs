using System;

namespace Dungeons.Core
{
  public class TimeTracker
  {
    DateTime start = DateTime.Now;
    double pausedSeconds = 0;

    public TimeTracker()
    {
    
    }
    //DateTime pauseStart = DateTime.MinValue;

    //public void Update()
    //{
    //    //if (ApplicationService.Instance.GamePaused)
    //    //{
    //    //    if (pauseStart == DateTime.MinValue)
    //    //    {
    //    //        //pausedSeconds = 0;
    //    //        pauseStart = DateTime.Now;
    //    //    }
    //    //}
    //    //else if (pauseStart != DateTime.MinValue)
    //    //{
    //    //    pausedSeconds += (DateTime.Now - pauseStart).TotalSeconds;
    //    //    pauseStart = DateTime.MinValue;
    //    //}
    //}

    public void Reset()
    {
      start = DateTime.Now;
    }

    public double TotalSeconds
    {
      get
      {
        return (DateTime.Now - start).TotalSeconds - pausedSeconds;
      }
    }

  }
}
