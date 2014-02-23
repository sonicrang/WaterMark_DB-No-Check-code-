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
using System.Windows.Threading;
using System.Threading;
using System.ServiceProcess;


namespace WaterMark_DB1._6
{
    /// <summary>
    /// LogForm.xaml 的交互逻辑
    /// </summary>
    public partial class LogForm : Window
    {
        private string DBType;
        private string DBName;
        private string DBExt;
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
        private float parameter1; //参数1、2、3、4...
        private float parameter2;
        private float parameter3;
        private float parameter4;
        private float parameter5;
        private float parameter6;
        private float parameter7;
        private float parameter8;
        private int algorithm1; //零水印算法选择
        private int algorithm2; //图像算法选择
        private bool flag;
        private bool picFinish;
        private bool step2Finish;
        private bool step3Finish;
        private byte[] bufPic;
        private byte[] bufPicBak;
        private string[] TableList;
        private string sqlserverName;
        private ArrayList[] DBInfo;
        //DBInfo 数据库信息：[0]数据库行数 [1]数据库列数 [2]数据库字段名 [3]字符型字段个数 [4]字符型字段名
        private ArrayList[] picDBInfo;
        ServiceController sc;

        public LogForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            parameter1 = 0;
            parameter2 = 0;
            parameter3 = 0;
            parameter4 = 0;
            parameter5 = 0;
            parameter6 = 0;
            parameter7 = 0;
            parameter8 = 0;
            algorithm1 = 0;
            algorithm2 = 0;
            watermark = "";
            markBit = 0;
            markLvl = 0;
            picLen = 0;
            picOffset1 = 0;
            picOffset2 = 0;
            picX = 0;
            picY = 0;
            DBType = "";
            DBName = "";
            DBExt = "";
            TableName = "";
            picFinish = false;
            step2Finish = false;
            step3Finish = false;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            dialogOpenFile.Filter = "mdb or mdf files (*.mdb or *.mdf) | *.mdb;*.mdf";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开数据库";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                filePath = dialogOpenFile.FileName.ToString();
                DBName = dialogOpenFile.SafeFileName;
                DBExt = DBName.Substring(DBName.LastIndexOf('.') + 1);
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
                switch (DBExt)
                {
                    case "mdb":
                        DBType = "Access";
                        SelectAccess(filePath);
                        break;
                    case "mdf":
                        DBType = "SQL Server";
                        SelectSQL(DBName);
                        break;
                    default:
                        MessageBox.Show("数据库类型选择出错", "操作提示");
                        break;
                }
            }
          /* 
             else
            {
                DBType = "SQL Server";
                SelectSQL("myDB");
            }
           */
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

