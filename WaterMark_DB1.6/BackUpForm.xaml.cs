using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ServiceProcess;
using System.IO;

namespace WaterMark_DB1._6
{
    /// <summary>
    /// BackUpForm.xaml 的交互逻辑
    /// </summary>
    public partial class BackUpForm : Window
    {
        private string sourcePath;
        private string targetPath;
        private string sourceFileName;
        private string targetFileName;
        private string fileExt;
        private string sqlserverName;

        ServiceController sc;

        public BackUpForm()
        {
            InitializeComponent();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamReader myReader = new StreamReader("config.ini"))
                {
                    sqlserverName = myReader.ReadLine();
                }

                sc = new ServiceController(sqlserverName, ".");
                if (sc.Status == ServiceControllerStatus.Stopped)
                    sc.Start();
            }
            catch
            {
                  MessageBox.Show("SqlServer服务不能重启\n请确定您安装了SQLServer\n并确定您配置了正确的实例名","消息提示");
            }
               
            this.Close();
        }

        private void btnSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamReader myReader = new StreamReader("config.ini"))
                {
                    sqlserverName = myReader.ReadLine();
                }

                sc = new ServiceController(sqlserverName, ".");
                if (sc.Status == ServiceControllerStatus.Running)
                    sc.Stop();
            }
            catch
            {
                MessageBox.Show("不能进行SqlServer备份\n请确定您安装了SQL Server\n并确定您配置了正确的实例名", "消息提示");
            }

            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "mdb or mdf files (*.mdb or *.mdf) | *.mdb;*.mdf";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开源数据库";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                sourcePath = dialogOpenFile.FileName.ToString();
                txtSource.Text = sourcePath;
                sourceFileName = dialogOpenFile.SafeFileName;
                sourcePath = sourcePath.Replace(sourceFileName, "");
                fileExt = sourceFileName.Substring(sourceFileName.LastIndexOf('.') + 1);
            }
        }

        private void btnTarget_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialogOpenFile = new SaveFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = fileExt + " files (*." + fileExt + ") | *." + fileExt;
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "创建目的数据库";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                targetPath = dialogOpenFile.FileName.ToString();
                txtTarget.Text = targetPath;
                targetFileName = dialogOpenFile.SafeFileName;
                targetPath = targetPath.Replace(targetFileName, "");
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (txtSource.Text.Length > 0 && txtTarget.Text.Length > 0)
            {
                string sourceFile = System.IO.Path.Combine(sourcePath, sourceFileName);
                string destFile = System.IO.Path.Combine(targetPath, targetFileName);

                if (!System.IO.Directory.Exists(targetPath))
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                }

                try
                {
                    System.IO.File.Copy(sourceFile, destFile, true);
                    MessageBox.Show("备份成功！", "操作提示");
                }
                catch
                {
                    MessageBox.Show("备份失败！", "操作提示");
                }
            }
            else
            {
                MessageBox.Show("请您先选择路径！", "操作提示");
            }
        }
    }
}
