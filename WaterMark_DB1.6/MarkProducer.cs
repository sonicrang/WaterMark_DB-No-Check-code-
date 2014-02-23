using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Controls;


namespace WaterMark_DB1._6
{
    class MarkProducer
    {
        public static string Hamming;

        public string get_Hamming()
        {
            return Hamming;
        }

        public void set_Hamming(string newHamming)
        {
            Hamming = newHamming;
        }

        /// <summary>
        /// logistic算法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="u"></param>
        /// <param name="markBit"></param>
        /// <param name="DBInfo"></param>
        /// <returns></returns>
        public ArrayList[] Logistic(float x, float u, int markBit, ArrayList[] DBInfo)
        {
            int i;
            int temp;
            float x_next = 0.0f;
            ArrayList[] logistic = new ArrayList[2];
            logistic[0] = new ArrayList(markBit);
            logistic[1] = new ArrayList(markBit);

            /*************************预先迭代200次*************************/

            for (i = 0; i < 200; i++)
            {
                x_next = u * x * (1 - x);
                x = x_next;
            }


            /**********************正常迭代取不同的32位**********************/

            while (logistic[0].Count < markBit)
            {
                x_next = u * x * (1 - x);
                x = x_next;
                temp = (int)(x * (int)DBInfo[0][0]);
                if (!logistic[0].Contains(temp))
                {
                    logistic[0].Add(temp);
                    logistic[1].Add(temp % (int)DBInfo[1][0]);
                }

            }

            return logistic;
        }

        /// <summary>
        /// 超混沌算法
        /// </summary>
        /// <param name="m4"></param>
        /// <param name="m5"></param>
        /// <param name="m8"></param>
        /// <param name="m10"></param>
        /// <param name="markBit"></param>
        /// <param name="DBInfo"></param>
        /// <returns></returns>
        public ArrayList[] SuperChaos(float m4, float m5, float m8, float m10, int markBit, ArrayList[] DBInfo)
        {
            float x;
            float y;
            float x_next;
            float y_next;
            float l;
            int row;
            int col;
            ArrayList[] SuperChaos = new ArrayList[2];
            SuperChaos[0] = new ArrayList(markBit);
            SuperChaos[1] = new ArrayList(markBit);
            x = 0.5f;
            y = 0.5f;

            while (SuperChaos[0].Count < markBit)
            {
                x_next = m4 * y + m5 * y * y;
                y_next = m8 * x + m10 * y;
                l = (float)((x_next - y_next + 1.5) / 2.5);
                row = (int)(l * (int)DBInfo[0][0]);
                col = row % (int)DBInfo[1][0];
                if (!SuperChaos[0].Contains(row) && row > 0 && row < (int)DBInfo[0][0])
                {
                    SuperChaos[0].Add(row);
                    SuperChaos[1].Add(col);
                }
                x = x_next;
                y = y_next;
            }

            return SuperChaos;
        }

        /// <summary>
        /// 二重logistic置乱算法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="u"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public string LogisticChaos(float x, float u, string mark)
        {
            int i;
            int markLen;
            int temp;
            char swap;
            float x_next = 0.0f;
            StringBuilder chaosMark = new StringBuilder(mark);
            ArrayList logistic = new ArrayList();
            markLen = mark.Length;

            for (i = 0; i < 200; i++)
            {
                x_next = u * x * (1 - x);
                x = x_next;
            }

            while (logistic.Count < markLen)
            {
                x_next = u * x * (1 - x);
                x = x_next;
                temp = (int)(x * markLen);
                logistic.Add(temp);
            }

            for (i = 0; i < markLen; i++)
            {
                swap = mark[i];
                chaosMark[i] = chaosMark[(int)logistic[i]];
                chaosMark[(int)logistic[i]] = swap;
            }

            return chaosMark.ToString();
        }

