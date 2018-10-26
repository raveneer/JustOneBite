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

        public static MonitoringProcess GetNewProcess(Process process, int sessionTimeLimit, int totalTimeLimit)
        {
            return new MonitoringProcess
            {
                ProcessName = process.ProcessName,
                SessionTimeLimit = new TimeSpan(0, sessionTimeLimit, 0),
                TotalUsedTimeLimit = new TimeSpan(0, totalTimeLimit, 0)
            };
        }

        public void KillProcess()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            foreach (var process in processes)
            {
                process.Kill();
            }

            SessionUsedTime = new TimeSpan(0);
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

        private bool IsRunning()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            return processes.Any();
        }

        private string IsRunningString()
        {
            return IsRunning() ? "사용중" : "꺼짐";
        }
    }
}