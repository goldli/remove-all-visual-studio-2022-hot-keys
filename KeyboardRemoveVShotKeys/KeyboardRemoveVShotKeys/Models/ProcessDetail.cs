using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardRemoveVShotKeys.Models
{
    public class ProcessDetail
    {
        public int ProcessId => Process?.Id ?? 0;

        public Process Process { get; set; }

        public string Path { get; set; }

        public string CommandLine { get; set; }
    }
}
