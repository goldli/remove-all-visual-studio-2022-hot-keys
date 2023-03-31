using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using KeyboardRemoveVShotKeys.Models;

namespace Ai2.WinApi
{
    public static class NativeMethods
    {
        public static HandleRef ToRef(this IntPtr handle)
        {
            return new HandleRef(null, handle);
        }

        public static void SetText(IntPtr hWnd, string strTextToSet)
        {
            Win32.SendMessage(hWnd.ToRef(), 12u, IntPtr.Zero, strTextToSet);
        }

        public static void SendButtonClick(IntPtr hWnd)
        {
            Win32.SendMessage(hWnd.ToRef(), 245u, IntPtr.Zero, IntPtr.Zero);
        }

        public static void MouseClick()
        {
            INPUT iNPUT = default(INPUT);
            iNPUT.Type = 0u;
            INPUT iNPUT2 = iNPUT;
            iNPUT2.Data.Mouse.Flags = 2u;
            iNPUT = default(INPUT);
            iNPUT.Type = 0u;
            INPUT iNPUT3 = iNPUT;
            iNPUT3.Data.Mouse.Flags = 4u;
            INPUT[] array = new INPUT[2] { iNPUT2, iNPUT3 };
            Win32.SendInput((uint)array.Length, array, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void OutputKey(string[] keys)
        {
        }

        public static void MouseClick(int x, int y)
        {
            Win32.SetCursorPos(x, y);
            Thread.Sleep(100);
            MouseClick();
        }

        public static void MouseClick(IntPtr hWnd, POINT p)
        {
            Win32.ClientToScreen(hWnd, ref p);
            Win32.SetCursorPos(p.x, p.y);
            Thread.Sleep(100);
            MouseClick();
        }

        public static void MouseRightClick()
        {
            INPUT iNPUT = default(INPUT);
            iNPUT.Type = 0u;
            INPUT iNPUT2 = iNPUT;
            iNPUT2.Data.Mouse.Flags = 8u;
            iNPUT = default(INPUT);
            iNPUT.Type = 0u;
            INPUT iNPUT3 = iNPUT;
            iNPUT3.Data.Mouse.Flags = 16u;
            INPUT[] array = new INPUT[2] { iNPUT2, iNPUT3 };
            Win32.SendInput((uint)array.Length, array, Marshal.SizeOf(typeof(INPUT)));
        }

        public static string GetWindowText(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return string.Empty;
            }
            int num = Win32.SendMessage(windowHandle, 14, 0, 0);
            byte[] array = new byte[num];
            Win32.SendMessage(windowHandle, 13, num + 1, array);
            return Encoding.Default.GetString(array);
        }

        private static bool IsWindowVisible(IntPtr wndHandle)
        {
            var wndStyle = (WindowStyles)Win32.GetWindowLong(wndHandle, (int)WindowLongFlags.GWL_STYLE);
            Debug.WriteLine($"WindowStyles= {wndStyle}"); 
            return (wndStyle & WindowStyles.WS_VISIBLE) == WindowStyles.WS_VISIBLE;
        }

        public static List<WindowInfo> GetTopWindows()
        {
            List<WindowInfo> result = new List<WindowInfo>();
            Win32.EnumWindows(delegate (IntPtr h, IntPtr l)
            {
                Win32.GetWindowThreadProcessId(h, out var lpdwProcessId);

                var windowInfo = new WindowInfo()
                {
                    Handle = h,
                    ProcessId = lpdwProcessId,
                    IsVisible = IsWindowVisible(h)
                };

                if (windowInfo.IsVisible)
                {
                    windowInfo.Caption = GetWindowText(h);
                    windowInfo.ClassName = GetClassName(h);

                    if (Win32.GetWindowRect(h, out var lpRect))
                    {
                        windowInfo.Rect = lpRect;
                    }

                    result.Add(windowInfo);
                }

                return true;

            }, IntPtr.Zero);
            return result;
        }

        public static List<WindowInfo> GetChildrenWindows(WindowInfo parent)
        {
            List<WindowInfo> result = new List<WindowInfo>();
            if (parent != null)
            {
                Win32.EnumChildWindows(parent.Handle, delegate (IntPtr h, IntPtr l)
                {
                    Win32.GetWindowThreadProcessId(h, out var lpdwProcessId);
                    WindowInfo windowInfo = new WindowInfo
                    {
                        Handle = h,
                        ProcessId = lpdwProcessId,
                        IsVisible = Win32.IsWindowVisible(h),
                        Caption = GetWindowText(h),
                        ClassName = GetClassName(h)
                    };
                    if (Win32.GetWindowRect(h, out var lpRect))
                    {
                        windowInfo.Rect = lpRect;
                    }
                    result.Add(windowInfo);
                    return true;
                }, IntPtr.Zero);
            }
            return result;
        }

        public static string GetClassName(IntPtr hWnd)
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            Win32.GetClassName(hWnd, stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
        }

        public static void CloseWindow(WindowInfo window)
        {
            if (window != null)
            {
            }
        }

        public static void CloseWindow(IntPtr hWnd)
        {
            if (!(hWnd == IntPtr.Zero))
            {
                HandleRef hWnd2 = new HandleRef(null, hWnd);
                Win32.SendMessage(hWnd2, 16u, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static Color GetColor(int x, int y)
        {
            IntPtr dC = Win32.GetDC(IntPtr.Zero);
            uint pixel = Win32.GetPixel(dC, x, y);
            Win32.ReleaseDC(IntPtr.Zero, dC);
            return Color.FromArgb((int)(pixel & 0xFF), (int)(pixel & 0xFF00) >> 8, (int)(pixel & 0xFF0000) >> 16);
        }

    }
}