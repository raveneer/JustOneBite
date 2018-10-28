using System;
using System.Diagnostics;
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
            _currentFocusedWebPage = GetFocusedChromeURL();
        }

        /// <summary>
        /// 이 연산은 상당히 무거우므로 자주 돌려서는 안됨.
        /// </summary>
        private string GetFocusedChromeURL()
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

        public static void KillFocusedWebPage(string partOfPageName)
        {
            if (IsWebPageFocused(partOfPageName))
            {
                Process[] procsChrome = Process.GetProcessesByName("chrome");
                foreach (Process process in procsChrome)
                {
                    if (GetProcessURL(process).Contains(partOfPageName))
                    {
                        process.Kill();
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