        /// <summary>
        /// logistic位交换算法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="u"></param>
        /// <param name="len"></param>
        /// <param name="bufPic"></param>
        /// <param name="codeSwitch">加密解密开关 1加密, 2解密</param>
        /// <returns></returns>
        public byte[] BitExPicMark(int l, float x, float u, int len, byte[] bufPic, int codeSwitch)
        {
            int i;
            int j;
            int m;
            int n;
            char strTemp;
            float temp;
            float x_next = 0.0f;
            string binary;
            StringBuilder newMark_head;
            StringBuilder newMark_tail;
            StringBuilder newMark;
            ArrayList logistic = new ArrayList();

            for (i = 0; i < l; i++)
            {
                x_next = u * x * (1 - x);
                x = x_next;
            }


            while (logistic.Count < len)
            {
                temp = x;
                x_next = u * x * (1 - x);
                x = x_next;


                if (logistic.Count == 0)
                {
                    if (x < 0.5)
                    {
                        logistic.Add(0);
                    }
                    else
                    {
                        logistic.Add(1);
                    }
                }
                else
                {
                    if (x < temp)
                    {
                        logistic.Add(0);
                    }
                    else
                    {
                        logistic.Add(1);
                    }
                }

            }

            switch (codeSwitch)
            {
                case 1:    //encode
                    {

                        for (i = 62; i < bufPic.Length; i++)
                        {
                            m = (i - 62) / (bufPic[18] / 8);
                            n = (i - 62) % (bufPic[18] / 8);
                            binary = System.Convert.ToString(bufPic[i], 2);

                            while (binary.Length < 8)
                            {
                                binary = binary.Insert(0, "0");
                            }

                            newMark = new StringBuilder(binary);

                            if ((m + n) % 2 == 0)
                            {
                                strTemp = newMark[0];
                                newMark[0] = newMark[7];
                                newMark[7] = strTemp;

                                for (j = 0; j < 8; j++)
                                {
                                    newMark[j] = (char)(newMark[j] ^ (int)logistic[m + n + j]);
                                }
                            }
                            else
                            {
                                for (j = 0; j < 8; j++)
                                {
                                    newMark[j] = (char)(newMark[j] ^ (int)logistic[m + n + j]);
                                }


                                strTemp = newMark[0];
                                newMark[0] = newMark[7];
                                newMark[7] = strTemp;
                            }
                            bufPic[i] = System.Convert.ToByte(newMark.ToString(), 2);
                        }

                        break;
                    }
                case 2:    //decode
                    {
                        for (i = 62; i < bufPic.Length; i++)
                        {
                            m = (i - 62) / (bufPic[18] / 8);
                            n = (i - 62) % (bufPic[18] / 8);
                            binary = System.Convert.ToString(bufPic[i], 2);
                            while (binary.Length < 8)
                            {
                                binary = binary.Insert(0, "0");
                            }

                            newMark_head = new StringBuilder(binary.Substring(0, 4));
                            newMark_tail = new StringBuilder(binary.Substring(4, 4));
                            newMark = new StringBuilder(newMark_head.ToString() + newMark_tail.ToString());

                            if ((m + n) % 2 == 0)
                            {
                                for (j = 0; j < 8; j++)
                                {
                                    newMark[j] = (char)(newMark[j] ^ (int)logistic[m + n + j]);
                                }

                                strTemp = newMark[0];
                                newMark[0] = newMark[7];
                                newMark[7] = strTemp;
                            }
                            else
                            {
                                strTemp = newMark[0];
                                newMark[0] = newMark[7];
                                newMark[7] = strTemp;

                                for (j = 0; j < 8; j++)
                                {
                                    newMark[j] = (char)(newMark[j] ^ (int)logistic[m + n + j]);
                                }

                            }

                            bufPic[i] = System.Convert.ToByte(newMark.ToString(), 2);
                        }

                        break;
                    }
            }

            return bufPic;
        }

        private StringBuilder Hamming_correct(StringBuilder data, int y0, int y1, int y2)
        {
            string lable;

            lable = "" + y0 + y1 + y2;
            switch (lable)
            {
                case "011":
                    data[2] = char.Parse(((data[2] - 48) ^ 1).ToString());
                    break;
                case "101":
                    data[0] = char.Parse(((data[0] - 48) ^ 1).ToString());
                    break;
                case "110":
                    data[3] = char.Parse(((data[3] - 48) ^ 1).ToString());
                    break;
                case "111":
                    data[1] = char.Parse(((data[1] - 48) ^ 1).ToString());
                    break;
                default:
                    break;
            }

            return data;
        }

        /// <summary>
        /// 获取数据库信息
        /// </summary>
        /// <param name="markBit">水印位</param>
        /// <param name="markLocate1">水印位置</param>
        /// <param name="TableName">表名</param>
        /// <param name="DBType">数据库类型</param>
        /// <param name="field">字段名</param>
        /// <returns>读取的数据信息</returns>
        public string[] getDataInfo(int markBit, ArrayList[] markLocate1, string TableName, string DBType, ArrayList field)
        {
            string[] mydataInfo = new string[markBit];
            int i;

            for (i = 0; i < markBit; i++)
            {
                DBlink myLink = new DBlink();
                mydataInfo[i] = myLink.getData((int)markLocate1[0][i], (int)markLocate1[1][i], TableName, DBType, field);
            }

            return mydataInfo;
        }


