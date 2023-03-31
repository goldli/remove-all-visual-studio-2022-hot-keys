using System;

namespace Ai2.WinApi
{
    public struct MOUSEINPUT
    {
        public int X;

        public int Y;

        public uint MouseData;

        public uint Flags;

        public uint Time;

        public IntPtr ExtraInfo;
    }
}