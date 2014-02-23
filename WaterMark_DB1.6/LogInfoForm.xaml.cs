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

namespace WaterMark_DB1._6
{
    /// <summary>
    /// LogInfoForm.xaml 的交互逻辑
    /// </summary>
    public partial class LogInfoForm : Window
    {
        public LogInfoForm()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
           if (txtCAPath.Text.Length > 0)
               MessageBox.Show("已提交至第三方");
           else
               MessageBox.Show("请选择dat文件!");
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "dat files (*.dat) | *.dat";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开信息源";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                txtCAPath.Text = dialogOpenFile.FileName.ToString();
            }
        }
    }
}
