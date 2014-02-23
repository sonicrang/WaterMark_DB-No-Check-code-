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
using System.IO;
using System.Data;
using System.Collections;
using System.ServiceProcess;

namespace WaterMark_DB1._6
{
    /// <summary>
    /// TestForm.xaml 的交互逻辑
    /// </summary>
    public partial class TestForm : Window
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
        private int algorithm1;           //零水印算法选择
        private int algorithm2;           //图像算法选择
        private bool flag;
        private byte[] bufPic;
        private string[] TableList;
        private string DBName;
        private string LogTime;
        private string zeroHarmming;
        private string picHarmming;
        private string sqlserverName;
        private ArrayList sourceBufPic;
        private ArrayList[] sourceDBInfo;
        private ArrayList[] DBInfo;
        //DBInfo 数据库信息：[0]数据库行数 [1]数据库列数 [2]数据库字段名 [3]字符型字段个数 [4]字符型字段名
        private ArrayList[] picDBInfo;
        ServiceController sc;

        public TestForm()
        {
            InitializeComponent();
        }

        private void btnChooseCA_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "dat files (*.dat) | *.dat";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开信息源";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                filePath = dialogOpenFile.FileName.ToString();
                try
                {
                    sourceDBInfo = new ArrayList[2] { new ArrayList(), new ArrayList()};
                    sourceBufPic = new ArrayList();
                    picDBInfo = new ArrayList[3] { new ArrayList(), new ArrayList(), new ArrayList() };
                    using (StreamReader myReader = new StreamReader(filePath))
                    {
                        sourceDBInfo[0].Add(int.Parse(myReader.ReadLine()));
                        sourceDBInfo[1].Add(int.Parse(myReader.ReadLine()));
                        picDBInfo[1].Add(int.Parse(myReader.ReadLine()));
                        txtSourceMark.Text = myReader.ReadLine();
                        DBType = myReader.ReadLine();
                        picLen = int.Parse(myReader.ReadLine());
                        picOffset1 = byte.Parse(myReader.ReadLine());
                        picOffset2 = byte.Parse(myReader.ReadLine());
                        picX = byte.Parse(myReader.ReadLine());
                        picY = byte.Parse(myReader.ReadLine());
                        zeroHarmming = myReader.ReadLine();
                        picHarmming = myReader.ReadLine();
                        LogTime = myReader.ReadLine();

                        while (!myReader.EndOfStream)
                        {
                            sourceBufPic.Add(byte.Parse(myReader.ReadLine()));
                        }

                        myReader.Close();
                        lblDBType.Content = DBType;
                    }
                    txtCA.Text = filePath;
                    txtWaterMark.Clear();
                }
                catch
                {
                    MessageBox.Show("导入失败！", "操作提示");
                }
            }
        }

        private void btnChooseDB_Click(object sender, RoutedEventArgs e)
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
            }

            string filePath = "";
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开数据库";
            switch (DBType)
            {
                case "Access":
                    dialogOpenFile.Filter = "mdb files (*.mdb) | *.mdb";
                    break;
                case "SQL Server":
                    dialogOpenFile.Filter = "mdf files (*.mdf) | *.mdf";
                    break;
            }
            bool? result = dialogOpenFile.ShowDialog();
            if (result == true)
            {
                filePath = dialogOpenFile.FileName.ToString();
                DBName = dialogOpenFile.SafeFileName;
                DBName = DBName.Substring(0, DBName.LastIndexOf('.'));
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
                }
                switch (DBType)
                {
                    case "Access":
                        SelectAccess(filePath);
                        break;
                    case "SQL Server":
                        SelectSQL(DBName);
                        break;
                    default:
                        MessageBox.Show("数据库类型选择出错", "操作提示");
                        break;
                }

                txtWaterMark.Clear();
            }
        }

         /// <summary>
        /// 打开Access数据库
        /// </summary>
        /// <param name="filePath"></param>
        private void SelectAccess(string filePath)
        {
            DBlink mylink = new DBlink();
            TableList = mylink.ACSconection(filePath);
            if (TableList == null)
            {
                MessageBox.Show("数据库打开失败", "操作提示");
            }
            else
            {
                cmbTableList.IsEnabled = true;
                txtDBPath.Text = filePath;
                lblDBType.Content = DBType;

                cmbTableList.Items.Clear();
                for (int i = 0; i < TableList.Length; i++)
                {
                    cmbTableList.Items.Add(TableList[i]);
                }

                cmbTableList.SelectedIndex = 0;

                MessageBox.Show("数据库打开成功", "操作提示");
            }
        }

        /// <summary>
        /// 打开SQL Server数据库
        /// </summary>
        /// <param name="filePath"></param>
        private void SelectSQL(string filePath)
        {
            DBlink mylink = new DBlink();
            TableList = mylink.SQLconection(filePath);
            if (TableList == null)
            {
                MessageBox.Show("数据库打开失败", "操作提示");
            }
            else
            {  
                cmbTableList.IsEnabled = true;
                txtDBPath.Text = filePath;
                lblDBType.Content = DBType;

                cmbTableList.Items.Clear();
                for (int i = 0; i < TableList.Length; i++)
                {
                    cmbTableList.Items.Add(TableList[i]);
                }

                cmbTableList.SelectedIndex = 0;

                MessageBox.Show("数据库打开成功", "操作提示");
            }
        }

        private void cmbTableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTableList.SelectedIndex >= 0)
            {
                int i;
                TableName = cmbTableList.Items[cmbTableList.SelectedIndex].ToString();
                DBlink mylink = new DBlink();
                DBInfo = mylink.GetDBinfo(DBType, TableName);
                lblDBNum.Content = (int)DBInfo[0][0] + "行 " + (int)DBInfo[1][0] + "列 ";
              
                picDBInfo[0].Add(sourceDBInfo[0][0]);
                for (i = 0; i < (int)picDBInfo[1][0]; i++)
                    picDBInfo[2].Add(DBInfo[4][i]);
                if ((int)sourceDBInfo[0][0] * (int)sourceDBInfo[1][0] < 100)
                {
                    markLvl = 0;
                    lblMarkType.Content = "您的数据库没有注册水印！";
                    grpStep2.IsEnabled = false;
                    grpStep3.IsEnabled = false;
                }
                else
                {
                    markLvl = 1;
                    markBit = (int)sourceDBInfo[0][0] / 100 * 32;

                    if (markBit > 1024)
                        markBit = 1024;

                    lblMarkType.Content = "您的数据库注册了零水印" + markBit + "位！";
                    grpStep2.IsEnabled = true;
                    grpStep3.IsEnabled = false;
                }

                if ((int)sourceDBInfo[0][0] * (int)sourceDBInfo[1][0] < 1500)
                {
                    lblPicInfo.Content = "您的数据库没有注册图像水印！";
                }
                else
                {
                    markLvl = 2;
                    lblPicInfo.Content = "您的数据库注册了图像水印！";
                    grpStep2.IsEnabled = true;
                    grpStep3.IsEnabled = true;
                }
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmbAlgList1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtPara1.Clear();
            txtPara2.Clear();
            txtPara3.Clear();
            txtPara4.Clear();

            switch (cmbAlgList1.SelectedIndex)
            {
                case 0:
                    algorithm1 = 1;
                    lblPara1.Content = "X ( 0 , 1 )  :";
                    lblPara2.Content = "U (3.57, 4)  :";
                    lblPara3.Content = " ";
                    lblPara4.Content = " ";
                    txtPara1.Visibility = Visibility.Visible;
                    txtPara2.Visibility = Visibility.Visible;
                    txtPara3.Visibility = Visibility.Hidden;
                    txtPara4.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    algorithm1 = 2;
                    lblPara1.Content = "X ( 0 , 1 )  :";
                    lblPara2.Content = "U (3.57, 4)  :";
                    lblPara3.Content = "X ( 0 , 1 )  :";
                    lblPara4.Content = "U (3.57, 4)  :";
                    txtPara1.Visibility = Visibility.Visible;
                    txtPara2.Visibility = Visibility.Visible;
                    txtPara3.Visibility = Visibility.Visible;
                    txtPara4.Visibility = Visibility.Visible;
                    break;
                case 2:
                    algorithm1 = 3;
                    lblPara1.Content = "m4   :";
                    lblPara2.Content = "m5   :";
                    lblPara3.Content = "m8   :";
                    lblPara4.Content = "m10  :";
                    txtPara1.Visibility = Visibility.Visible;
                    txtPara2.Visibility = Visibility.Visible;
                    txtPara3.Visibility = Visibility.Visible;
                    txtPara4.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void cmbAlgList2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtPara5.Clear();
            txtPara6.Clear();
            txtPara7.Clear();
            txtPara8.Clear();


            switch (cmbAlgList2.SelectedIndex)
            {
                case 0:
                    algorithm2 = 1;
                    lblPara5.Content = "X ( 0 , 1 )  :";
                    lblPara6.Content = "U (3.57, 4)  :";
                    lblPara7.Content = " ";
                    lblPara8.Content = " ";
                    txtPara5.Visibility = Visibility.Visible;
                    txtPara6.Visibility = Visibility.Visible;
                    txtPara7.Visibility = Visibility.Hidden;
                    txtPara8.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    algorithm2 = 2;
                    lblPara5.Content = "L (0 , 200)  :";
                    lblPara6.Content = "X ( 0 , 1 )  :";
                    lblPara7.Content = "U (3.57, 4)  :";
                    lblPara8.Content = " ";
                    txtPara5.Visibility = Visibility.Visible;
                    txtPara6.Visibility = Visibility.Visible;
                    txtPara7.Visibility = Visibility.Visible;
                    txtPara8.Visibility = Visibility.Hidden;
                    break;
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
                    flag = float.TryParse(txtPara1.Text, out parameter1) && float.TryParse(txtPara2.Text, out parameter2);
                    flag = flag && (parameter1 < 1) && (parameter1 > 0) && (parameter2 >= 3.57) && (parameter2 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("第二步参数设置出错！", "操作提示");
                        txtPara1.Clear();
                        txtPara2.Clear();
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        login.set_Hamming(zeroHarmming);
                        markLocate1 = login.Logistic(parameter1, parameter2, markBit, sourceDBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, DBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 2);
                        txtWaterMark.Text = watermark;
                        lblInfo.Content = "零水印生成完毕！";
                    }
                    break;
                case 2:
                    flag = float.TryParse(txtPara1.Text, out parameter1) && float.TryParse(txtPara2.Text, out parameter2)
                        && float.TryParse(txtPara3.Text, out parameter3) && float.TryParse(txtPara4.Text, out parameter4);
                    flag = flag && (parameter1 < 1) && (parameter1 > 0) && (parameter2 >= 3.57) && (parameter2 <= 4)
                        && (parameter3 < 1) && (parameter3 > 0) && (parameter4 >= 3.57) && (parameter4 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("第二步参数设置出错！", "操作提示");
                        txtPara1.Clear();
                        txtPara2.Clear();
                        txtPara3.Clear();
                        txtPara4.Clear();
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        login.set_Hamming(zeroHarmming);
                        markLocate1 = login.Logistic(parameter1, parameter2, markBit, DBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, DBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 2);
                        txtWaterMark.Text = myMark.LogisticChaos(parameter3, parameter4, watermark);
                        lblInfo.Content = "零水印生成完毕！";
                    }
                    break;
                case 3:
                    flag = float.TryParse(txtPara1.Text, out parameter1) && float.TryParse(txtPara2.Text, out parameter2)
                       && float.TryParse(txtPara3.Text, out parameter3) && float.TryParse(txtPara4.Text, out parameter4);
                    if (flag == false)
                    {
                        MessageBox.Show("第二步参数设置出错！", "操作提示");
                        txtPara1.Clear();
                        txtPara2.Clear();
                        txtPara3.Clear();
                        txtPara4.Clear();
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        login.set_Hamming(zeroHarmming);
                        markLocate1 = login.SuperChaos(parameter1, parameter2, parameter3, parameter4, markBit, DBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, DBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 2);
                        txtWaterMark.Text = watermark;
                        lblInfo.Content = "零水印生成完毕！";
                    }
                    break;
                default:
                    MessageBox.Show("请您选择第二步算法！", "操作提示");
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
                    flag = flag && float.TryParse(txtPara5.Text, out parameter5) && float.TryParse(txtPara6.Text, out parameter6);
                    flag = flag && (parameter5 < 1) && (parameter5 > 0) && (parameter6 >= 3.57) && (parameter6 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("第三步参数设置出错！", "操作提示");
                        txtPara5.Clear();
                        txtPara6.Clear();
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        markLocate2 = login.Logistic(parameter5, parameter6, (picLen - 62) * 8, picDBInfo);
                        bufPic = myMark.GetPic(picLen, picOffset1, picOffset2, picX, picY, markLocate2, TableName, DBType, picDBInfo[2]);
                        lblInfo.Content = "水印生成完毕！";
                        myPicShow();
                    }
                    break;
                case 2:
                   flag = flag && float.TryParse(txtPara5.Text, out parameter5) && float.TryParse(txtPara6.Text, out parameter6)
                        && float.TryParse(txtPara7.Text, out parameter7) ;
                    flag = flag && (parameter5 <= 200) && (parameter5 >= 0) 
                        && (parameter6 < 1) && (parameter6 > 0) && (parameter7 >= 3.57) && (parameter7 <= 4);
                    if (flag == false)
                    {
                        MessageBox.Show("第三步参数设置出错！", "操作提示");
                        txtPara5.Clear();
                        txtPara6.Clear();
                        txtPara7.Clear();
                    }
                    else
                    {
                        MarkProducer login = new MarkProducer();
                        markLocate2 = login.Logistic(parameter6, parameter7, (picLen - 62) * 8, picDBInfo);
                        bufPic = myMark.GetPic(picLen, picOffset1, picOffset2, picX, picY, markLocate2, TableName, DBType, picDBInfo[2]);
                        login.set_Hamming(picHarmming);
                        bufPic = myMark.BitExPicMark((int)parameter5,parameter1, parameter2, (picX - 1) + (picY - 1) + 8, bufPic, 2);
                        lblInfo.Content = "水印生成完毕！";
                        myPicShow();
                    }
                    break;

                default:
                    MessageBox.Show("请您选择第三步算法！", "操作提示");
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

        private void btnCmp_Click(object sender, RoutedEventArgs e)
        {
            int i;
            int j;
            int count;
            int len;
            double zero;
            double pic;
            string source;
            string target;
            string temp1;
            string temp2;
            string match = "无水印"; 
            string picMatch = "无图像";
            string copyright ="";
            count = 0;
            len = txtSourceMark.Text.Length;
            if (txtWaterMark.Text.Length == len && len != 0)
            {
                source = txtSourceMark.Text;
                target = txtWaterMark.Text;

                for (i = 0; i < len; i++)
                {
                    if (source[i].Equals(target[i]))
                        count++;
                }

                zero = Math.Round((count * 1.0) / len * 100, 2);
                match = zero.ToString() + "%";
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

                    pic = Math.Round((count * 1.0) / (sourceBufPic.Count * 8) *100, 2);
                    picMatch = pic.ToString() + "%";

                    if (zero >= 80 && pic >= 80)
                    {
                        copyright = "水印匹配率符合标准\n该数据库版权确定！";
                    }
                    else if (zero < 80 && pic >= 80)
                    {
                        copyright = "零水印匹配较低，图像水印匹配率符合标准\n该数据库版权需进一步确定！";
                    }
                    else if (zero >= 80 && pic < 80)
                    {
                        copyright = "零水印匹配率符合标准，图像水印匹配较低\n该数据库版权需进一步确定！";
                    }
                    else
                    {
                        copyright = "零水印及图像水印匹配较低，不符合标准\n该数据库版权不确定！";
                    }

                }   

                DateTime effectTime = DateTime.Now;
                MessageBox.Show("CA文件: " + txtCA.Text + "\n注册时间: " + LogTime + "\n水印有效期: " +
                      (effectTime.AddYears(1)) + "\n零水印匹配率: " + match + "\n图像水印: " + picMatch + "\n版权归属：" + copyright, "匹配信息");
            }
            else
            {
                MessageBox.Show("水印匹配出错！", "操作提示");
            }
        }

        private void btnDBpre_Click(object sender, RoutedEventArgs e)
        {
            ViewForm myView = new ViewForm(TableName, DBType);
            myView.Owner = this;
            myView.Show();
        }

    }
}
