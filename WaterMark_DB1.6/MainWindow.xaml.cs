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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace WaterMark_DB1._6
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

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HelpForm myHelp = new HelpForm();
            myHelp.Owner = this;
            myHelp.Show();
        }

        private void CA_Click(object sender, RoutedEventArgs e)
        {
            LogInfoForm myLogInfo = new LogInfoForm();
            myLogInfo.Owner = this;
            myLogInfo.Show();
        }

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            LogForm myLog = new LogForm();
            myLog.Owner = this;
            myLog.Show();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            TestForm myTest = new TestForm();
            myTest.Owner = this;
            myTest.Show();
        }
        private void SQL_Click(object sender, RoutedEventArgs e)
        {
            SQLForm myForm = new SQLForm();
            myForm.Owner = this;
            myForm.Show();
        }

        private void BackUp_Click(object sender, RoutedEventArgs e)
        {
            BackUpForm myBackUp = new BackUpForm();
            myBackUp.Owner = this;
            myBackUp.Show();  
        }

        private void Lab_Click(object sender, RoutedEventArgs e)
        {
            LabForm myLab = new LabForm();
            myLab.Owner = this;
            myLab.Show();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutForm myAbout = new AboutForm();
            myAbout.Owner = this;
            myAbout.Show();
        }

        private void Ver_Click(object sender, RoutedEventArgs e)
        {
            VersionForm myVer = new VersionForm();
            myVer.Owner = this;
            myVer.Show();
        }

        private void picComp_Click(object sender, RoutedEventArgs e)
        {
            picComp myPic = new picComp();
            myPic.Owner = this;
            myPic.Show();
        }

    }
}
