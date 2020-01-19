using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VKPomodoro
{
    [Serializable]
    public class PomodoroSaveData
    {
        public Dictionary<string, int> ResultDictionary = new Dictionary<string, int>();

        public string Serialize()
        {
            return JsonConvert.SerializeObject(ResultDictionary, Formatting.Indented);
        }

        public void DeSerialize(string json)
        {
             ResultDictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        }

        public bool TryGetTodayPomodoroCount(out int found)
        {
            return ResultDictionary.TryGetValue(GetDateString(), out found);
        }

        public static string GetDateString()
        {
            return $"{DateTime.Now.ToShortDateString()}";
        }
    }
}