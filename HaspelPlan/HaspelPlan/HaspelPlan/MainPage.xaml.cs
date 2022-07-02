using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows;

namespace HaspelPlan
{
    public partial class MainPage : ContentPage
    {
        public List<String> JobList;
        public MainPage()
        {
            InitializeComponent();
            showPlan();
            BindingContext = new ViewModel.MainViewModel();
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
                        noConnection.IsVisible = true;
                        planView.IsVisible = false;
                        isConnectionSuccessful = false;
                    }
                }
                catch{}      
            }

            if (isConnectionSuccessful)
            {
                noConnection.IsVisible = false;
                updatedTable.IsVisible = true;
                planView.IsVisible = true;
            }
            return pageContent;
        }

        private int getCalendarWeek()
        {
            CultureInfo myCI = new CultureInfo("de-de");
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            return myCal.GetWeekOfYear(DateTime.Now, myCWR, myFirstDOW);
        }

        string removeUnnecessaryRows(string content)
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

        string addHoursToTable(string content)
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

        string adjustTimetable(string content)
        {
            //content = content.Replace("<CENTER>", "");
            //content = content.Replace("</CENTER>", "");
            content = content.Replace("�", "Ä");
            content = content.Replace("<TABLE border=\"3\" rules=\"all\" cellpadding=\"1\" cellspacing=\"1\">", "<TABLE border=\\ \"3\\\" rules=\\ \"all\\\" cellpadding=\\ \"1\\\" cellspacing=\\ \"1\\\" style=\"transform: scale(0.65)!important; transform-origin: top left; margin-left:1%; \">");
            return content;
        }

        private void showPlan()
        {
            int calendarWeek = getCalendarWeek();
            string content = LoadHttpPageWithBasicAuthentication($"http://www.bkah.de/schuelerplan_praesenz/32/c/c00116.htm", "schuelerplan", "schwebebahn");

            content = adjustTimetable(content);
            content = addHoursToTable(content);
            content = removeUnnecessaryRows(content);

            var html = new HtmlWebViewSource
            {
                Html = content
            };
            planView.Source = html;

            Device.StartTimer(new TimeSpan(0, 0, 5), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    updatedTable.IsVisible = false;
                });
                return false;
            });
        }

        // Button-Click "Aktualisieren"
        private void updateTable(object sender, EventArgs e)
        {
            showPlan();
        }
    }
}
