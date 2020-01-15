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
using System.Net.Http;
using System.Net;
using System.IO;

namespace FlingoDesktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void backupSql(object sender, RoutedEventArgs e)
        {
            string url = this.urlText.Text.Trim();
            Console.WriteLine(url);
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://www.hbuvt.com/cdn/123456.jpeg");
            httpRequest.Method = WebRequestMethods.Http.Get;
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream httpResponseStream = httpResponse.GetResponseStream();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "backup" + DateTime.Now.ToString("yyyy-MM-dd"); // Default file name
            dlg.DefaultExt = ".sql"; // Default file extension
            dlg.Filter = "Text documents (.sql)|*.sql"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                // Save document
                string filename = dlg.FileName;

                FileStream fileStream = File.Create(filename);
                while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                } // end while

            }
        }
        private void backupStatic(object sender, RoutedEventArgs e)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)
            WebRequest.Create("https://www.hbuvt.com/cdn/123456.jpeg");
            httpRequest.Method = WebRequestMethods.Http.Get;
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream httpResponseStream = httpResponse.GetResponseStream();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "backup" + DateTime.Now.ToString("yyyy-MM-dd"); // Default file name
            dlg.DefaultExt = ".sql"; // Default file extension
            dlg.Filter = "Text documents (.sql)|*.sql"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                // Save document
                string filename = dlg.FileName;

                FileStream fileStream = File.Create(filename);
                while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                } // end while

            }
        }

   
    }
}
