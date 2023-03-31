using KeyboardRemoveVShotKeys.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Ai2.WinApi
{
    public static class WMI
    {
        public static List<ProcessDetail> GetProcess()
        {
            using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process");
            using ManagementObjectCollection source = managementObjectSearcher.Get();
            IEnumerable<ProcessDetail> source2 = from p in Process.GetProcesses()
                                                 join mo in source.Cast<ManagementObject>() on p.Id equals (int)(uint)mo["ProcessId"]
                                                 select new ProcessDetail
                                                 {
                                                     Process = p,
                                                     Path = (string)mo["ExecutablePath"],
                                                     CommandLine = (string)mo["CommandLine"]
                                                 };
            return source2.ToList();
        }

        public static List<ProcessDetail> GetProcessByName(string processName)
        {
            List<ProcessDetail> list = new List<ProcessDetail>();
            Process[] processesByName = Process.GetProcessesByName(processName);
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process"))
            {
                using ManagementObjectCollection source = managementObjectSearcher.Get();
                IEnumerable<ProcessDetail> collection = from p in processesByName
                                                        join mo in source.Cast<ManagementObject>() on p.Id equals (int)(uint)mo["ProcessId"]
                                                        select new ProcessDetail
                                                        {
                                                            Process = p,
                                                            Path = (string)mo["ExecutablePath"],
                                                            CommandLine = (string)mo["CommandLine"]
                                                        };
                list.AddRange(collection);
            }
            return list;
        }

        public static List<ProcessDetail> GetProcessByPath(string exePath)
        {
            List<ProcessDetail> list = new List<ProcessDetail>();
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process"))
            {
                using ManagementObjectCollection source = managementObjectSearcher.Get();
                IEnumerable<ProcessDetail> source2 = from p in Process.GetProcesses()
                                                     join mo in source.Cast<ManagementObject>() on p.Id equals (int)(uint)mo["ProcessId"]
                                                     select new ProcessDetail
                                                     {
                                                         Process = p,
                                                         Path = (string)mo["ExecutablePath"],
                                                         CommandLine = (string)mo["CommandLine"]
                                                     };
                list.AddRange(source2.Where((ProcessDetail p) => !string.IsNullOrEmpty(p.Path) && p.Path.Equals(exePath, StringComparison.CurrentCultureIgnoreCase)));
            }
            return list;
        }
    }
}