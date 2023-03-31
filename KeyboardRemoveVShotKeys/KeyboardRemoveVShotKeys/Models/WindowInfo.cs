using Ai2.WinApi;
using System;

namespace KeyboardRemoveVShotKeys.Models
{
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }

        public uint ProcessId { get; set; }

        public string Caption { get; set; }

        public string ClassName { get; set; }

        public bool IsVisible { get; set; }

        public RECT? Rect { get; set; }
    }
}
