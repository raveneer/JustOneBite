using System;
using ChattingHabit;
using NUnit.Framework;

namespace UnitTestProject2
{
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
