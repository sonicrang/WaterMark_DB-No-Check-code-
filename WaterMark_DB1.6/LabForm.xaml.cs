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
using System.Data;
using System.Collections;
using System.Data.OleDb;
using System.Data.SqlClient;


namespace WaterMark_DB1._6
{
    /// <summary>
    /// LabForm.xaml 的交互逻辑
    /// </summary>
    public partial class LabForm : Window
    {
        private string DBType;
        private string TableName;
        private string[] dataInfo;
        private int markBit;
        private int markLvl;
        private int picLen;               //图像大小
        private byte picOffset1;          //图像偏移量
        private byte picOffset2;
        private byte picX;                //图像分辨率
        private byte picY;
        private ArrayList[] markLocate1;  //零水印位置
        private ArrayList[] markLocate2;  //图像水印位置
        private string watermark;
        private float parameter1;         //参数1、2、3、4
        private float parameter2;
        private float parameter3;
        private float parameter4;
        private float parameter5;        
        private float parameter6;
        private float parameter7;
        private float parameter8;
        private string nc;                //归一化相关系数
        private string ber;               //误码率
        private string picBer;
        private int algorithm1;           //零水印算法选择
        private int algorithm2;           //图像算法选择
        private bool flag;
        private byte[] bufPic;
        private string[] TableList;
        private ArrayList[] sourceDBInfo;
        private ArrayList[] picDBInfo;
        private ArrayList sourceBufPic;

        public LabForm()
        {
            InitializeComponent();
        }

        private void btnCmp_Click(object sender, RoutedEventArgs e)
        {
            int i;
            int j;
            int count;
            int len;
            int bn = 0;
            float nc_1 = 0;
            float nc_2 = 0;
            float nc_3 = 0;
            string source;
            string target;
            string temp1;
            string temp2;
            string match = "无水印";
            string picMatch = "无图像";
            count = 0;
            len = txtSourceMark.Text.Length;

            if (txtWaterMark.Text.Length == len && len != 0)
            {
                source = txtSourceMark.Text;
                target = txtWaterMark.Text;

                for (i = 0; i < len; i++)
                {
                    if (source[i].Equals(target[i]))
                    {
                        count++;
                    }
                    else
                    {
                        bn++;
                    }
                    nc_1 += (float)Math.Sqrt((int)source[i] * (int)source[i]);
                    nc_2 += (float)Math.Sqrt((int)target[i] * (int)target[i]);
                    nc_3 += (int)target[i] * (int)source[i];
                }

                nc = (nc_3 / (nc_2 * nc_1)).ToString();
                ber = Math.Round((bn * 1.0 / source.Length * 100), 2).ToString() + "%";
                match = Math.Round((count * 1.0) / len * 100, 2).ToString() + "%";
                count = 0;

                if (bufPic != null)
                {
                    if (bufPic.Length != sourceBufPic.Count + 62)
                    {
                        MessageBox.Show("图像水印大小匹配出错！", "操作提示");
                        return;
                    }

                    for (i = 0; i < sourceBufPic.Count; i++)
                    {
                        temp1 = System.Convert.ToString(bufPic[i + 62], 2);
                        temp2 = System.Convert.ToString(byte.Parse(sourceBufPic[i].ToString()), 2);
                        while (temp1.Length < 8)
                        {
                            temp1 = temp1.Insert(0, "0");
                        }
                        while (temp2.Length < 8)
                        {
                            temp2 = temp2.Insert(0, "0");
                        }

                        for (j = 0; j < 8; j++)
                        {
                            if (temp1[j].Equals(temp2[j]))
                                count++;
                        }
                    }

                    picMatch = Math.Round((count * 1.0) / (sourceBufPic.Count * 8) * 100, 2).ToString() + "%";
                    picBer = (100 - Math.Round((count * 1.0) / (sourceBufPic.Count * 8) * 100, 2)).ToString() + "%";
                  
                }

                MessageBox.Show("\n零水印匹配率: " + match + "\n零水印误码率: " + ber+ "\n图像水印: " + picMatch +  "\n图像水印误码率："+ picBer +",匹配信息");
            }
            else
            {
                MessageBox.Show("水印匹配出错！", "操作提示");
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedIndex >= 0)
            {
                switch (cmbType.SelectedIndex)
                {
                    case 0:
                        lblPrompt1.Content = "添加ID 从 ";
                        lblPrompt2.Content = "到";
                        lblPrompt3.Content = "";
                        txtlblPrompt1.Clear();
                        txtlblPrompt2.Clear();
                        txtlblPrompt3.Clear();
                        txtlblPrompt1.Visibility = System.Windows.Visibility.Visible;
                        txtlblPrompt2.Visibility = System.Windows.Visibility.Visible;
                        txtlblPrompt3.Visibility = System.Windows.Visibility.Hidden;
                        cmbField.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    case 2:
                        lblPrompt1.Content = "更改字段 ";
                        lblPrompt2.Content = "ID";
                        lblPrompt3.Content = "值为";
                        txtlblPrompt1.Clear();
                        txtlblPrompt2.Clear();
                        txtlblPrompt3.Clear();
                        txtlblPrompt1.Visibility = System.Windows.Visibility.Hidden;
                        txtlblPrompt2.Visibility = System.Windows.Visibility.Visible;
                        txtlblPrompt3.Visibility = System.Windows.Visibility.Visible;
                        cmbField.Visibility = System.Windows.Visibility.Visible;
                        cmbField.Items.Clear();

                        for (int i = 0; i < (int)sourceDBInfo[1][0]; i++)
                            cmbField.Items.Add(sourceDBInfo[2][i].ToString());

                        break;
                    case 1:
                        lblPrompt1.Content = "删除ID 从 ";
                        lblPrompt2.Content = "到";
                        lblPrompt3.Content = "";
                        txtlblPrompt1.Clear();
                        txtlblPrompt2.Clear();
                        txtlblPrompt3.Clear();
                        txtlblPrompt1.Visibility = System.Windows.Visibility.Visible;
                        txtlblPrompt2.Visibility = System.Windows.Visibility.Visible;
                        txtlblPrompt3.Visibility = System.Windows.Visibility.Hidden;
                        cmbField.Visibility = System.Windows.Visibility.Hidden;
                        break;
                }
            }
        }

