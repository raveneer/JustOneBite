using System;
using System.Diagnostics;
using System.Linq;

namespace ChattingHabit
{
    public class MonitoringProcess
    {
        public string ProcessName;
        public TimeSpan TotalUsedTime;
        public TimeSpan SessionUsedTime;
        public int TotalSessionCount;
        public TimeSpan SessionTimeLimit;
        public TimeSpan TotalUsedTimeLimit;

        private const int DefaultSessionLimitMin = 1;
        private const int DefaultTotalLimitMin = 60;

        public void Tick()
        {
            var isRunning = IsRunning();
            if (!isRunning)
            {
                return;
            }

            SessionUsedTime += new TimeSpan(0, 0, MainWindow.TICKSECONDS);
            TotalUsedTime += new TimeSpan(0, 0, MainWindow.TICKSECONDS);

            if (SessionUsedTime > SessionTimeLimit)
            {
                EventManager.Broadcast_ProcessSessionLimitReached(this);
                KillProcess();
            }

            if (TotalUsedTime > TotalUsedTimeLimit)
            {
                EventManager.Broadcast_ProcessTotalLimitReached(this);
                KillProcess();
            }
        }

        public static MonitoringProcess NewProcess(Process process)
        {
            return new MonitoringProcess
            {
                ProcessName = process.ProcessName,
                SessionTimeLimit = new TimeSpan(0, DefaultSessionLimitMin, 0),
                TotalUsedTimeLimit = new TimeSpan(0, DefaultTotalLimitMin, 0)
            }
             ;
        }

        private bool IsRunning()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            return processes.Any();
        }

        public void KillProcess()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            foreach (var process in processes)
            {
                process.Kill();
            }
        }
    }
}