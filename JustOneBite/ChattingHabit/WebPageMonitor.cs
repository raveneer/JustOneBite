using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace ChattingHabit
{
    public class WebPageMonitor
    {
        private static string _currentFocusedWebPage = "";

        public static bool IsWebPageFocused(string pageName)
        {
            return _currentFocusedWebPage.Contains(pageName);
        }

        public void Tick()
        {
            var page = GetFocusedChromeURL();
            _currentFocusedWebPage = page;
        }

        /// <summary>
        /// 이 연산은 상당히 무거우므로 자주 돌려서는 안됨.
        /// </summary>
        public string GetFocusedChromeURL()
        {
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            foreach (Process chrome in procsChrome)
            {
                if (!string.IsNullOrWhiteSpace(GetProcessURL(chrome)))
                {
                    return GetProcessURL(chrome);
                }
            }
            return "";
        }

        /// <summary>
        /// 이 연산은 상당히 무거우므로 자주 돌려서는 안됨.
        /// </summary>
        public async Task<string> GetFocusedChromeURLAsync()
        {
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            foreach (Process chrome in procsChrome)
            {
                if (!string.IsNullOrWhiteSpace(GetProcessURL(chrome)))
                {
                    /*var task1 = Task.Run(() => LongCalcAsync(10));
                    // task1이 끝나길 기다렸다가 끝나면 결과치를 sum에 할당
                    int sum = await task1;*/

                    var task1 = Task.Run(() => GetProcessURL(chrome));

                    // task1이 끝나길 기다렸다가 끝나면 결과치를 sum에 할당
                    string result = await task1;
                    return result;
                }
            }
            return "";
        }

        /// <summary>
        /// 웹페이지는 kill 을 하면 모든 탭을 닫아버리므로 close 한다.
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

        public void Debug_ChangeOpenedURL(string str)
        {
            _currentFocusedWebPage = str;
        }

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
            var url = ((ValuePattern)elementx.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
            return url;
        }
    }
}