        private void btnChooseLab_Click(object sender, RoutedEventArgs e)
        {
            int i;
            string filePath = "";
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "lab files (*.lab) | *.lab";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开测试信息";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                filePath = dialogOpenFile.FileName.ToString();
                txtWaterMark.Clear();
                txtMarkLocate.Clear();
                btnStartLab.IsEnabled = true;
                cmbType.IsEnabled = true;

                try
                {
                    sourceBufPic = new ArrayList();
                    sourceDBInfo = new ArrayList[3] { new ArrayList(), new ArrayList(), new ArrayList() };
                    picDBInfo = new ArrayList[3] { new ArrayList(), new ArrayList(), new ArrayList() };
                    using (StreamReader myReader = new StreamReader(filePath))
                    {
                        parameter1 = float.Parse(myReader.ReadLine());
                        parameter2 = float.Parse(myReader.ReadLine());
                        parameter3 = float.Parse(myReader.ReadLine());
                        parameter4 = float.Parse(myReader.ReadLine());
                        parameter5 = float.Parse(myReader.ReadLine());
                        parameter6 = float.Parse(myReader.ReadLine());
                        parameter7 = float.Parse(myReader.ReadLine());
                        parameter8 = float.Parse(myReader.ReadLine());
                        algorithm1 = int.Parse(myReader.ReadLine());
                        algorithm2 = int.Parse(myReader.ReadLine());
                        sourceDBInfo[0].Add(int.Parse(myReader.ReadLine()));
                        sourceDBInfo[1].Add(int.Parse(myReader.ReadLine()));

                        for (i = 0; i < (int)sourceDBInfo[1][0]; i++)
                            sourceDBInfo[2].Add(myReader.ReadLine());

                        picDBInfo[0].Add(int.Parse(myReader.ReadLine()));
                        picDBInfo[1].Add(int.Parse(myReader.ReadLine()));

                        for (i = 0; i < (int)picDBInfo[1][0]; i++)
                            picDBInfo[2].Add(myReader.ReadLine());

                        txtSourceMark.Text = myReader.ReadLine();
                        TableName = myReader.ReadLine();
                        DBType = myReader.ReadLine();
                        markBit = int.Parse(myReader.ReadLine());
                        markLvl = int.Parse(myReader.ReadLine());
                        txtDBPath.Text = myReader.ReadLine();
                        picLen = int.Parse(myReader.ReadLine());
                        picOffset1 = byte.Parse(myReader.ReadLine());
                        picOffset2 = byte.Parse(myReader.ReadLine());
                        picX = byte.Parse(myReader.ReadLine());
                        picY = byte.Parse(myReader.ReadLine());

                        for (i = 0; i < picLen - 62; i++)
                        {
                            sourceBufPic.Add(byte.Parse(myReader.ReadLine()));
                        }

                        txtMarkLocate.Text += myReader.ReadToEnd();
                        myReader.Close();
                    }

                    switch (DBType)
                    {
                        case "Access":
                            SelectAccess();
                            break;
                        case "SQL Server":
                            SelectSQL();
                            break;
                        default:
                            MessageBox.Show("数据库类型选择出错", "操作提示");
                            break;
                    }


                }
                catch
                {
                    MessageBox.Show("导入失败！", "操作提示");
                }
            }
        }

      
        /// <summary>
        /// 打开Access数据库
        /// </summary>
        /// <param name="filePath"></param>
        private void SelectAccess()
        {
            DBlink mylink = new DBlink();
            TableList = mylink.ACSconection(txtDBPath.Text);
            if (TableList == null)
            {
                MessageBox.Show("数据库打开失败", "操作提示");
            }
            else
            {
                dgvDB.ItemsSource = mylink.Read(TableName, DBType).DefaultView;
               
            }
        }

