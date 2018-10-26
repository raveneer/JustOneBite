using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChattingHabit
{
    [Serializable]
    public class ProcessCollection
    {
        //제이슨 저장을 위해 부득이 public 으로 둠.
        public List<MonitoringProcess> Processes = new List<MonitoringProcess>();

        //제이슨 저장을 위해 부득이 public 으로 둠.
        public List<string> MonitoringProcessNames = new List<string>();

        public void Add(string processName)
        {
            MonitoringProcessNames.Add(processName);
        }

        public void Remove(string processName)
        {
            Processes.RemoveAll(x => x.ProcessName == processName);
        }

        public void Tick()
        {
            foreach (var name in MonitoringProcessNames)
            {
                TryAddMonitoringProcess(name, MainWindow.SessionTimeLimitMinute, MainWindow.TotalTimeLimitMinute);
            }

            foreach (var monitoringProcess in Processes)
            {
                monitoringProcess.Tick();
            }
        }

        public string GetProcessesInfo()
        {
            return string.Join("\r\n", Processes.Select(x => x.GetInfo()));
        }

        private void TryAddMonitoringProcess(string processName, int sessionTimeLimit, int totalTimeLimit)
        {
            if (Processes.Any(x => x.ProcessName == processName))
            {
                return;
            }

            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                return;
            }

            var newMonitoringProcess = MonitoringProcess.GetNewProcess(process, sessionTimeLimit, totalTimeLimit);
            Processes.Add(newMonitoringProcess);
        }

        public void ChangeAllSessionTimeLimit(int minute)
        {
            Processes.ForEach(x => x.SessionTimeLimit = new TimeSpan(0, minute, 0));
        }

        public void ChangeAllTotalTimeLimit(int minute)
        {
            Processes.ForEach(x => x.TotalUsedTimeLimit = new TimeSpan(0, minute, 0));
        }
    }
}