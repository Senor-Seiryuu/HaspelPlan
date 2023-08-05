using HaspelPlan.ViewModelHaspelPlan.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace HaspelPlan.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            UpdateCommand = new DelegateCommand(x => UpdateTable());
            GetClassFrames();
            ReadSelectedValue();
            FillCalendarWeekList();
            ShowPlan();
        }

        private string classValueFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "classValue.txt");
        private string classFrameFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "classFrame.txt");
        private HtmlWebViewSource _planHtml;
        private string _selectedCalendarWeek;
        private string _selectedClass;
        private string _selectedValue;
        private bool _noConnection = false;
        private bool _planViewVisibility = true;
        private bool _updatedTable = false;
        private List<string> _calendarWeeks = new List<string> { };
        private List<string> _dropdownOptions { get; set; } = new List<string>
        {
            "ITU1", "ITU2", "ITU3", "ITU4", "ITM1", "ITM2", "ITM3", "ITM4", "ITO1", "ITO2", "ITO3", "ITO4"
        };

        static Dictionary<string, string> classes { get; } = new Dictionary<string, string>
        {
            { "ITU1", "" }, { "ITU2", "" }, { "ITU3", "" }, { "ITU4", "" },
            { "ITM1", "" }, { "ITM2", "" }, { "ITM3", "" }, { "ITM4", "" },
            { "ITO1", "" }, { "ITO2", "" }, { "ITO3", "" }, { "ITO4", "" }
        };

        public HtmlWebViewSource planHtml { get { return _planHtml; } set { _planHtml = value; NotifyPropertyChanged(); } }
        public List<string> DropdownOptions { get { return _dropdownOptions; } set { _dropdownOptions = value; } }
        public bool noConnection { get { return _noConnection; } set { _noConnection = value; NotifyPropertyChanged(); } }
        public bool planViewVisibility { get { return _planViewVisibility; } set { _planViewVisibility = value; NotifyPropertyChanged(); } }
        public bool updatedTable { get { return _updatedTable; } set { _updatedTable = value; NotifyPropertyChanged(); } }
        public string selectedCalendarWeek { get { return _selectedCalendarWeek; } set { _selectedCalendarWeek = value; NotifyPropertyChanged(); } }
        public List<string> calendarWeeks { get { return _calendarWeeks; } set { _calendarWeeks = value; NotifyPropertyChanged(); } }
        public string selectedClass
        {
            get { return _selectedClass; }
            set
            {
                _selectedClass = value;
                NotifyPropertyChanged();
                SetSelectedValue();
                CacheSelectedValue();
            }
        }

        public ICommand UpdateCommand { get; set; }

        private void FillCalendarWeekList()
        {
            CultureInfo CI = new CultureInfo("de-de");
            Calendar cal = CI.Calendar;
            DateTime startOfWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);

            calendarWeeks.Add(cal.GetDayOfMonth(startOfWeek) + "." + cal.GetMonth(startOfWeek) + "." + cal.GetYear(startOfWeek) + " - " + cal.GetDayOfMonth(startOfWeek.AddDays(6)) + "." + cal.GetMonth(startOfWeek.AddDays(6)) + "." + cal.GetYear(startOfWeek.AddDays(6)));
            calendarWeeks.Add(cal.GetDayOfMonth(startOfWeek.AddDays(7)) + "." + cal.GetMonth(startOfWeek.AddDays(7)) + "." + cal.GetYear(startOfWeek.AddDays(7)) + " - " + cal.GetDayOfMonth(startOfWeek.AddDays(13)) + "." + cal.GetMonth(startOfWeek.AddDays(13)) + "." + cal.GetYear(startOfWeek.AddDays(13)));
            calendarWeeks.Add(cal.GetDayOfMonth(startOfWeek.AddDays(14)) + "." + cal.GetMonth(startOfWeek.AddDays(14)) + "." + cal.GetYear(startOfWeek.AddDays(14)) + " - " + cal.GetDayOfMonth(startOfWeek.AddDays(20)) + "." + cal.GetMonth(startOfWeek.AddDays(20)) + "." + cal.GetYear(startOfWeek.AddDays(20)));
            calendarWeeks.Add(cal.GetDayOfMonth(startOfWeek.AddDays(21)) + "." + cal.GetMonth(startOfWeek.AddDays(21)) + "." + cal.GetYear(startOfWeek.AddDays(21)) + " - " + cal.GetDayOfMonth(startOfWeek.AddDays(27)) + "." + cal.GetMonth(startOfWeek.AddDays(27)) + "." + cal.GetYear(startOfWeek.AddDays(27)));
            selectedCalendarWeek = calendarWeeks[0];
        }

        private void SetSelectedValue()
        {
            if (string.IsNullOrEmpty(selectedClass)) return;
            _selectedValue = classes[selectedClass];
        }

        private void CacheSelectedValue()
        {
            try
            {
                File.WriteAllText(classValueFile, _selectedValue);
            }
            catch { }
        }

        private void ReadSelectedValue()
        {
            try
            {
                if (!File.Exists(classValueFile)) throw new InvalidOperationException("Cannot initialize because classValue-File does not exist.");
                _selectedValue = File.ReadAllText(classValueFile);

                if (_selectedValue == "") throw new InvalidOperationException("Cannot initialize because string is empty.");
                selectedClass = classes.FirstOrDefault(x => x.Value == _selectedValue).Key;
            }
            catch (InvalidOperationException)
            {
                selectedClass = DropdownOptions[4];
                _selectedValue = classes[selectedClass];
            }
        }

        private string LoadHttpPageWithBasicAuthentication(string url, string username, string password)
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cache.txt");
            string pageContent = "";
            bool isConnectionSuccessful = false;
            Uri myUri = new Uri(url);
            WebRequest myWebRequest = HttpWebRequest.Create(myUri);

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;
            myHttpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:54.0) Gecko/20100101 Firefox/54.0";
            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebRequest.AllowAutoRedirect = true;
            myHttpWebRequest.Timeout = 100000;

            NetworkCredential myNetworkCredential = new NetworkCredential(username, password);

            CredentialCache myCredentialCache = new CredentialCache();
            myCredentialCache.Add(myUri, "Basic", myNetworkCredential);

            myHttpWebRequest.PreAuthenticate = true;
            myHttpWebRequest.Credentials = myCredentialCache;

            try
            {
                WebResponse myWebResponse = myWebRequest.GetResponse();
                Stream responseStream = myWebResponse.GetResponseStream();

                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

                pageContent = myStreamReader.ReadToEnd();

                responseStream.Close();
                myWebResponse.Close();
                try
                {
                    File.WriteAllText(fileName, pageContent);
                }
                catch { }
                isConnectionSuccessful = true;
            }
            catch
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        pageContent = File.ReadAllText(fileName);
                    }
                    else
                    {
                        noConnection = true;
                        planViewVisibility = false;
                        isConnectionSuccessful = false;
                    }
                }
                catch { }
            }

            if (isConnectionSuccessful)
            {
                noConnection = false;
                updatedTable = true;
                planViewVisibility = true;
            }
            return pageContent;
        }

        private int GetCalendarWeek()
        {
            CultureInfo myCI = new CultureInfo("de-de");
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            return myCal.GetWeekOfYear(DateTime.Now, myCWR, myFirstDOW);
        }

        private int GetSelectedCalendarWeekAsInt()
        {
            int calendarWeek = GetCalendarWeek();

            for (int i = 0; i < calendarWeeks.Count(); i++)
            {
                if (selectedCalendarWeek == calendarWeeks[i]) return calendarWeek + i;
            }
            return calendarWeek;
        }

        private string RemoveUnnecessaryRows(string content)
        {
            // Nicht benötigte Flächen entfernen
            int startingIndex = content.IndexOf("<TR>\r\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\r\n<B>9</B>");
            int endingIndex = content.IndexOf("</TABLE><TABLE");
            return content.Remove(startingIndex, endingIndex - startingIndex);
        }

        private string AddHoursToTable(string content)
        {
            //Stundenzeiten hinzufügen
            content = content.Replace("  color=\"#000000\"", "");
            content = content.Replace("<TD align=\"center\"><TABLE><TR><TD></TD></TR></TABLE></TD>", "<TD align=\"center\"><TABLE><TR><TD>Stunden</TD></TR></TABLE></TD>");
            // Stunden 1 und 2
            content = content.Replace("<B>1</B>", "<B>1 / 07:30 - 08:15</B>");
            content = content.Replace("<B>2</B>", "<B>2 / 08:15 - 09:00</B>");
            // Stunden 3 und 4
            content = content.Replace("<B>3</B>", "<B>3 / 09:15 - 10:00</B>");
            content = content.Replace("<B>4</B>", "<B>4 / 10:00 - 10:45</B>");
            // Stunden 5 und 6
            content = content.Replace("<B>5</B>", "<B>5 / 11:05 - 11:50</B>");
            content = content.Replace("<B>6</B>", "<B>6 / 11:50 - 12:35</B>");
            // Stunden 7 und 8
            content = content.Replace("<B>7</B>", "<B>7 / 12:50 - 13:35</B>");
            content = content.Replace("<B>8</B>", "<B>8 / 13:35 - 14:20</B>");
            return content;
        }

        private string AdjustTimetable(string content)
        {
            content = content.Replace("�", "Ä");
            content = content.Replace("<TABLE border=\"3\" rules=\"all\" cellpadding=\"1\" cellspacing=\"1\">", "<TABLE border=\\ \"3\\\" rules=\\ \"all\\\" cellpadding=\\ \"1\\\" cellspacing=\\ \"1\\\" style=\"transform: scale(0.65)!important; transform-origin: top left; margin-left:1%; \">");
            return content;
        }

        private void ShowPlan()
        {
            string link = $"http://www.bkah.de/schuelerplan_praesenz/{GetSelectedCalendarWeekAsInt()}/c/c{_selectedValue}.htm";
            string content = LoadHttpPageWithBasicAuthentication(link, "schuelerplan", "schwebebahn");

            content = AdjustTimetable(content);
            content = AddHoursToTable(content);
            content = RemoveUnnecessaryRows(content);

            var html = new HtmlWebViewSource
            {
                Html = content
            };
            planHtml = html;

            Device.StartTimer(new TimeSpan(0, 0, 5), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    updatedTable = false;
                });
                return false;
            });
        }

        // Button-Click "Aktualisieren"
        private void UpdateTable()
        {
            ShowPlan();
        }

        private void GetClassFrames()
        {
            string link = "";
            string content;
            string cf = "";
            try
            {
                if (!File.Exists(classFrameFile)) throw new InvalidOperationException("Cannot initialize because classFrame-File does not exist.");

                cf = File.ReadAllText(classFrameFile);
                link = $"http://www.bkah.de/schuelerplan_praesenz/{GetCalendarWeek()}/c/c{cf}.htm";
                content = LoadHttpPageWithBasicAuthentication(link, "schuelerplan", "schwebebahn");

                if (!content.ToLower().Contains("itm1")) throw new InvalidOperationException("classFrame-string does not match the current class frames.");

                UpdateClassesDict(cf);
            }
            catch (InvalidOperationException)
            {
                int cWeek = GetCalendarWeek();
                for(int i; cWeek < 55; cWeek++)
                {
                    link = $"http://www.bkah.de/schuelerplan_praesenz/{cWeek}/c/c{FillDigits(2)}.htm";
                    content = LoadHttpPageWithBasicAuthentication(link, "schuelerplan", "schwebebahn");
                    if (!String.IsNullOrEmpty(content)) break;
                }

                for (int i = 1; i < 150; i++)
                {
                    cf = FillDigits(i);
                    link = $"http://www.bkah.de/schuelerplan_praesenz/{cWeek}/c/c{cf}.htm";
                    content = LoadHttpPageWithBasicAuthentication(link, "schuelerplan", "schwebebahn");
                    if (content.ToLower().Contains("itm1"))
                    {
                        break;
                    }
                }

                UpdateClassesDict(cf);
                CacheClassFrame(cf);
            }
        }

        private void UpdateClassesDict(string classFrame)
        {
            for (int i = 1; i <= 4; i++)
            {
                classes[$"ITM{i}"] = FillDigits(int.Parse(classFrame) + (i - 1));
                classes[$"ITO{i}"] = FillDigits((int.Parse(classFrame) + 4) + (i - 1));
                classes[$"ITU{i}"] = FillDigits((int.Parse(classFrame) + 8) + (i - 1));
            }
        }

        private string FillDigits(int classFrame)
        {
            string cfBuffer = classFrame.ToString();
            for (int i = 0; i < 4; i++)
            {
                if (cfBuffer.Length != 5) cfBuffer = "0" + cfBuffer;
            }
            return cfBuffer;
        }

        private void CacheClassFrame(string classFrame)
        {
            try
            {
                File.WriteAllText(classFrameFile, classFrame);
            }
            catch { }
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
