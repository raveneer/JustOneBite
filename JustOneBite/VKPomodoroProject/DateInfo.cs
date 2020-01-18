using System;

namespace VKPomodoro
{
    [Serializable]
    public class DateInfo
    {
        public int Day;
        public int Month;
        public int Year;

        public static DateInfo FromDateTime(DateTime dateTime)
        {
            var date = new DateInfo();
            date.Year = dateTime.Year;
            date.Month = dateTime.Month;
            date.Day = dateTime.Day;
            return date;
        }
    }
}