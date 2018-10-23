using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;
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
        private const int TICKNANOSECONDS = 1000;
        private Dictionary<string, TimeSpan> _processRunTimeDic = new Dictionary<string, TimeSpan>();
        private readonly List<string> _monitoringProcesses = new List<string>() { "KakaoTalk" };

        public MainWindow()
        {
            InitializeComponent();
            ShowProcesses();
            StartTimer();
        }

        private void AddProcessToMonitoringList(string processName)
        {
            if (!IsValidProcessName(processName))
            {
                throw new ArgumentException();
            }
            _monitoringProcesses.Add(processName);
        }

        private bool IsValidProcessName(string processName)
        {
            var processes = Process.GetProcesses();
            return processes.Any(x => x.ProcessName == processName);
        }

        private void StartTimer()
        {
            // 윈폼 타이머 사용
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(TICKNANOSECONDS); // 1초
            timer.Tick += new EventHandler(OnTick);
            timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            ShowClock();
            Monitoring();
        }

        private void ShowClock()
        {
            ChattingInfoText.Text = DateTime.Now.ToLongTimeString();
        }

        private void Monitoring()
        {
            if (!_monitoringProcesses.Any())
            {
                return;
            }
            foreach (var monitoringProcess in _monitoringProcesses)
            {
                IncreaseRunTime(monitoringProcess);
            }
        }

        private void IncreaseRunTime(string monitoringProcess)
        {
            if (_processRunTimeDic.ContainsKey(monitoringProcess))
            {
                _processRunTimeDic[monitoringProcess] += new TimeSpan(TICKNANOSECONDS);
            }
            else
            {
                _processRunTimeDic.Add(monitoringProcess, new TimeSpan(TICKNANOSECONDS));
            }
        }

        private void ShowProcesses()
        {
            var processes = Process.GetProcesses();
            ProcessText.Text = string.Join("\r\n", processes.Select(x => x.ProcessName));

            var listBox = new ListBox();
            ProcessList.Content = listBox;
            listBox.DisplayMemberPath = "Name";
            foreach (var process in processes)
            {
                listBox.Items.Add(new MyObject { Name = process.ProcessName });
            }
        }

        public class MyObject
        {
            public string Name { get; set; }
        }

        private void KillKakaoTalk_Click(object sender, RoutedEventArgs e)
        {
            KillKakaoTalkProcess();
        }

        private static void KillKakaoTalkProcess()
        {
            var kakaoProcess = Process.GetProcessesByName("KakaoTalk");
            foreach (var process in kakaoProcess)
            {
                process.Kill();
            }
        }
    }
}