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
            ReadSelectedValue();
            FillCalendarWeekList();
            ShowPlan();
        }

        private string classValueFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "classValue.txt");
        private HtmlWebViewSource _planHtml;
        private int _selectedCalendarWeek;
        private string _selectedClass;
        private string _selectedValue;
        private bool _noConnection = false;
        private bool _planViewVisibility = true;
        private bool _updatedTable = false;
        private List<int> _calendarWeeks = new List<int> { };
        private List<string> _dropdownOptions { get; set; } = new List<string>
        {
            "ITU1", "ITU2", "ITU3", "ITU4", "ITM1", "ITM2", "ITM3", "ITM4", "ITO1", "ITO2", "ITO3", "ITO4"
        };

        static Dictionary<string, string> classes { get; } = new Dictionary<string, string>
        {
            { "ITU1", "c00124" }, { "ITU2", "c00125" }, { "ITU3", "c00126" }, { "ITU4", "c00127" },
            { "ITM1", "c00116" }, { "ITM2", "c00117" }, { "ITM3", "c00118" }, { "ITM4", "c00119" },
            { "ITO1", "c00120" }, { "ITO2", "c00121" }, { "ITO3", "c00122" }, { "ITO4", "c00123" },
        };

        public HtmlWebViewSource planHtml { get { return _planHtml; } set { _planHtml = value; NotifyPropertyChanged(); } }
        public List<string> DropdownOptions { get { return _dropdownOptions; } set { _dropdownOptions = value; } }
        public bool noConnection { get { return _noConnection; } set { _noConnection = value; NotifyPropertyChanged(); } }
        public bool planViewVisibility { get { return _planViewVisibility; } set { _planViewVisibility = value; NotifyPropertyChanged(); } }
        public bool updatedTable { get { return _updatedTable; } set { _updatedTable = value; NotifyPropertyChanged(); } }
        public int selectedCalendarWeek { get { return _selectedCalendarWeek; } set { _selectedCalendarWeek = value; NotifyPropertyChanged(); } }
        public List<int> calendarWeeks { get { return _calendarWeeks; } set { _calendarWeeks = value; NotifyPropertyChanged(); } }
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
            int calendarWeek = GetCalendarWeek();
            calendarWeeks.Add(calendarWeek);
            calendarWeeks.Add(calendarWeek + 1);
            calendarWeeks.Add(calendarWeek + 2);
            calendarWeeks.Add(calendarWeek + 3);
            selectedCalendarWeek = calendarWeek;

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

        private string RemoveUnnecessaryRows(string content)
        {
            // Nicht benötigte Flächen entfernen
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>9</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>10</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>11</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>12</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>13</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>14</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>15</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            content = content.Replace("<TR>\n<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\n<B>16</B>\n</font> </TD>\n</TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>\n</TR><TR>\n</TR>", "");
            return content;
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
            string content = LoadHttpPageWithBasicAuthentication($"http://www.bkah.de/schuelerplan_praesenz/{selectedCalendarWeek}/c/{_selectedValue}.htm", "schuelerplan", "schwebebahn");

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
    }
}