        /// <summary>
        /// 打开SQL Server数据库
        /// </summary>
        /// <param name="filePath"></param>
        private void SelectSQL()
        {
            DBlink mylink = new DBlink();
            TableList = mylink.SQLconection(txtDBPath.Text);
            if (TableList == null)
            {
                MessageBox.Show("数据库打开失败", "操作提示");
            }
            else
            {
                dgvDB.ItemsSource = mylink.Read(TableName, DBType).DefaultView;
               
            }
        }

        private void btnStartLab_Click(object sender, RoutedEventArgs e)
        {
            if (cmbType.SelectedIndex >= 0)
            {
                try
                {
                    switch (cmbType.SelectedIndex)
                    {
                        case 0:
                            Add(int.Parse(txtlblPrompt1.Text), int.Parse(txtlblPrompt2.Text));
                            break;
                        case 2:
                            Upd(cmbField.Text, int.Parse(txtlblPrompt2.Text), txtlblPrompt3.Text );
                            break;
                        case 1:
                            Del(int.Parse(txtlblPrompt1.Text), int.Parse(txtlblPrompt2.Text));
                            break;
                    }

                    DBlink myLink = new DBlink();
                    dgvDB.ItemsSource = myLink.Read(TableName, DBType).DefaultView;
                }
                catch
                {
                    MessageBox.Show("参数错误！", "操作提示");
                }
            }
            else
            {
                MessageBox.Show("请您先选择实验类型！", "操作提示");
            }
        }

        private void Add(int from, int to)
        {
            DBlink myLink = new DBlink();
            try
            {
                myLink.AddAttack(TableName, from, to, DBType);;
            }
            catch
            {
            }

        }

        private void Del(int from, int to)
        {
            DBlink myLink = new DBlink();
            try
            {
                myLink.DelAttack(TableName, from, to, DBType);
            }
            catch
            {
            }
        }

