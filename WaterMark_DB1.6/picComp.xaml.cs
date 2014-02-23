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
using Microsoft.Win32;

namespace WaterMark_DB1._6
{
    /// <summary>
    /// picComp.xaml 的交互逻辑
    /// </summary>
    public partial class picComp : Window
    {
        private byte[] bufPic1;
        private byte[] bufPic2;

        public picComp()
        {
            InitializeComponent();
        }

        private void btnpic1_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream loginmark = new MemoryStream();
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "bmp files (*.bmp) | *.bmp";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开图片";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                txtPic1.Text = dialogOpenFile.FileName.ToString();
                FileStream fs = File.OpenRead(txtPic1.Text);
                bufPic1 = new Byte[fs.Length];
                fs.Read(bufPic1, 0, bufPic1.Length);
                BitmapImage BI = new BitmapImage();
                BI.BeginInit();
                BI.UriSource = new Uri(txtPic1.Text);
                BI.EndInit();
                imgPic1.Source = BI;
            }
        }

        private void btnPic2_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream loginmark = new MemoryStream();
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "bmp files (*.bmp) | *.bmp";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开图片";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                txtPic2.Text = dialogOpenFile.FileName.ToString();
                FileStream fs = File.OpenRead(txtPic2.Text);
                bufPic2 = new Byte[fs.Length];
                fs.Read(bufPic2, 0, bufPic2.Length);
                BitmapImage BI = new BitmapImage();
                BI.BeginInit();
                BI.UriSource = new Uri(txtPic2.Text);
                BI.EndInit();
                imgPic2.Source = BI;
            }
        }

        private void btnComp_Click(object sender, RoutedEventArgs e)
        {
            int i;
            int picX1;
            int picX2;
            int picY1;
            int picY2;
            int count = 0;
            float nc_1 = 0;
            float nc_2 = 0;
            float nc_3 = 0;
            string binary1;
            string nc;
            string match;
            string binary2;
            string picMark1 = "";
            string picMark2 = "";

            picX1 = bufPic1[18];
            picY1 = bufPic1[22];
            picX2 = bufPic2[18];
            picY2 = bufPic2[22];

            if (picX1 == picX2 && picY1 == picY2)
            {
                for (i = 62; i < bufPic1.Length; i++)
                {
                    binary1 = System.Convert.ToString(bufPic1[i], 2);
                    binary2 = System.Convert.ToString(bufPic2[i], 2);

                    while (binary1.Length < 8)
                    {
                        binary1 = binary1.Insert(0, "0");
                    }

                    while (binary2.Length < 8)
                    {
                        binary2 = binary2.Insert(0, "0");
                    }

                    picMark1 += binary1;
                    picMark2 += binary2;
                }

                for (i = 0; i < picMark1.Length; i++)
                {
                    if (picMark1[i].Equals(picMark2[i]))
                    {
                        count++;
                    }
                    nc_1 += (float)Math.Sqrt((int)picMark1[i] * (int)picMark1[i]);
                    nc_2 += (float)Math.Sqrt((int)picMark2[i] * (int)picMark2[i]);
                    nc_3 += (int)picMark1[i] * (int)picMark2[i];
                }

                match = Math.Round((count * 1.0) / picMark1.Length * 100, 2).ToString() + "%"; 
                nc = (nc_3 / (nc_2 * nc_1)).ToString();
                MessageBox.Show("匹配结果:\n匹配率:" + match + "\n归一化相关系数：" + nc, "消息提示");
            }
            else
            {
                MessageBox.Show("图像规格不统一！","消息提示");
            }

        }
    }
}
