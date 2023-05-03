﻿using System;
using System.Runtime.InteropServices;

namespace DungeonsConsoleRunner
{
  class ConsoleSetup
  {
    private const int STD_OUTPUT_HANDLE = -11;
    private const int TMPF_TRUETYPE = 4;
    private const int LF_FACESIZE = 32;
    private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct CONSOLE_FONT_INFO_EX
    {
      internal uint cbSize;
      internal uint nFont;
      internal COORD dwFontSize;
      internal int FontFamily;
      internal int FontWeight;
      internal fixed char FaceName[LF_FACESIZE];
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct COORD
    {
      internal short X;
      internal short Y;

      internal COORD(short x, short y)
      {
        X = x;
        Y = y;
      }
    }


    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetCurrentConsoleFontEx(
        IntPtr consoleOutput,
        bool maximumWindow,
        ref CONSOLE_FONT_INFO_EX consoleCurrentFontEx);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int dwType);


    [DllImport("kernel32.dll", SetLastError = true)]
    static extern int SetConsoleFont(
        IntPtr hOut,
        uint dwFontNum
        );
    public static void SetConsoleFont(string fontName = "Lucida Console")
    {
      unsafe
      {
        IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
        if (hnd != INVALID_HANDLE_VALUE)
        {
          CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
          info.cbSize = (uint)Marshal.SizeOf(info);

          // Set console font to Lucida Console.
          CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
          newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
          newInfo.FontFamily = TMPF_TRUETYPE;
          IntPtr ptr = new IntPtr(newInfo.FaceName);
          Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

          // Get some settings from current font.
          newInfo.dwFontSize = new COORD(info.dwFontSize.X, info.dwFontSize.Y);
          newInfo.FontWeight = 700;
          SetCurrentConsoleFontEx(hnd, false, ref newInfo);
        }
      }
    }

    public static void Init()
    {
      var ww = Console.WindowWidth;
      var wh = Console.WindowHeight;
      try
      {
        Console.SetWindowSize((int)(ww * 1.0f), wh * 2);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      ConsoleSetup.SetConsoleFont();
    }
  }
}
