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
            saveData.ResultDictionary = new Dictionary<string, int>(){{PomodoroSaveData.GetDateString(), 1}};
            var serialString = saveData.Serialize();

            Debug.WriteLine(serialString);

            var otherSaveData = new PomodoroSaveData();
            otherSaveData.DeSerialize(serialString);
            Assert.AreEqual(saveData.ResultDictionary.First().Value, otherSaveData.ResultDictionary.First().Value);
            Assert.AreEqual(saveData.ResultDictionary.First().Key, otherSaveData.ResultDictionary.First().Key);
        }
    }

    public class Test_DateInfo
    {
        [Test]
        public void Test_FromDateTime()
        {
            Assert.AreEqual(DateInfo.FromDateTime(new DateTime(2000,1,2)).Year , 2000);
            Assert.AreEqual(DateInfo.FromDateTime(new DateTime(2000,1,2)).Month , 1);
            Assert.AreEqual(DateInfo.FromDateTime(new DateTime(2000,1,2)).Day, 2);
        }

        [Test]
        public void Test_Same()
        {
            Assert.AreEqual(DateInfo.FromDateTime(new DateTime(2000,1,2)), DateInfo.FromDateTime(new DateTime(2000,1,2)));
        }

        [Test]
        public void Test_Serialize()
        {
            var dateInfo = DateInfo.FromDateTime(new DateTime(2000,1,2));
            var serial = JsonConvert.SerializeObject(dateInfo);
            Assert.NotNull(serial);
            var newDateInfo = JsonConvert.DeserializeObject<DateInfo>(serial);
            Assert.AreEqual(dateInfo, newDateInfo);

        }
    }
}