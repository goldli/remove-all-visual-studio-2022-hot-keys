using System;

namespace Ai2.WinApi
{
    [Flags]
    public enum WindowFromPointFlags
    {
        CWP_ALL = 0,
        CWP_SKIPINVISIBLE = 1,
        CWP_SKIPDISABLED = 2,
        CWP_SKIPTRANSPARENT = 4
    }
}