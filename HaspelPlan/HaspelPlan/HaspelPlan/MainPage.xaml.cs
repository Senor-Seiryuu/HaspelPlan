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
        public MainPage()
        {
            InitializeComponent();
            showPlan();
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
            content = content.Replace("<TD colspan=12 rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD></TD></TR></TABLE></TD>", "");
            content = content.Replace("<TD colspan=12 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\"  color=\"#000000\">\nMontag\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD colspan=12 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\nDienstag\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD colspan=12 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\nMittwoch\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD colspan=12 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"4\" face=\"Arial\">\nFreitag\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>16</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>15</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>14</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>13</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>12</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>11</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>10</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>9</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>2</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>1</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            return content;
        }

        string addHoursToTable(string content)
        {
            //Stundenzeiten hinzufügen
            content = content.Replace("<TD align=\"center\"><TABLE><TR><TD></TD></TR></TABLE></TD>", "<TD align=\"center\"><TABLE><TR><TD>Stunden</TD></TR></TABLE></TD>");
            // Stunden 3 und 4
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>3</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>1 / 09:15 - 10:00</B>\n</font> </TD>\n</TR></TABLE></TD>");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>4</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>2 / 10:00 - 10:45</B>\n</font> </TD>\n</TR></TABLE></TD>");
            // Stunden 5 und 6
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>5</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>3 / 11:05 - 11:50</B>\n</font> </TD>\n</TR></TABLE></TD>");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>6</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>4 / 11:50 - 12:35</B>\n</font> </TD>\n</TR></TABLE></TD>");
            //Stunden 7 und 8
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>7</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>5 / 12:50 - 13:35</B>\n</font> </TD>\n</TR></TABLE></TD>");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>8</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>6 / 13:35 - 14:20</B>\n</font> </TD>\n</TR></TABLE></TD>");
            return content;
        }

        private void showPlan()
        {
            int calendarWeek = getCalendarWeek();
            string content = LoadHttpPageWithBasicAuthentication($"http://www.bkah.de/schuelerplan_praesenz/{calendarWeek}/c/c00121.htm", "schuelerplan", "schwebebahn");

            content = content.Replace("�", "Ä");
            content = addHoursToTable(content);
            content = removeUnnecessaryRows(content);

            var html = new HtmlWebViewSource
            {
                Html = content
            };
            planView.Source = html;

            Device.StartTimer(new TimeSpan(0, 0, 15), () =>
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
