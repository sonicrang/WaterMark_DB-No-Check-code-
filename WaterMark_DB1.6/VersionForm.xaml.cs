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
using System.IO;

namespace WaterMark_DB1._6
{
    /// <summary>
    /// VersionForm.xaml 的交互逻辑
    /// </summary>
    public partial class VersionForm : Window
    {
        public VersionForm()
        {
            InitializeComponent();

            txtVer.Clear();
            string infoPath = "readme.txt";
            try
            {
                using (StreamReader myReader = new StreamReader(infoPath))
                {
                    txtVer.Text += myReader.ReadToEnd();
                }
            }
            catch
            {
                txtVer.Text = "找不到版本信息！";
            }
        }
    }
}
