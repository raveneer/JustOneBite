using System;
using System.Diagnostics;
using System.Linq;
using System.Media;

namespace ChattingHabit
{
    [Serializable]
    public class MonitoringProcess
    {
        public int TotalSessionCount;
        public string ProcessName;
        public TimeSpan SessionTimeLimit;
        public TimeSpan SessionUsedTime;
        public TimeSpan TotalUsedTime;
        public TimeSpan TotalUsedTimeLimit;
        public DateTime LastSessionClosedTime;
        private readonly TimeSpan SessionCoolDown = new TimeSpan(0, 10, 0);

        private DateTime _prevTickTime;

        public void Tick()
        {
            var isRunning = IsAppRunning() || IsWebPageRunning();
            if (!isRunning)
            {
                return;
            }

            //마지막으로 강제로 꺼진 후, 일정 시간 안에는 다시 켤 수 없다.
            if (DateTime.Now - LastSessionClosedTime < SessionCoolDown)
            {
                KillProcess();
            }

            IncreaseUsedTime();

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

        private void IncreaseUsedTime()
        {
            TimeSpan usedTimeThisTick;
            if (_prevTickTime.Year == 1) //초기값이면 (구조체이므 초기값은 1년임)
            {
                _prevTickTime = DateTime.Now;
                usedTimeThisTick = new TimeSpan(1);
            }
            else
            {
                usedTimeThisTick = DateTime.Now - _prevTickTime;
            }

            SessionUsedTime += usedTimeThisTick;
            TotalUsedTime += usedTimeThisTick;
            _prevTickTime = DateTime.Now;
        }

        public static MonitoringProcess GetNewProcess(string name, int sessionTimeLimit, int totalTimeLimit)
        {
            return new MonitoringProcess
            {
                ProcessName = name,
                SessionTimeLimit = new TimeSpan(0, sessionTimeLimit, 0),
                TotalUsedTimeLimit = new TimeSpan(0, totalTimeLimit, 0)
            };
        }

        public void KillProcess()
        {
            KillApp();
            KillWebPage();

            SessionUsedTime = new TimeSpan(0);
            LastSessionClosedTime = DateTime.Now;
            EventManager.Broadcast_PlayTimeOverSound();
        }

        private void KillWebPage()
        {
            WebPageMonitor.CloseFocusedWebPage(ProcessName);
        }

        private void KillApp()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            foreach (var process in processes)
            {
                process.Kill();
            }
        }

        public string GetInfo()
        {
            return
                $"{ProcessName} ({IsRunningString()})"
                + "\r\n" +
                $"1회 사용한도 : {SessionTimeLimit.TotalMinutes}분 (현재 {SessionUsedTime.Minutes}분 {SessionUsedTime.Seconds}초 사용중)"
                + "\r\n" +
                $"하루 사용한도 : {TotalUsedTimeLimit.TotalMinutes}분 (현재 {TotalUsedTime.Minutes}분 {TotalUsedTime.Seconds}초 사용중)"
                + "\r\n" +
                $"포커스 된 횟수 : xx 회"
                + "\r\n" +
                $"입력한 문자 : xx 자";
        }

        private bool IsProcessRunning()
        {
            return IsAppRunning() || IsWebPageRunning();
        }

        private bool IsAppRunning()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            return processes.Any();
        }

        private bool IsWebPageRunning()
        {
            return WebPageMonitor.IsWebPageFocused(ProcessName);
        }

        private string IsRunningString()
        {
            return IsProcessRunning() ? "사용중" : "꺼짐";
        }

        public void ResetUsedTime()
        {
            SessionUsedTime = new TimeSpan();
            TotalUsedTime = new TimeSpan();
        }
    }
}