        /// <summary>
        /// 生成零水印
        /// </summary>
        /// <param name="dataInfo">数据库信息</param>
        /// <param name="markBit">水印位</param>
        /// <returns>零水印</returns>
        public string initWaterMark(string[] dataInfo, int markBit, int op)
        {
            int i;
            int HighBit;
            string myWaterMarkText = "";
            string temp;

                        for (i = 0; i < markBit; i++)
                        {
                            if (!dataInfo[i].Equals("*"))
                            {
                                HighBit = Int32.Parse(dataInfo[i].Substring(0, 1));
                                temp = System.Convert.ToString(HighBit, 2);

                                // 因为B的取值规定在0~4之间，所以对于不满4位的二进制序列，要在高位补0
                                while (temp.Length < 4)
                                {
                                    temp = temp.Insert(0, "0");
                                }

                                myWaterMarkText += temp.Substring(2, 1);
                            }
                            else
                            {
                                myWaterMarkText += "0";
                            }

            }

            return myWaterMarkText;
        }

        public void DelMarkAdapter(string TableName, string DBType, ArrayList[] DBInfo)
        {
            int i;
            int j;
            DBlink mylink = new DBlink();

            for (i = 0; i < (int)DBInfo[0][0]; i++)
                for (j = 0; j < (int)DBInfo[1][0]; j++)
                    mylink.DelOldMark(i, j, TableName, DBType, DBInfo[2]);

        }

        /// <summary>
        /// 加入图片水印
        /// </summary>
        /// <param name="bufPic">图片信息</param>
        /// <param name="markBit">水印位</param>
        /// <param name="markLocate2">水印位置</param>
        /// <param name="TableName">数据表名</param>
        /// <param name="DBType">数据库类型</param>
        /// <param name="field">字段名</param>
        public void PicMark(int picLen, byte[] bufPic, ArrayList[] markLocate2, string TableName, string DBType, ArrayList field)
        {
            int i;
            string mark = "";
            int picMarkBit;
            picMarkBit = (picLen - 62) * 8;

            for (i = 62; i < picLen; i++)
            {
                string binary = System.Convert.ToString(bufPic[i], 2);
                while (binary.Length < 8)
                {
                    binary = binary.Insert(0, "0");
                }
                mark += binary;
            }

            for (i = 0; i < picMarkBit; i++)
            {
                DBlink myLink = new DBlink();
                myLink.AddPicInfo(mark[i], (int)markLocate2[0][i], (int)markLocate2[1][i], TableName, DBType, field);
            }
        }
        /*
       /// <summary>
        /// 重载加入图片水印
       /// </summary>
       /// <param name="picLen"></param>
       /// <param name="bufPic"></param>
        /// <param name="Hamming">循环冗余检验</param>
       /// <param name="markLocate2"></param>
       /// <param name="TableName"></param>
       /// <param name="DBType"></param>
       /// <param name="field"></param>
        public void PicMark(int picLen, byte[] bufPic, string Hamming, ArrayList[] markLocate2, byte picX, byte picY, string TableName, string DBType, ArrayList field)
        {
            int i;
            string mark = "";
            string head;
            string tail;
            int picMarkBit;
            picMarkBit = (picLen - 62) * 8 + picX / 4 * 3 * picY;

            for (i = 62; i < picLen; i++)
            {
                string binary = System.Convert.ToString(bufPic[i], 2);
                while (binary.Length < 8)
                {
                    binary = binary.Insert(0, "0");
                }
                head = binary.Substring(0, 4);
                tail = binary.Substring(4, 4);
                mark = mark + head + Hamming.Substring((i - 62) * 2 * 3, 3) + tail + Hamming.Substring(((i - 62) * 2 + 1) * 3, 3);
            }

            for (i = 0; i < picMarkBit; i++)
            {
                DBlink myLink = new DBlink();
                myLink.AddPicInfo(mark[i], (int)markLocate2[0][i], (int)markLocate2[1][i], TableName, DBType, field);
            }
        }
*/
        public byte[] GetPic(int picLen, byte picOffset1, byte picOffset2, byte picX, byte picY, ArrayList[] markLocate2, string TableName, string DBType, ArrayList field)
        {
            int i;
            string temp;
            int picMarkBit;
            byte[] myPic = new byte[picLen];
            string mark = "";
            int count = 0;
            int key = 62;
            byte[] head = {66,77,picOffset1,picOffset2,0,0,0,0,0,0,62,0,0,0,40,0,0,0,picX,0,0,0,picY,
                              0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,
                              0,2,0,0,0,0,0,0,255,255,255,255,255};


            picMarkBit = (picLen - 62) * 8;

            for (i = 0; i < picMarkBit; i++)
            {
                DBlink myLink = new DBlink();
                temp = myLink.GetPicInfo((int)markLocate2[0][i], (int)markLocate2[1][i], TableName, DBType, field);

                if ((int)temp[temp.Length - 1] == 8204)
                    mark += 0;
                else
                    mark += 1;

                count++;
                if (count == 8)
                {
                    count = 0;
                    myPic[key] = System.Convert.ToByte(mark, 2);
                    mark = "";
                    key++;
                }
            }
            for (i = 0; i < 62; i++)
            {
                myPic[i] = head[i];
            }

            return myPic;
        }
    }
}
