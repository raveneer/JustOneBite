using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChattingHabit
{
    public class MonitoringProcesses
    {
        private readonly List<MonitoringProcess> _processes = new List<MonitoringProcess>();

        public void Add(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                return;
            }

            var newMonitoringProcess = MonitoringProcess.NewProcess(process);
            _processes.Add(newMonitoringProcess);
        }

        public void Remove(string processName)
        {
            _processes.RemoveAll(x => x.ProcessName == processName);
        }

        public void Tick()
        {
            foreach (var monitoringProcess in _processes)
            {
                monitoringProcess.Tick();
            }
        }
    }
}