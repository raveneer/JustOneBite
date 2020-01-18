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

            var saveData = JsonConvert.DeserializeObject<PomodoroResultSaveData>(File.ReadAllText("SaveData.json"));
            if (saveData.ResultDictionary.TryGetValue(DateInfo.FromDateTime(DateTime.Now), out var count))
            {
                return count;
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
                var thisSessionSaveData = new PomodoroResultSaveData();
                thisSessionSaveData.ResultDictionary.Add(DateInfo.FromDateTime(DateTime.Now), completePomodoroToday);
                var saveDataString = JsonConvert.SerializeObject(thisSessionSaveData);
                File.WriteAllText("SaveData.json", saveDataString);
                return;
            }

            //파일이 있으면 기존 데이터를 읽어와서 수정사항을 저장함.
            var saveData = JsonConvert.DeserializeObject<PomodoroResultSaveData>(File.ReadAllText("SaveData.json"));
            if (saveData.ResultDictionary.ContainsKey(DateInfo.FromDateTime(DateTime.Now)))
            {
                saveData.ResultDictionary[DateInfo.FromDateTime(DateTime.Now)] = completePomodoroToday;
                var saveDataString = JsonConvert.SerializeObject(saveData);
                File.WriteAllText("SaveData.json", saveDataString);
            }
        }
    }
}