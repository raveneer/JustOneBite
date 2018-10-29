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
                if (Processes.Any(x => x.ProcessName == name))
                {
                    continue;
                }
                AddMonitoringProcess(name, MainWindow.SessionTimeLimitMinute, MainWindow.TotalTimeLimitMinute);
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

        private void AddMonitoringProcess(string processName, int sessionTimeLimit, int totalTimeLimit)
        {
            var newMonitoringProcess = MonitoringProcess.GetNewProcess(processName, sessionTimeLimit, totalTimeLimit);
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

        public void ResetUsedTime()
        {
            Processes.ForEach(x => x.Reset());
        }
    }
}