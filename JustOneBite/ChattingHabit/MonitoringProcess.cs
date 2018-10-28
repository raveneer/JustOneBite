﻿using System;
using System.Diagnostics;
using System.Linq;

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
        private DateTime _prevTickTime;

        public void Tick()
        {
            var isRunning = IsAppRunning() || IsWebPageRunning();
            if (!isRunning)
            {
                return;
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
            if (_prevTickTime.Equals(null))
            {
                _prevTickTime = DateTime.Now;
            }

            var usedTimeThisTick = DateTime.Now - _prevTickTime;
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