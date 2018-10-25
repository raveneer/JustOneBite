using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChattingHabit
{
    public class ProcessCollection
    {
        private readonly List<MonitoringProcess> _processes = new List<MonitoringProcess>();
        private readonly List<string> _monitoringProcessNames = new List<string>();

        public void Add(string processName)
        {
            _monitoringProcessNames.Add(processName);
        }

        public void Remove(string processName)
        {
            _processes.RemoveAll(x => x.ProcessName == processName);
        }

        public void Tick()
        {
            foreach (var name in _monitoringProcessNames)
            {
                TryAddMonitoringProcess(name, MainWindow.SessionTimeLimitMinute, MainWindow.TotalTimeLimitMinute);
            }

            foreach (var monitoringProcess in _processes)
            {
                monitoringProcess.Tick();
            }
        }

        public string GetProcessesInfo()
        {
            return string.Join("\r\n", _processes.Select(x => x.GetInfo()));
        }

        private void TryAddMonitoringProcess(string processName, int sessionTimeLimit, int totalTimeLimit)
        {
            if (_processes.Any(x => x.ProcessName == processName))
            {
                return;
            }

            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                return;
            }

            var newMonitoringProcess = MonitoringProcess.GetNewProcess(process, sessionTimeLimit, totalTimeLimit);
            _processes.Add(newMonitoringProcess);
        }

        public void ChangeAllSessionTimeLimit(int minute)
        {
            _processes.ForEach(x => x.SessionTimeLimit = new TimeSpan(0, minute, 0));
        }

        public void ChangeAllTotalTimeLimit(int minute)
        {
            _processes.ForEach(x => x.TotalUsedTimeLimit = new TimeSpan(0, minute, 0));
        }
    }
}