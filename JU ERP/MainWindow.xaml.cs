using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Xml;


namespace JU_ERP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CookieContainer cookies = new CookieContainer(); //Cookies :D
        List<Subjects> Sub = new List<Subjects>(); // wow it worked wow. such magic. no hope.


        public MainWindow()
        {

            InitializeComponent();

            if (Properties.Settings.Default.Autologin)
            {
                string userId = Properties.Settings.Default.Username;
                userIdBox.Text = userId;
                string password = Properties.Settings.Default.Password;
                passBox.Password = password;
                autoLogin.IsChecked = true;
                Login(userId, password);

            }
            else
            {
                string userId;

                if (Properties.Settings.Default.Username != null)
                    userId = Properties.Settings.Default.Username;
                else
                    userId = userIdBox.Text;
                userIdBox.Text = userId;
                string password = passBox.Password;
            }       // Something fishy here; it's gonna bite me later, ain't it.
        }



        public static string TimeStamp()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://erp.jecrcuniversity.edu.in:8084/jecrc_academia/getTimeStampInMilli.do");
            request.Method = "POST";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                string tp = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return tp;
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Unable to connect-timestamp");
                return null;
            }

        }


        private void Login(string userId, string password)
        {

            var request = (HttpWebRequest)WebRequest.Create("http://erp.jecrcuniversity.edu.in:8084/jecrc_academia/LoginCheck.do");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string data = String.Format("userId={0}&login={0}&password={1}&actionType=loginReq&homeUrl=&userType=0&username={0}&password={1}&admin=&sec={2}",userId,GetMD5(password),TimeStamp());

            request.CookieContainer = cookies;

            var dataByte = Encoding.UTF8.GetBytes(data);
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(dataByte, 0, dataByte.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                
                if (responseString.Contains("is incorrect."))
                {
                    MessageBox.Show("User Id or Password is incorrect.");
                }
                else if (responseString.Contains("Welcome"))
                {
                    HelperA();
                }
                
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Unable to connect-login");
                
            }
        }

       

        public string getResponseString()
        {
            
            var request = (HttpWebRequest)WebRequest.Create("http://erp.jecrcuniversity.edu.in:8084/jecrc_academia/viewStudentCourseAttendanceReportWebguru.do");
            request.CookieContainer = cookies;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string data = String.Format("multipleAcademicYearId=5&acadmicYear=2016-2017&multipleTermId=28&termName=JAN-MAY&acadmicYearId=5&term=28");
            
            var dataByte = Encoding.UTF8.GetBytes(data);
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(dataByte, 0, dataByte.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseStr = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseStr;
                
            }
            catch (System.Net.WebException)
            {
                MessageBox.Show("Unable to connect-getATT");
                return null;
            }
        }

        //public int SubNo()        //Why did I write this?
        //{
        //    string responseString = getResponseString();
        //    int startIndex = responseString.LastIndexOf("<tr>\n                        <td>");
        //    int endIndex = responseString.IndexOf("</td>", startIndex);
        //    int subNo = int.Parse(responseString.Substring(endIndex - 2, 2));
        //    return subNo;
        //}
       


        class Subjects
        {
            public int ID { get; set; }
            public string Name{ get; set; }
            public string Code { get; set; }
            public string Instructor { get; set; }
            public int TotalClass { get; set; }
            public int PresentClass { get; set; }
            public int PresentPercent{ get; set; }

            public Subjects(int id, string name, string code, string instructor, int totalClass, int presentClass, int presentPercent)
            {
                ID = id;
                Name = name;
                Code = code;
                Instructor = instructor;
                TotalClass = totalClass;
                PresentClass = presentClass;
                PresentPercent = presentPercent;
            }
        }

        public void Helper(int id, string name,string code,string instructor,int totalClass,int presentClass,int presentPercent)
        {           
            Sub.Add(new Subjects(id, name, code, instructor, totalClass, presentClass, presentPercent));

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                FillData();
            })); // DAMN

        }

        void FillData()
        {
            this.dataGrid.ItemsSource = Sub;
        }

        public void HelperA ()
       
        {
            string name = null;
            string code = null;
            string instructor = null;
            int totalClass = 0;
            int presentClass = 0;
            double t1 = 0;
            int presentPercent = 0;
            int temp = 0;
            int temp1 = 0;
            int id=0;

            string responseString = getResponseString();
            int startIndex = responseString.IndexOf("<table border=\"1\" id= \"courses\">");
            int endIndex = responseString.IndexOf("</table", startIndex);
            responseString = responseString.Substring(startIndex, endIndex - startIndex); //DATA stuff

            int trIndex = responseString.IndexOf("<tr"); // Trimming the string even further to isolate subjects
            while (trIndex != -1)
            {
                ++id;
                    
                int tdIndex = trIndex;
                for (int i = 0; i < 10; i++)
                {

                tdIndex = responseString.IndexOf("<td", tdIndex + 1); // subjects's assets
#region
                switch (i)
                {
                    case 0:
                    case 1:
                    case 7:
                        break;
                    case 2:
                        temp = responseString.IndexOf(">", tdIndex) + 1;
                        temp1 = responseString.IndexOf("</", tdIndex);
                        code = String.Format("{0} ", responseString.Substring(temp, temp1 - temp));
                        break;
                    case 3:
                        temp = responseString.IndexOf(">", tdIndex) + 1;
                        temp1 = responseString.IndexOf("</", tdIndex);
                        name = String.Format("{0} ", responseString.Substring(temp, temp1 - temp));
                        name = name.Replace("&amp;", "&");
                        break;
                    case 4:
                        temp = responseString.IndexOf(">", tdIndex) + 1;
                        temp1 = responseString.IndexOf("</", tdIndex);
                        instructor = (String.Format("{0} ", responseString.Substring(temp, temp1 - temp)));
                        break;
                    case 5:
                        temp = responseString.IndexOf(">", tdIndex) + 1;
                        temp1 = responseString.IndexOf("</", tdIndex);
                        totalClass = int.Parse(String.Format("{0} ", responseString.Substring(temp, temp1 - temp)));
                        break;
                    case 6:
                        temp = responseString.IndexOf(">", tdIndex) + 1;
                        temp1 = responseString.IndexOf("</", tdIndex);
                        presentClass = int.Parse(String.Format("{0} ", responseString.Substring(temp, temp1 - temp)));
                        break;
                    case 8:
                        temp = responseString.IndexOf(">", tdIndex) + 1;
                        temp1 = responseString.IndexOf("</", tdIndex);
                        t1 = Double.Parse(String.Format("{0} ", responseString.Substring(temp, temp1 - temp)));
                        presentPercent = Convert.ToInt16(t1);
                        break;
                    case 9:
                        Helper(id, name, code, instructor, totalClass, presentClass, presentPercent); //BKBC                     
                        break;
                }
                    //
#endregion
                }
                trIndex = responseString.IndexOf("<tr", trIndex + 1);
              }
        
        }

        public static string GetMD5(string value)
        {
            MD5 algorithm = MD5.Create();
            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            string md5 = "";
            for (int i = 0; i < data.Length; i++)
            {
                md5 += data[i].ToString("x2").ToLowerInvariant();
            }
            return md5;
        }

        private void autoLogin_Checked(object sender, RoutedEventArgs e)
        {
            if (autoLogin.IsChecked ?? true)
            {
                Properties.Settings.Default.Autologin = true;
                Properties.Settings.Default.Save();
            }           
        }// Redundant code :/

        private void autoLogin_Unchecked(object sender, RoutedEventArgs e)
        {            
                Properties.Settings.Default.Autologin = false;
                Properties.Settings.Default.Save();           
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(userIdBox.Text) || String.IsNullOrEmpty(passBox.Password))
            {
                MessageBox.Show("Enter Credentials");
            }

            else if (autoLogin.IsChecked.Value)
            {
                Properties.Settings.Default.Password = passBox.Password;
                Properties.Settings.Default.Username = userIdBox.Text;
                Properties.Settings.Default.Save();
            }
            else if (!String.IsNullOrEmpty(userIdBox.Text) || !String.IsNullOrEmpty(passBox.Password))
            {
                string userId = userIdBox.Text;
                string password = passBox.Password;
                Login(userId, password);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e) // Use AcceptedButton?
        {
            if (e.Key == Key.Enter)
            {
                this.goButton_Click(sender, e);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("JU ERP Attendance 0.8 - Pre-Release\n\n\tpratyush997");          
            System.Diagnostics.Process.Start("https://github.com/pratyush997");
        }

    }

}
// pratyush997
//TODO: Fix About.
//TODO: add "classes to attend to reach 75%( or whatever min is) Prolly?
//TODO: Should requests be async?
//TODO: Do Detailed ?
//TODO: XAML looks potato. fix pls.
