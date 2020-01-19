using System;

namespace VKPomodoro
{
    //제이슨닷넷이 이걸 키로 쓰질 못하네... 그냥 스트링 날짜를 써야겠다.
    [Serializable]
    public struct DateInfo
    {
        public int Day;
        public int Month;
        public int Year;

        public static DateInfo FromDateTime(DateTime dateTime)
        {
            return new DateInfo { Year = dateTime.Year, Month = dateTime.Month, Day = dateTime.Day };
        }
    }
}