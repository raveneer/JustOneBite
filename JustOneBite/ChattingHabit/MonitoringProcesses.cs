using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChattingHabit
{
    public class MonitoringProcesses
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
                TryAddMonitoringProcess(name);
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

        private void TryAddMonitoringProcess(string processName)
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

            var newMonitoringProcess = MonitoringProcess.NewProcess(process);
            _processes.Add(newMonitoringProcess);
        }
    }
}