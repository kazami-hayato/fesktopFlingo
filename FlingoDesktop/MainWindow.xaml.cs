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
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.ComponentModel;

namespace FlingoDesktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int currentProgress = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public int CurrentProgress
        {
            get { return currentProgress; }
            set
            {
                currentProgress = value;
                OnPropertyChanged("CurrentProgress");
            }
        }
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



        private void openVideoFolder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.videoFolder.Text = m_Dir;
            m_Dialog.Dispose();
        }

        private void openVideoFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择视频汇总csv文件";
            openFileDialog.Filter = "csv文件|*.csv";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "csv";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.videoFile.Text = openFileDialog.FileName;
            }
            openFileDialog.Dispose();
        }
        //  private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
        private async void genBatchData(object sender, RoutedEventArgs e)
        {
            var video_dir = this.videoFile.Text;
            var files_dir = this.videoFolder.Text;
            Console.WriteLine(video_dir);
            Console.WriteLine(files_dir);
            var rets = ProcessCsv.readCsv(video_dir, files_dir, logText);
            int nums = rets.Count();
            string url = this.urlText.Text.Trim();
            //  Console.WriteLine(nums);
            string extralog = "";
            Action action = () =>
            {

                for (int i = 0; i < nums; ++i)
                {
                    CurrentProgress = 100 * ((i + 1) / nums);
                    ret tempret = rets[i];
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.hbuvt.com/apis/v1/system/courses");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        //tempret.course_id = "test_" + tempret.course_id;
                        string json = JsonConvert.SerializeObject(tempret);
                        streamWriter.Write(json);
                    }
                    try
                    {
                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            Console.WriteLine(result);
                        }
                    }
                    catch (System.Net.WebException ex)
                    {
                        Console.WriteLine(ex);
                        extralog += "\n" + "已经存在:" + tempret.course_id;
                    }
                    Thread.Sleep(100);
                }
            };
            await Task.Run(action);
            this.logText.Text += extralog;
        }

        private void openCatFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择单个课程视频xlsx文件";
            openFileDialog.Filter = "xlsx文件|*.xlsx";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "xlsx";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.catFile.Text = openFileDialog.FileName;
            }
            openFileDialog.Dispose();
        }

        private void openVidFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择视频汇总csv文件";
            openFileDialog.Filter = "csv文件|*.csv";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "csv";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.vidFile.Text = openFileDialog.FileName;
            }
            openFileDialog.Dispose();
        }

        private void genSingleData(object sender, RoutedEventArgs e)
        {

        }

        private void urlText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
