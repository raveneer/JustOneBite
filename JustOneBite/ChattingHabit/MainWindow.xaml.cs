using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace ChattingHabit
{
    /// <inheritdoc />
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
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
            var processed = Process.GetProcesses();
            ProcessText.Text = string.Join("\r\n", processed.Select(x => x.ProcessName));
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