using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace VKPomodoro
{
    public class WebPageMonitor
    {
        private static string _currentFocusedWebPage = "";

        /// <summary>
        ///     웹페이지는 kill 을 하면 모든 탭을 닫아버리므로 close 한다.
        /// </summary>
        public static void CloseFocusedWebPage(string partOfPageName)
        {
            if (IsWebPageFocused(partOfPageName))
            {
                Process[] procsChrome = Process.GetProcessesByName("chrome");
                foreach (Process process in procsChrome)
                {
                    if (GetProcessURL(process).Contains(partOfPageName))
                    {
                        process.CloseMainWindow();
                    }
                }
            }
        }

        /// <summary>
        ///     이 연산은 상당히 무거우므로 자주 돌려서는 안됨.
        /// </summary>
        public string GetFocusedChromeURL()
        {
            IntPtr hwnd = GetForegroundWindow();
            // The foreground window can be NULL in certain circumstances, 
            // such as when a window is losing activation.
            if (hwnd == null)
            {
                return "Unknown";
            }

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            foreach (Process p in Process.GetProcessesByName("chrome"))
            {
                if (p.Id == pid)
                {
                    if (!string.IsNullOrWhiteSpace(GetProcessURL(p)))
                    {
                        return GetProcessURL(p);
                    }
                }
            }

            return "";
        }

        /// <summary>
        ///     이 연산은 상당히 무거우므로 자주 돌려서는 안됨.
        /// </summary>
        public async Task<string> GetFocusedChromeURLAsync()
        {
            IntPtr hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out var pid);

            foreach (Process cromeProcess in Process.GetProcessesByName("chrome"))
            {
                if (cromeProcess.Id == pid)
                {
                    if (!string.IsNullOrWhiteSpace(GetProcessURL(cromeProcess)))
                    {
                        var task1 = Task.Run(() => GetProcessURL(cromeProcess));
                        string result = await task1;
                        return result;
                    }
                }
            }

            return "";


        }

        public static bool IsWebPageFocused(string pageName)
        {
            return _currentFocusedWebPage.Contains(pageName);
        }

        public void Tick()
        {
            var page = GetFocusedChromeURL();
            _currentFocusedWebPage = page;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static string GetProcessURL(Process chrome)
        {
            if (chrome.MainWindowHandle == IntPtr.Zero)
            {
                return "";
            }

            AutomationElement element = AutomationElement.FromHandle(chrome.MainWindowHandle);
            if (element == null)
            {
                return "";
            }

            AndCondition conditions = new AndCondition(
                new PropertyCondition(AutomationElement.ProcessIdProperty, chrome.Id),
                new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                new PropertyCondition(AutomationElement.IsContentElementProperty, true),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
            );

            AutomationElement elementx = element.FindFirst(TreeScope.Descendants, conditions);
            if (elementx == null)
            {
                return "";
            }

            var url = ((ValuePattern) elementx.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
            return url;
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public void Debug_ChangeOpenedURL(string str)
        {
            _currentFocusedWebPage = str;
        }
    }
}