        private void Upd(string field, int id, string value)
        {
            DBlink myLink = new DBlink();
            try
            {
                myLink.UpdAttack(TableName, field, value, id, DBType);
            }
            catch
            {
            }
        }

        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            switch (markLvl)
            {
                case 0:
                    MessageBox.Show("该数据库不能注册水印！", "操作提示");
                    break;
                case 1:
                    Switch_algorithm1();
                    break;
                case 2:
                    Switch_algorithm1();
                    Switch_algorithm2();
                    break;
            }
        }

        /// <summary>
        /// 零水印算法调度
        /// </summary>
        private void Switch_algorithm1()
        {
            MarkProducer myMark = new MarkProducer();
            switch (algorithm1)
            {
                case 1:
                    flag = (parameter1 < 1) && (parameter1 > 0) && (parameter2 >= 3.57) && (parameter2 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("零水印参数导入出错！", "操作提示");
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        markLocate1 = login.Logistic(parameter1, parameter2, markBit, sourceDBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, sourceDBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 2);
                        txtWaterMark.Text = watermark;
                    }
                    break;
                case 2:
                    flag = (parameter1 < 1) && (parameter1 > 0) && (parameter2 >= 3.57) && (parameter2 <= 4)
                        && (parameter3 < 1) && (parameter3 > 0) && (parameter4 >= 3.57) && (parameter4 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("零水印参数导入出错！", "操作提示");
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        markLocate1 = login.Logistic(parameter1, parameter2, markBit, sourceDBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, sourceDBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 2);
                        txtWaterMark.Text = myMark.LogisticChaos(parameter3, parameter4, watermark);
                    }
                    break;
                case 3:
                    {
                        flag = true;
                        MarkProducer login = new MarkProducer();
                        markLocate1 = login.SuperChaos(parameter1, parameter2, parameter3, parameter4, markBit, sourceDBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, sourceDBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit,2);
                        txtWaterMark.Text = watermark;
                    }
                    break;
                default:
                    MessageBox.Show("零水印算法导入出错！", "操作提示");
                    break;
            }
        }

        /// <summary>
        /// 双重水印算法调度
        /// </summary>
        private void Switch_algorithm2()
        {
            MarkProducer myMark = new MarkProducer();
            switch (algorithm2)
            {
                case 1:
                    flag = flag && (parameter5 < 1) && (parameter5 > 0) && (parameter6 >= 3.57) && (parameter6 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("图像水印参数导入出错！", "操作提示");
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        markLocate2 = login.Logistic(parameter5, parameter6, (picLen - 62) * 8, picDBInfo);
                        bufPic = myMark.GetPic(picLen, picOffset1, picOffset2, picX, picY, markLocate2, TableName, DBType, picDBInfo[2]);
                        myPicShow();
                    }
                    break;
                case 2:
                    flag = flag && (parameter5 <= 200) && (parameter5 >= 0)
                          && (parameter6 < 1) && (parameter6 > 0) && (parameter7 >= 3.57) && (parameter7 <= 4);

                     if (flag == false)
                    {
                        MessageBox.Show("图像水印参数导入出错！", "操作提示");
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        markLocate2 = login.Logistic(parameter6, parameter7, (picLen - 62) * 8, picDBInfo);
                        bufPic = myMark.GetPic(picLen, picOffset1, picOffset2, picX, picY, markLocate2, TableName, DBType, picDBInfo[2]);
                        bufPic = myMark.BitExPicMark((int)parameter5, parameter1, parameter2, (picX - 1) + (picY - 1) + 8, bufPic, 2);
                        myPicShow();
                    }
                    break;

                default:
                    MessageBox.Show("图像水印算法导入出错！", "操作提示");
                    break;
            }
        }

        private void myPicShow()
        {
            BitmapImage BI = new BitmapImage();
            BI.BeginInit();
            BI.StreamSource = new MemoryStream(bufPic);
            BI.EndInit();
            imgShow.Source = BI;
        }

        private void btnOutPic_Click(object sender, RoutedEventArgs e)
        {
                string filePath = "";
                SaveFileDialog dialogSaveFile = new SaveFileDialog();
                dialogSaveFile.AddExtension = true;
                dialogSaveFile.Filter = "bmp files (*.bmp) | *.bmp";
                dialogSaveFile.CheckPathExists = true;
                dialogSaveFile.Title = "导出图像";
                bool? result = dialogSaveFile.ShowDialog();

                if (result == true)
                {
                    filePath = dialogSaveFile.FileName.ToString();
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    try
                    {
                        encoder.Frames.Add(BitmapFrame.Create(new MemoryStream(bufPic)));
                        FileStream bitmap = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                        encoder.Save(bitmap);
                        bitmap.Close();   
                    }
                    catch
                    {
                        MessageBox.Show("导出图像失败！","消息提示");
                    }
                   
                }
        }
    }
}
