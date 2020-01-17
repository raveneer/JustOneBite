using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace ChattingHabit
{
    /// <summary>
    ///     아주 간단한 채팅 프로그램 제어 툴.
    ///     이 프로그램은 지정된 프로그램에 대해 다음 사항을 수행한다.
    ///     1. 사용자는 어떤 프로그램을 관리할지를 선택 또는 명령하여 지정할 수 있다. 기본은 카톡, 슬랙, 디스코드.
    ///     2. 사용자는 1회의 사용시간과 하루의 사용시간 제한을 지정할 수 있다. 기본은 10분, 1시간.
    ///     3. 해당 프로그램이 켜지면, 지정한 시간 후에 강제로 꺼버림. (세션 타임 컨트롤)
    ///     4. 해당 프로그램의 하루 사용시간을 초과하면, 해당 프로그램이 켜지면 1초 뒤 꺼버림. (토탈 타임 컨트롤)
    ///     5. 사용자가 해당 프로그램을 closeMainWindow 한 상태면 사용시간에 집계하지는 않는다. 그러나 지정한 세션타임이 되면 끔.
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int AutoSaveTermSec = 10;
        public static int BaseMethodsUpdateSecPerLoop = 1;
        public static int SessionTimeLimitMinute;
        public static int TotalTimeLimitMinute;
        private static int PomodoroSec = 5;
        private static readonly string SaveFileName = "ChattingHabitSave.Json";
        public int SiteMonitorSecPerLoop = 5;
        private int _completePomodoroToday;
        private SoundPlayer _finishSound;
        private bool _isFirstRunningInDay;
        private bool _isPomodoroRunning;
        private DateTime _nextResetTime;
        private TimeSpan _pomodoroRestTime;
        private ProcessCollection _processCollection;
        private readonly string _saveFilePath = AppDomain.CurrentDomain.BaseDirectory + SaveFileName;
        private SoundPlayer _startSound;
        private readonly string _systemSaveFilePath = AppDomain.CurrentDomain.BaseDirectory + "ChattingHabitSystemSave.Json";
        private readonly WebPageMonitor _webPageMonitor = new WebPageMonitor();
        private string[] blockSites;

        public MainWindow()
        {
            EventManager.ShowLogMessage += msg => LogText.Text = msg;

            InitializeComponent(); // WPF 자체함수. 건드리지 말 것.
            InitBlockSiteList();
            InitPomodoroSetting();
            InitSounds();
            LoadPomodoroResult();
            StartUpdateEventLoop();
            SiteMonitorLoop();
            StartAutoSaveLoop();
            StopPomodoro();
        }


        private void StartUpdateEventLoop()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, BaseMethodsUpdateSecPerLoop);
            timer.Tick += BaseMethodsUpdate;
            timer.Start();
        }

        private void StartAutoSaveLoop()
        {
            var autoSaveTimer = new DispatcherTimer();
            autoSaveTimer.Interval = new TimeSpan(0, 0, AutoSaveTermSec);
            autoSaveTimer.Tick += (sender, args) => SaveDataToFile();
            autoSaveTimer.Start();
        }

        private void StartPomodoro()
        {
            _isPomodoroRunning = true;
            _pomodoroRestTime = TimeSpan.FromSeconds(PomodoroSec);
            PomodoroButton.Foreground = Brushes.Red;
            _startSound.Play();

            //시작시 1회 돌려준다.
            CheckPomodoroComplete();
        }

        /// <summary>
        ///     가벼운 연산들. 자주 돌린다.
        /// </summary>
        private async void BaseMethodsUpdate(object sender, EventArgs e)
        {
            //IfTimeOverResetUsedTime();
            //_processCollection.Tick();
            //_webPageMonitor.Tick();
            //ManagingProcessInfoText.Text = _processCollection.GetProcessesInfo();
            //걍 불러버려
            GetUrlAndBlockAsync(blockSites);
            CheckPomodoroComplete();
            RefreshTodayState();
        }

        private void InitPomodoroSetting()
        {
            string path = "Setting.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("Write your pomodoro time sec");
                    sw.WriteLine("5");
                }
            }

            if (int.TryParse(File.ReadAllLines(path)[1], out var sec))
            {
                PomodoroSec = sec;
            }
            else
            {
                PomodoroSec = 25 * 60;
            }
        }

        private void InitBlockSiteList()
        {
            string path = "BlockSiteUrl.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("writeYourBlockSites");
                    sw.WriteLine("dcinside.com");
                }
            }

            blockSites = File.ReadAllLines(path);
        }

        private void InitSounds()
        {
            _finishSound = new SoundPlayer(Properties.Resources.End);
            _finishSound.Load();
            _startSound = new SoundPlayer(Properties.Resources.Start);
            _startSound.Load();
        }

        private void OnClick_PomodoroButton(object sender, RoutedEventArgs e)
        {
            if (_isPomodoroRunning)
            {
                StopPomodoro();
            }
            else
            {
                StartPomodoro();
            }
        }

        private void CheckPomodoroComplete()
        {
            if (_isPomodoroRunning)
            {
                //뽀모도로 틱 갱신
                _pomodoroRestTime -= TimeSpan.FromSeconds(BaseMethodsUpdateSecPerLoop);
                PomodoroButton.Content = $"{_pomodoroRestTime.Minutes} : {_pomodoroRestTime.Seconds}";

                //달성
                if (_pomodoroRestTime <= TimeSpan.Zero)
                {
                    StopPomodoro();
                    CompletePomodoro();
                }
            }
            else
            {
                LogText.Text = "";
            }
        }

        private void CompletePomodoro()
        {
            _finishSound.Play();
            _completePomodoroToday += 1;
            SavePomodoroResult();
        }

        private async void GetUrlAndBlockAsync(string[] blockSites)
        {
            if (!blockSites.Any())
            {
                return;
            }

            if (!_isPomodoroRunning)
            {
                return;
            }

            var task = await Task.Run(() => _webPageMonitor.GetFocusedChromeURLAsync());

            if (blockSites.Any(site => task.Contains(site)))
            {
                //크롬 창 닫기
                SendKeys.SendWait("^w");
            }
        }

        private void LoadPomodoroResult()
        {
            if (!File.Exists("SaveData.json"))
            {
                _completePomodoroToday = 0;
                return;
            }

            var saveData = JsonConvert.DeserializeObject<PomodoroResultSaveData>(File.ReadAllText("SaveData.json"));
            if (saveData.ResultDictionary.TryGetValue(DateInfo.FromDateTime(DateTime.Now), out var count))
            {
                _completePomodoroToday = count;
            }
            else
            {
                _completePomodoroToday = 0;
            }
        }

        private void SavePomodoroResult()
        {
            //없으면 만들고 끝냄
            if (!File.Exists("SaveData.json"))
            {
                var stream = File.Create("SaveData.json");
                stream.Close();
                var thisSessionSaveData = new PomodoroResultSaveData();
                thisSessionSaveData.ResultDictionary.Add(DateInfo.FromDateTime(DateTime.Now), _completePomodoroToday );
                var saveDataString = JsonConvert.SerializeObject(thisSessionSaveData);
                File.WriteAllText("SaveData.json", saveDataString);
                return;
            }

            //파일이 있으면 기존 데이터를 읽어와서 수정사항을 저장함.
            var saveData = JsonConvert.DeserializeObject<PomodoroResultSaveData>(File.ReadAllText("SaveData.json"));
            if (saveData.ResultDictionary.ContainsKey(DateInfo.FromDateTime(DateTime.Now)))
            {
                saveData.ResultDictionary[DateInfo.FromDateTime(DateTime.Now)] = _completePomodoroToday;
                var saveDataString = JsonConvert.SerializeObject(saveData);
                File.WriteAllText("SaveData.json", saveDataString);
            }
           
        }

        private void RefreshTodayState()
        {
            LogText.Text = $"완료한 뽀모도로 {_completePomodoroToday}회";
        }

        private void SaveDataToFile()
        {
            SaveProcessCollection();
            SaveSystemData();
            
        }

        private void SaveProcessCollection()
        {
            using (var stream = new StreamWriter(File.Open(_saveFilePath, FileMode.Create)))
            {
                var dataJson = JsonConvert.SerializeObject(_processCollection, Formatting.Indented);
                stream.Write(dataJson);
            }
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

        /// <summary>
        ///     무거운 연산이므로 따로 돌린다. 느리게.
        /// </summary>
        private void SiteMonitorLoop()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, SiteMonitorSecPerLoop);
            timer.Tick += (sender, args) => { GetUrlAndBlockAsync(blockSites); };
        }

        private void StopPomodoro()
        {
            PomodoroButton.Content = "START";
            _isPomodoroRunning = false;
            PomodoroButton.Foreground = Brushes.Black;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
    }

    public class ListBoxElem
    {
        public string Name { get; set; }
    }

    public class SystemSettingSaveData
    {
        public DateTime NextResetTime;
        public int SessionTimeLimitMinute;
        public int TotalTimeLimitMinute;

        public static SystemSettingSaveData Default()
        {
            return new SystemSettingSaveData
            {
                SessionTimeLimitMinute = 5, TotalTimeLimitMinute = 60, NextResetTime = DateTime.Now + new TimeSpan(1, 0, 0, 0)
            };
        }
    }

    [Serializable]
    public class PomodoroResultSaveData
    {
        public Dictionary<DateInfo, int> ResultDictionary = new Dictionary<DateInfo, int>();
    }

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