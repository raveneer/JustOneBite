using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace VKPomodoro
{
    public partial class MainWindow : Window
    {
        public static int AutoSaveTermSec = 10;
        public static int BaseMethodsUpdateSecPerLoop = 1;
        private static int PomodoroSec = 5;
        public int SiteMonitorSecPerLoop = 5;
        private string[] _blockSites;
        private int _completePomodoroToday;
        private SoundPlayer _finishSound;
        private bool _isPomodoroRunning;
        private TimeSpan _pomodoroRestTime;
        private SoundPlayer _startSound;
        private readonly string _systemSaveFilePath = AppDomain.CurrentDomain.BaseDirectory + "ChattingHabitSystemSave.Json";
        private readonly WebPageMonitor _webPageMonitor = new WebPageMonitor();

        public MainWindow()
        {
            EventManager.ShowLogMessage += msg => LogText.Text = msg;

            //초기화- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            InitializeComponent(); // WPF 자체함수. 건드리지 말 것.
            InitBlockSiteList();
            InitPomodoroSetting();
            InitSounds();
            _completePomodoroToday = PomodoroSaveManager.LoadPomodoroResult();
            InitPomodoroUI();

            //업데이트- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
            StartUpdateEventLoop();
            SiteMonitorLoop();
        }

        private void StartUpdateEventLoop()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, BaseMethodsUpdateSecPerLoop);
            timer.Tick += BaseMethodsUpdate;
            timer.Start();
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
            GetChromeUrlAndBlockSiteAsync(_blockSites);
            CheckPomodoroComplete();
            RefreshTodayState();
        }

        private void InitPomodoroSetting()
        {
            var path = "Setting.txt";
            if (!File.Exists(path))
            {
                using (var sw = File.AppendText(path))
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
            var path = "BlockSiteUrl.txt";
            if (!File.Exists(path))
            {
                using (var sw = File.AppendText(path))
                {
                    sw.WriteLine("writeYourBlockSites");
                    sw.WriteLine("dcinside.com");
                }
            }

            _blockSites = File.ReadAllLines(path);
        }

        private void InitSounds()
        {
            _finishSound = new SoundPlayer(Properties.Resources.End);
            _finishSound.Load();
            _startSound = new SoundPlayer(Properties.Resources.Start);
            _startSound.Load();
        }

        /// <summary>
        ///     현재는 stop과 의미상 같다. 추후 분리될 수 있음.
        ///     유저가 앱을 켰을 때의 상태를 설정함.
        /// </summary>
        private void InitPomodoroUI()
        {
            StopPomodoro();
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
            PomodoroSaveManager.SavePomodoroResult(_completePomodoroToday);
        }

        private async void GetChromeUrlAndBlockSiteAsync(string[] blockSiteLists)
        {
            if (!blockSiteLists.Any())
            {
                return;
            }

            if (!_isPomodoroRunning)
            {
                return;
            }

            var task = await Task.Run(() => _webPageMonitor.GetFocusedChromeURLAsync());

            if (blockSiteLists.Any(site => task.Contains(site)))
            {
                //크롬 창 강제로 닫기
                //hack : 유저가 단축키를 바꿔놓았으면 작동하지 않을 수도 있음.
                SendKeys.SendWait("^w");
            }
        }

        private void RefreshTodayState()
        {
            LogText.Text = $"완료한 뽀모도로 {_completePomodoroToday}회";
        }

        /// <summary>
        ///     무거운 연산이므로 따로 돌린다. 느리게.
        /// </summary>
        private void SiteMonitorLoop()
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, SiteMonitorSecPerLoop);
            timer.Tick += (sender, args) => { GetChromeUrlAndBlockSiteAsync(_blockSites); };
        }

        private void StopPomodoro()
        {
            PomodoroButton.Content = "START";
            _isPomodoroRunning = false;
            PomodoroButton.Foreground = Brushes.Black;
        }

        private bool TryGetNumberFromUserInputString(string text, out int number)
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
}