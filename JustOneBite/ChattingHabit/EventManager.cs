using System;

namespace ChattingHabit
{
    public static class EventManager
    {
        public static Action<MonitoringProcess> ProcessSessionLimitReached;

        public static void Broadcast_ProcessSessionLimitReached(MonitoringProcess process)
        {
            ProcessSessionLimitReached?.Invoke(process);
        }

        public static Action<MonitoringProcess> ProcessTotalLimitReached;

        public static void Broadcast_ProcessTotalLimitReached(MonitoringProcess process)
        {
            ProcessTotalLimitReached?.Invoke(process);
        }

        public static Action<string> ShowLogMessage;

        public static void Broadcast_ShowLogMessage(string str)
        {
            ShowLogMessage?.Invoke(str);
        }
    }
}