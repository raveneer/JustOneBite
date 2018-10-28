using System;
using NUnit.Framework;
using ChattingHabit;

namespace UnitTestProject
{
    public class TestClass
    {
        [Test]
        public void Test_First()
        {
            Assert.AreEqual(1, 1);
        }
    }

    public class Test_WebPageMonitor
    {
        [Test]
        public void False_WhenNoPageOpened()
        {
            var monitor = new WebPageMonitor();
            Assert.AreEqual(false, WebPageMonitor.IsWebPageFocused("kakaoTalk"));
        }
    }
}