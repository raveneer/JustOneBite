using System;
using System.IO;
using Newtonsoft.Json;

namespace VKPomodoro
{
    public class PomodoroSaveManager
    {
        public static int LoadPomodoroResult()
        {
            if (!File.Exists("SaveData.json"))
            {
                return 0;
            }

            var saveData = JsonConvert.DeserializeObject<PomodoroSaveData>(File.ReadAllText("SaveData.json"));
            if (saveData.TryGetTodayPomodoroCount(out int found))
            {
                return found;
            }
            return 0;
        }

        public static void SavePomodoroResult(int completePomodoroToday)
        {
            //없으면 만들고 끝냄
            if (!File.Exists("SaveData.json"))
            {
                var stream = File.Create("SaveData.json");
                stream.Close();
                var thisSessionSaveData = new PomodoroSaveData();
                thisSessionSaveData.ResultDictionary.Add(PomodoroSaveData.GetDateString(), completePomodoroToday);
                var saveDataString = JsonConvert.SerializeObject(thisSessionSaveData);
                File.WriteAllText("SaveData.json", saveDataString);
                return;
            }

            //파일이 있으면 기존 데이터를 읽어와서 수정사항을 저장함.
            var saveData = JsonConvert.DeserializeObject<PomodoroSaveData>(File.ReadAllText("SaveData.json"));
            if (saveData.ResultDictionary.ContainsKey(PomodoroSaveData.GetDateString()))
            {
                saveData.ResultDictionary[PomodoroSaveData.GetDateString()] = completePomodoroToday;
                var saveDataString = JsonConvert.SerializeObject(saveData);
                File.WriteAllText("SaveData.json", saveDataString);
            }
        }
    }
}