using Godot;
using System;
using Roguelike.Abstract.Multimedia;

public class MusicPlayer : ISoundPlayer
{
  public static AudioStreamPlayer audioPlayer;
  public void PlaySound(string soundName)
  {


	if (ResourceLoader.Exists("res://Sounds/" + soundName + ".wav"))
	{
	  AudioStream stream = ResourceLoader.Load("res://Sounds/" + soundName + ".wav") as AudioStream;
	  audioPlayer.Stream = stream;
	  audioPlayer.Play();
	}
  }

  public void StopSound()
  {
	throw new NotImplementedException();
  }
}
