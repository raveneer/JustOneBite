using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChattingHabit
{
    /// <summary>
    /// 아주 간단한 채팅 프로그램 제어 툴.
    /// 이 프로그램은 지정된 프로그램에 대해 다음 사항을 수행한다.
    /// 1. 사용자는 어떤 프로그램을 관리할지를 선택 또는 명령하여 지정할 수 있다. 기본은 카톡, 슬랙, 디스코드.
    /// 2. 사용자는 1회의 사용시간과 하루의 사용시간 제한을 지정할 수 있다. 기본은 10분, 1시간.
    /// 3. 해당 프로그램이 켜지면, 지정한 시간 후에 강제로 꺼버림. (세션 타임 컨트롤)
    /// 4. 해당 프로그램의 하루 사용시간을 초과하면, 해당 프로그램이 켜지면 1초 뒤 꺼버림. (토탈 타임 컨트롤)
    /// 5. 사용자가 해당 프로그램을 closeMainWindow 한 상태면 사용시간에 집계하지는 않는다. 그러나 지정한 세션타임이 되면 끔.
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int TICKSECONDS = 1;
        private ProcessCollection _processCollection;
        public static int SessionTimeLimitMinute = 5;
        public static int TotalTimeLimitMinute = 60;

        public MainWindow()
        {
            EventManager.ShowLogMessage += ChangeFeedBackBoxText;

            InitializeComponent();
            InitMonitoringProcesses();
            ChangeTotalLimit(TotalTimeLimitMinute);
            ChangeSessionLimit(SessionTimeLimitMinute);
            ShowProcesses();
            StartTimer();
        }

        public static bool IsValidProcessName(string processName)
        {
            var processes = Process.GetProcesses();
            return processes.Any(x => x.ProcessName == processName);
        }

        private void InitMonitoringProcesses()
        {
            _processCollection = new ProcessCollection();
            _processCollection.Add("KakaoTalk");
            _processCollection.Add("slack");
        }

        private void OnTick(object sender, EventArgs e)
        {
            ShowClock();
            _processCollection.Tick();
            ManagingProcessInfoText.Text = _processCollection.GetProcessesInfo();
        }

        private void StartTimer()
        {
            // 윈폼 타이머 사용
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, TICKSECONDS); // 1초
            timer.Tick += OnTick;
            timer.Start();
        }

        private void ShowClock()
        {
            //ManagingProcessInfoText.Text = DateTime.Now.ToLongTimeString();
        }

        private void ShowProcesses()
        {
            var processes = Process.GetProcesses().OrderBy(x => x.ProcessName).ToArray();
            var listBox = new ListBox();
            ProcessList.Content = listBox;
            listBox.DisplayMemberPath = "Name";
            foreach (var process in processes)
            {
                listBox.Items.Add(new ListBoxElem { Name = process.ProcessName });
            }
        }

        private void ChattingInfoText_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ManagingProcessInfoText_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public void OnClick_ChangeTotalLimitButton(object sender, RoutedEventArgs e)
        {
            if (TryGetMinute(TotalLimitText.Text, out var minute))
            {
                ChangeTotalLimit(minute);
            }
        }

        private void OnClick_ChangeSessionLimitButton(object sender, RoutedEventArgs e)
        {
            if (TryGetMinute(SessionLimitText.Text, out var minute))
            {
                ChangeSessionLimit(minute);
            }
        }

        private void ChangeTotalLimit(int minute)
        {
            TotalTimeLimitMinute = minute;
            _processCollection.ChangeAllTotalTimeLimit(minute);
            TotalLimitText.Text = minute.ToString();
            EventManager.ShowLogMessage($"하루 사용시간이 {minute} 분으로 변경되었습니다!");
        }

        private void ChangeSessionLimit(int minute)
        {
            SessionTimeLimitMinute = minute;
            _processCollection.ChangeAllSessionTimeLimit(minute);
            SessionLimitText.Text = minute.ToString();
            EventManager.ShowLogMessage($"1회 사용시간이 {minute}분으로 변경되었습니다!");
        }

        private bool TryGetMinute(string text, out int minute)
        {
            if (!string.IsNullOrEmpty(text) && text.All(char.IsNumber) && int.Parse(text) >= 1)
            {
                minute = int.Parse(text);
                return true;
            }

            EventManager.ShowLogMessage("잘못된 입력입니다. 1 이상 의 숫자를 넣어주세요.");
            minute = 0;
            return false;
        }

        private void ChangeFeedBackBoxText(string messeage)
        {
            FeedBackText.Text = messeage;
        }

        public class ListBoxElem
        {
            public string Name { get; set; }
        }
    }
}