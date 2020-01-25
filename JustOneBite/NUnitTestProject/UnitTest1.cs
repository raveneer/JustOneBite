using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using VKPomodoro;

namespace Tests
{
    public class Test_PomodoroResultSaveData
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var saveData = new PomodoroSaveData();
            saveData.ResultDictionary = new Dictionary<string, int>(){{PomodoroSaveData.GetTodayString(), 1}};
            var serialString = saveData.Serialize();

            Debug.WriteLine(serialString);

            var otherSaveData = new PomodoroSaveData();
            otherSaveData.DeSerialize(serialString);
            Assert.AreEqual(saveData.ResultDictionary.First().Value, otherSaveData.ResultDictionary.First().Value);
            Assert.AreEqual(saveData.ResultDictionary.First().Key, otherSaveData.ResultDictionary.First().Key);
        }

        [Test]
        public void Test_TryGetTodayPomodoroCount()
        {
            var saveData = new PomodoroSaveData {ResultDictionary = new Dictionary<string, int>() {{DateTime.Now.ToShortDateString(), 1}}};
            saveData.TryGetTodayPomodoroCount(out var found);
            Assert.AreEqual(1, found);
        }
    }

}