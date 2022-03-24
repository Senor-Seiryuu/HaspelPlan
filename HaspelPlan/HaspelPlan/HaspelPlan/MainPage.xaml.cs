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
                planView.IsVisible = true;
            }
            return pageContent;
        }
        private void showPlan()
        {
            int calendarWeek = getCalendarWeek();
            string content = LoadHttpPageWithBasicAuthentication($"http://www.bkah.de/schuelerplan_praesenz/{calendarWeek}/c/c00118.htm", "schuelerplan", "schwebebahn");
            //content = content.Replace("<CENTER>", "");
            //content = content.Replace("</CENTER>", "");
            content = content.Replace("�", "Ä");

            //content = content.Replace("1", "1/ 07:30 - 09:00");
            
            //Stundezeiten hinzufügen
            //Stunden 1 und 2
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>1</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>1 / 07:30 - 08:15</B>\n</font> </TD>\n</TR></TABLE></TD>");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>2</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>2 / 08:15 - 09:00</B>\n</font> </TD>\n</TR></TABLE></TD>");
            // Stunden 3 und 4
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>3</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>3 / 09:15 - 10:00</B>\n</font> </TD>\n</TR></TABLE></TD>");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>4</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>4 / 10:00 - 10:45</B>\n</font> </TD>\n</TR></TABLE></TD>");
            // Stunden 5 und 6
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>5</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>5 / 11:05 - 11:50</B>\n</font> </TD>\n</TR></TABLE></TD>");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>6</B>\n</font> </TD>\n</TR></TABLE></TD>"
                , "<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>6 / 11:50 - 12:35</B>\n</font> </TD>\n</TR></TABLE></TD>");

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
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>8</B>\n</font> </TD>\n</TR></TABLE></TD>", "");
            content = content.Replace("<TD rowspan=2 align=\"center\" nowrap=\"1\"><TABLE><TR><TD align=\"center\" nowrap=1><font size=\"3\" face=\"Arial\">\n<B>7</B>\n</font> </TD>\n</TR></TABLE></TD>", "");

            var html = new HtmlWebViewSource
            {
                Html = content
            };
            planView.Source = html;

        }
        private int getCalendarWeek()
        {
            CultureInfo myCI = new CultureInfo("de-de");
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            return myCal.GetWeekOfYear(DateTime.Now, myCWR, myFirstDOW);
        }
    }
}
