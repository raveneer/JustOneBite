using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Condition = System.Windows.Condition;
using ListBox = System.Windows.Controls.ListBox;

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
        public const int TICKSECONDS = 3;
        private ProcessCollection _processCollection;
        public static int AutoSaveTermSec = 10;
        private const string SaveFileName = "ChattingHabitSave.Json";
        private readonly string _saveFilePath = System.AppDomain.CurrentDomain.BaseDirectory + SaveFileName;
        private readonly string _systemSaveFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "ChattingHabitSystemSave.Json";
        private bool _isFirstRunningInDay;
        public static int SessionTimeLimitMinute;
        public static int TotalTimeLimitMinute;
        private DateTime _nextResetTime;
        private SoundPlayer _timeOverSound = new SoundPlayer();
        private WebPageMonitor _webPageMonitor = new WebPageMonitor();

        public MainWindow()
        {
            EventManager.ShowLogMessage += ChangeFeedBackBoxText;
            EventManager.PlayTimeOverSound += () => _timeOverSound.Play();

            InitSounds();
            InitializeComponent();
            LoadSystemSetting();
            LoadMonitoringProcesses();
            ChangeTotalLimit(TotalTimeLimitMinute);
            ChangeSessionLimit(SessionTimeLimitMinute);
            ShowProcesses();
            StartTick();
            StartAutoSaveTick();
        }

        private void InitSounds()
        {
            //FileStream timeOverSoStream = File.Open(@"TimeOver.wav", FileMode.Open);
            //_timeOverSound = new SoundPlayer(timeOverSoStream);
            //_timeOverSound.Load();
        }

        private void LoadSystemSetting()
        {
            SystemSettingSaveData systemSettingSaveData;
            if (!File.Exists(_systemSaveFilePath))
            {
                systemSettingSaveData = SystemSettingSaveData.Default();
            }
            else
            {
                string json = File.ReadAllText(_systemSaveFilePath);
                systemSettingSaveData = JsonConvert.DeserializeObject<SystemSettingSaveData>(json);
            }

            SessionTimeLimitMinute = systemSettingSaveData.SessionTimeLimitMinute;
            TotalTimeLimitMinute = systemSettingSaveData.TotalTimeLimitMinute;
            _nextResetTime = systemSettingSaveData.NextResetTime;
            ResetHourText.Text = _nextResetTime.Hour.ToString();
            ResetMinText.Text = _nextResetTime.Minute.ToString();
        }

        private void LoadMonitoringProcesses()
        {
            if (File.Exists(_saveFilePath))
            {
                string json = File.ReadAllText(_saveFilePath);
                _processCollection = JsonConvert.DeserializeObject<ProcessCollection>(json);
                if (_isFirstRunningInDay)
                {
                    _processCollection.ResetUsedTime();
                }
            }
            else
            {
                _processCollection = new ProcessCollection();
                _processCollection.Add("KakaoTalk");
                _processCollection.Add("slack");
                _processCollection.Add("discord");
            }
        }

        public static bool IsValidProcessName(string processName)
        {
            var processes = Process.GetProcesses();
            return processes.Any(x => x.ProcessName == processName);
        }

        private void Update(object sender, EventArgs e)
        {
            ShowClock();
            IfTimeOverResetUsedTime();
            _processCollection.Tick();
            _webPageMonitor.Tick();
            ManagingProcessInfoText.Text = _processCollection.GetProcessesInfo();
        }

        private void StartTick()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, TICKSECONDS);
            timer.Tick += Update;
            timer.Start();
        }

        private void StartAutoSaveTick()
        {
            var autoSaveTimer = new System.Windows.Threading.DispatcherTimer();
            autoSaveTimer.Interval = new TimeSpan(0, 0, AutoSaveTermSec);
            autoSaveTimer.Tick += (sender, args) => SaveDataToFile();
            autoSaveTimer.Start();
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

        public void OnClick_ChangeTotalLimitButton(object sender, RoutedEventArgs e)
        {
            if (TryGetNumber(TotalLimitText.Text, out var minute))
            {
                ChangeTotalLimit(minute);
            }
        }

        private void OnClick_ChangeSessionLimitButton(object sender, RoutedEventArgs e)
        {
            if (TryGetNumber(SessionLimitText.Text, out var minute))
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

        private void SaveDataToFile()
        {
            SaveProcessCollection();
            SaveSystemData();
        }

        private void SaveSystemData()
        {
            using (var stream = new StreamWriter(File.Open(_systemSaveFilePath, FileMode.Create)))
            {
                var systemSaveData = new SystemSettingSaveData();
                systemSaveData.TotalTimeLimitMinute = TotalTimeLimitMinute;
                systemSaveData.SessionTimeLimitMinute = SessionTimeLimitMinute;
                systemSaveData.NextResetTime = _nextResetTime;
                var dataJson = JsonConvert.SerializeObject(systemSaveData, Formatting.Indented);
                stream.Write(dataJson);
            }
        }

        private void SaveProcessCollection()
        {
            using (var stream = new StreamWriter(File.Open(_saveFilePath, FileMode.Create)))
            {
                var dataJson = JsonConvert.SerializeObject(_processCollection, Formatting.Indented);
                stream.Write(dataJson);
            }
        }

        private void ChangeFeedBackBoxText(string messeage)
        {
            FeedBackText.Text = messeage;
        }

        private void IfTimeOverResetUsedTime()
        {
            if (DateTime.Now >= _nextResetTime)
            {
                _processCollection.ResetUsedTime();
                _nextResetTime = _nextResetTime + new TimeSpan(1, 0, 0, 0);
                EventManager.ShowLogMessage("사용량이 리셋 되었습니다!");
            }
        }

        private void OnClick_ChangeResetTime(object sender, RoutedEventArgs e)
        {
            if (TryGetNumber(ResetHourText.Text, out var hour) && TryGetNumber(ResetMinText.Text, out var min) && hour <= 24 && min <= 60)
            {
                var resetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, DateTime.Now.Second);
                if (resetTime < DateTime.Now)
                {
                    resetTime += new TimeSpan(1, 0, 0, 0);
                }
                _nextResetTime = resetTime;
                EventManager.ShowLogMessage($"리셋 시간이 변경되었습니다. {_nextResetTime.ToLongTimeString()}");
            }
        }

        private bool TryGetNumber(string text, out int number)
        {
            if (!string.IsNullOrEmpty(text) && text.All(char.IsNumber) && int.Parse(text) >= 1)
            {
                number = int.Parse(text);
                return true;
            }

            EventManager.ShowLogMessage("잘못된 입력입니다. 1 이상 의 숫자를 넣어주세요.");
            number = 0;
            return false;
        }

        private void ChattingInfoText_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void ManagingProcessInfoText_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }

    public class ListBoxElem
    {
        public string Name { get; set; }
    }

    public class SystemSettingSaveData
    {
        public int SessionTimeLimitMinute;
        public int TotalTimeLimitMinute;
        public DateTime NextResetTime;

        public static SystemSettingSaveData Default()
        {
            return new SystemSettingSaveData()
            {
                SessionTimeLimitMinute = 5,
                TotalTimeLimitMinute = 60,
                NextResetTime = DateTime.Now + new TimeSpan(1, 0, 0, 0)
            };
        }
    }
}