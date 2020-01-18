using System;
using System.Collections.Generic;

namespace VKPomodoro
{
    [Serializable]
    public class PomodoroResultSaveData
    {
        public Dictionary<DateInfo, int> ResultDictionary = new Dictionary<DateInfo, int>();
    }
}