        /// <summary>
        /// 选择数据表并获取数据量信息
        /// </summary>
        private void cmbTableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTableList.SelectedIndex >= 0)
            {
                int i;
                TableName = cmbTableList.Items[cmbTableList.SelectedIndex].ToString();
                DBlink mylink = new DBlink();
                DBInfo = mylink.GetDBinfo(DBType, TableName);
                lblDBNum.Content = (int)DBInfo[0][0] + "行 " + (int)DBInfo[1][0] + "列 ";

                picDBInfo = new ArrayList[3] { new ArrayList(), new ArrayList(), new ArrayList() };
                picDBInfo[0].Add(DBInfo[0][0]);
                picDBInfo[1].Add(DBInfo[3][0]);
                for (i = 0; i < DBInfo[4].Count; i++)
                    picDBInfo[2].Add(DBInfo[4][i]);
                    if ((int)DBInfo[0][0] < 100)
                    {
                        markLvl = 0;
                        lblMarkType.Content = "数据量过小，不能注册零水印！";
                        grpStep2.IsEnabled = false;
                        grpStep3.IsEnabled = false;
                    }
                    else
                    {
                        markLvl = 1;
                        markBit = (int)DBInfo[0][0] / 100 * 32;
                        //零水印位数控制
                        if (markBit > 1024)
                            markBit = 1024;

                        lblMarkType.Content = "可以注册零水印，水印为" + markBit + "位！";
                        grpStep2.IsEnabled = true;
                        grpStep3.IsEnabled = false;
                    }

                if ((int)DBInfo[0][0] < 1500)
                {
                    lblPicInfo.Content = "数据量过小，不能注册图像水印！";
                }
                else
                {
                    markLvl = 2;
                    lblPicInfo.Content = "可以加入图像，规格请参考帮助！";
                    grpStep2.IsEnabled = true;
                    grpStep3.IsEnabled = true;
                }
            }
        }


        /// <summary>
        /// 选择图像
        /// </summary>
        private void btnChoosePic_Click(object sender, RoutedEventArgs e)
        {
            int i;
            string picPath = "";
            MemoryStream loginmark = new MemoryStream();
            OpenFileDialog dialogOpenFile = new OpenFileDialog();
            dialogOpenFile.AddExtension = true;
            dialogOpenFile.Filter = "bmp files (*.bmp) | *.bmp";
            dialogOpenFile.CheckPathExists = true;
            dialogOpenFile.Title = "打开图像";
            bool? result = dialogOpenFile.ShowDialog();

            if (result == true)
            {
                picPath = dialogOpenFile.FileName.ToString();
                FileStream fs = File.OpenRead(picPath);
                bufPic = new Byte[fs.Length];
                fs.Read(bufPic, 0, bufPic.Length);
                int error = PicSize(bufPic, (int)DBInfo[0][0]);
                if (error == 0)
                {
                    MessageBox.Show("图像规格不符合，请查看帮助！", "操作提示");
                    picFinish = false;
                }
                else
                {
                    BitmapImage BI = new BitmapImage();
                    BI.BeginInit();
                    BI.UriSource = new Uri(picPath);
                    BI.EndInit();
                    imgShow.Source = BI;
                    picFinish = true;
                    bufPicBak = new byte[bufPic.Length];
                    for (i = 0; i < bufPic.Length; i++)
                        bufPicBak[i] = bufPic[i];
                }
            }


        }

        /// <summary>
        /// 提交
        /// </summary>
        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            step2Finish = false;
            step3Finish = false;
           
            switch (markLvl)
            {
                case 0:
                    MessageBox.Show("该数据库不能注册水印！", "操作提示");
                    break;
                case 1:
                    Switch_algorithm1();
                    MessageBox.Show("水印嵌入完毕！");
                    break;
                case 2:

                    if (picFinish == false)
                    {
                        MessageBox.Show("请您选择图像！", "操作提示");
                    }
                    else
                    {
                        Switch_algorithm1();
                        Switch_algorithm2();
                        MessageBox.Show("水印嵌入完毕！");
                    }
                    break;

            } 
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
                        lblInfo.Content = "正在生成零水印...";
    
                        markLocate1 = login.Logistic(parameter1, parameter2, markBit, DBInfo);    
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, DBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 1);
                        txtWaterMark.Text = watermark;
                        lblInfo.Content = "零水印生成完毕...";
                   
                        step2Finish = true;
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
                        lblInfo.Content = "正在生成零水印...";
                        markLocate1 = login.Logistic(parameter1, parameter2, markBit, DBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, DBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 1);
                        txtWaterMark.Text = myMark.LogisticChaos(parameter3, parameter4, watermark);
                        lblInfo.Content = "零水印生成完毕！";
                        step2Finish = true;
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
                        lblInfo.Content = "正在生成零水印...";
                        markLocate1 = login.SuperChaos(parameter1, parameter2, parameter3, parameter4, markBit, DBInfo);
                        dataInfo = myMark.getDataInfo(markBit, markLocate1, TableName, DBType, DBInfo[2]);
                        watermark = myMark.initWaterMark(dataInfo, markBit, 1);
                        txtWaterMark.Text = watermark;
                        lblInfo.Content = "零水印生成完毕！";
                        step2Finish = true;
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
                            picLen = bufPic.Length;
                            picOffset1 = bufPic[2];
                            picOffset2 = bufPic[3];
                            MarkProducer login = new MarkProducer();
                            lblInfo.Content = "正在生成图像水印...";
                            
                            markLocate2 = login.Logistic(parameter5, parameter6, (picLen - 62) * 8, picDBInfo);
                            myMark.PicMark(picLen, bufPic, markLocate2, TableName, DBType, picDBInfo[2]);
                            lblInfo.Content = "图像水印生成完毕！";
                               
                            step3Finish = true;
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
                            picLen = bufPic.Length;
                            picOffset1 = bufPic[2];
                            picOffset2 = bufPic[3];
                            MarkProducer login = new MarkProducer();
                            lblInfo.Content = "正在生成图像水印...";

                            markLocate2 = login.Logistic(parameter6, parameter7, (picLen - 62) * 8, picDBInfo);
                            bufPic = login.BitExPicMark((int)parameter5, parameter1, parameter2, (picX - 1) + (picY - 1) + 8, bufPic, 1);
                            myMark.PicMark(picLen, bufPic, markLocate2, TableName, DBType, picDBInfo[2]);
                            lblInfo.Content = "图像水印生成完毕！";
                               
                            step3Finish = true;
                    }
                    break;
                default:
                    MessageBox.Show("请您选择第三步算法！", "操作提示");
                    break;
            }
        }

        /// <summary>
        /// 提交水印信息到CA
        /// </summary>
        private void btnCA_Click(object sender, RoutedEventArgs e)
        {
            if (!((markLvl == 1 && step2Finish) || (markLvl == 2 && step2Finish && step3Finish)))
            {
                MessageBox.Show("请您先完成注册！", "操作提示");
            }
            else
            {
                int i;
                string filePath = "";
                SaveFileDialog dialogSaveFile = new SaveFileDialog();
                dialogSaveFile.AddExtension = true;
                dialogSaveFile.Filter = "dat files (*.dat) | *.dat";
                dialogSaveFile.CheckPathExists = true;
                dialogSaveFile.Title = "上传CA";
                bool? result = dialogSaveFile.ShowDialog();

                if (result == true)
                {
                    filePath = dialogSaveFile.FileName.ToString();
                    try
                    {
                        using (StreamWriter myWriter = new StreamWriter(filePath))
                        {
                            myWriter.WriteLine((int)DBInfo[0][0]);
                            myWriter.WriteLine((int)DBInfo[1][0]);
                            myWriter.WriteLine((int)picDBInfo[1][0]);
                            myWriter.WriteLine(txtWaterMark.Text);
                            myWriter.WriteLine(DBType);
                            myWriter.WriteLine(picLen);
                            myWriter.WriteLine(picOffset1);
                            myWriter.WriteLine(picOffset2);
                            myWriter.WriteLine(picX);
                            myWriter.WriteLine(picY);
                            myWriter.WriteLine(DateTime.Now.ToString());

                            if (bufPic != null)
                            {
                                for (i = 62; i < bufPic.Length; i++)
                                {
                                    myWriter.WriteLine(bufPicBak[i]);
                                }
                            }

                            myWriter.Close();
                            MessageBox.Show("提交成功！", "操作提示");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("提交失败！", "操作提示");
                    }
                }
            }
        }

        /// <summary>
        /// 判断所选图像是否和数据量匹配
        /// </summary>
        /// <param name="bufPic">图像数据</param>
        /// <param name="DBNum">数据库行数</param>
        /// <returns>返回0则视为规格不符合</returns>
        private int PicSize(byte[] bufPic, int DBNum)
        {
            picX = bufPic[18]; //图像分辨率
            picY = bufPic[22];

            if (DBNum < 3000 && DBNum >= 1500 && picX == 32 && picY == 16)
            {
                return 1;
            }
            else if (DBNum < 5000 && DBNum >= 3000 && picX == 32 && picY == 32)
            {
                return 1;
            }
            else if (DBNum < 7000 && DBNum >= 5000 && picX == 64 && picY == 32)
            {
                return 1;
            }
            else if (DBNum < 14000 && DBNum >= 7000 && picX == 64 && picY == 64)
            {
                return 1;
            }
            else if (DBNum < 28000 && DBNum >= 14000 && picX == 128 && picY == 64)
            {
                return 1;
            }
            else if (DBNum >= 28000 && picX == 128 && picY == 128)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private void btnLab_Click(object sender, RoutedEventArgs e)
        {
            if (!((markLvl == 1 && step2Finish) || (markLvl == 2 && step2Finish && step3Finish)))
            {
                MessageBox.Show("请您先完成注册！", "操作提示");
            }
            else
            {
                int i;
                string filePath = "";
                SaveFileDialog dialogSaveFile = new SaveFileDialog();
                dialogSaveFile.AddExtension = true;
                dialogSaveFile.Filter = "lab files (*.lab) | *.lab";
                dialogSaveFile.CheckPathExists = true;
                dialogSaveFile.Title = "导出试验信息";
                bool? result = dialogSaveFile.ShowDialog();

                if (result == true)
                {
                    filePath = dialogSaveFile.FileName.ToString();
                    try
                    {
                        using (StreamWriter myWriter = new StreamWriter(filePath))
                        {
                            myWriter.WriteLine(parameter1);
                            myWriter.WriteLine(parameter2);
                            myWriter.WriteLine(parameter3);
                            myWriter.WriteLine(parameter4);
                            myWriter.WriteLine(parameter5);
                            myWriter.WriteLine(parameter6);
                            myWriter.WriteLine(parameter7);
                            myWriter.WriteLine(parameter8);
                            myWriter.WriteLine(algorithm1);
                            myWriter.WriteLine(algorithm2);
                            myWriter.WriteLine((int)DBInfo[0][0]);
                            myWriter.WriteLine((int)DBInfo[1][0]);

                            for (i = 0; i < (int)DBInfo[1][0]; i++)
                                myWriter.WriteLine(DBInfo[2][i]);

                            myWriter.WriteLine((int)picDBInfo[0][0]);
                            myWriter.WriteLine((int)picDBInfo[1][0]);

                            for (i = 0; i < (int)picDBInfo[1][0]; i++)
                                myWriter.WriteLine(picDBInfo[2][i]);

                            myWriter.WriteLine(txtWaterMark.Text);
                            myWriter.WriteLine(TableName);
                            myWriter.WriteLine(DBType);
                            myWriter.WriteLine(markBit);
                            myWriter.WriteLine(markLvl);
                            myWriter.WriteLine(txtDBPath.Text);
                            myWriter.WriteLine(picLen);
                            myWriter.WriteLine(picOffset1);
                            myWriter.WriteLine(picOffset2);
                            myWriter.WriteLine(picX);
                            myWriter.WriteLine(picY);
                           

                            if (bufPic != null)
                            {
                                for (i = 62; i < bufPic.Length; i++)
                                {
                                    myWriter.WriteLine(bufPicBak[i]);
                                }
                            }
                            myWriter.WriteLine("零水印");
                            for (i = 0; i < markBit; i++)
                                myWriter.WriteLine("ID = " + ((int)markLocate1[0][i] + 1).ToString("d5") + ", 字段 = " + DBInfo[2][(int)markLocate1[1][i]] + "  (" + i + ")");
                            myWriter.WriteLine("图像水印");
                            for (i = 0; i < (picLen - 62) * 8; i++)
                                myWriter.WriteLine("ID = " + ((int)markLocate2[0][i] + 1).ToString("d5") + ", 字段 = " + picDBInfo[2][(int)markLocate2[1][i]] + "  (" + i + ")");
                            myWriter.Close();

                            MessageBox.Show("提交成功！", "操作提示");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("提交失败！", "操作提示");
                    }
                }
            }
        }

        private void btnDBpre_Click(object sender, RoutedEventArgs e)
        {
            ViewForm myView = new ViewForm(TableName, DBType);
            myView.Owner = this;
            myView.Show();
        }

        private void btnMarkClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("您确定要执行清除操作吗？ ", "操作提示",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
               MarkProducer myMark = new MarkProducer();
               myMark.DelMarkAdapter(TableName, DBType, DBInfo);
               lblInfo.Content = "清除完毕！";
            }
        }

        private void btnKey_Click(object sender, RoutedEventArgs e)
        {
            string info = "";
            string p1 = "";
            string p2 = "";
            string p3 = "";
            string p4 = "";
            string p5 = "";
            string p6 = "";
            string p7 = "";

            switch(algorithm1)
            {
                case 1:
                    p1 = "0.8324";
                    p2 = "3.9556";
                    break;
                case 2:
                    p1 = "0.8324";
                    p2 = "3.9556";
                    p3 = "0.8456";
                    p4 = "3.9765";
                    break;
                case 3:
                    p1 = "1.52";
                    p2 = "-1.2";
                    p3 = "-1.1";
                    p4 = "0.1";
                    break;
            }

            switch (algorithm2)
            {
                case 1:
                    p5 = "0.8";
                    p6 = "3.98";
                    break;
                case 2:
                    p5 = "180";
                    p6 = "0.8786";
                    p7 = "3.9495";
                    break;
            }

            info = "\np1:" + p1 + "\np2:" + p2 + "\np3:" + p3 + "\np4:" + p4 + "\np5:" + p5 + "\np6:" + p6 + "\np7:" + p7;
            if (MessageBox.Show("示例参数为："+info+"\n是否自动填写?", "操作提示",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                txtPara1.Text = p1;
                txtPara2.Text = p2;
                txtPara3.Text = p3;
                txtPara4.Text = p4;
                txtPara5.Text = p5;
                txtPara6.Text = p6;
                txtPara7.Text = p7;
            }
        }

    }
}
