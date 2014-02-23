using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Collections;
using System.Data;

namespace WaterMark_DB1._6
{
    class DBlink
    {
        private static SqlConnection sqlCon = null;
        private static OleDbConnection acsCon = null;

        private ArrayList[] DBInfo = new ArrayList[5] { new ArrayList(), new ArrayList(), new ArrayList(), new ArrayList(), new ArrayList() };

        /// <summary>
        /// 连接SQL Server 数据库
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>数据表表名</returns>
        public string[] SQLconection(string filePath)
        {
            string[] strTable = null;

            try
            {
                sqlCon = new SqlConnection("server = .; database = " + filePath + "; Trusted_Connection=SSPI ");
                sqlCon.Open();
                DataTable shemaTable = sqlCon.GetSchema("Tables");
                int n = shemaTable.Rows.Count;
                strTable = new string[n];
                int m = shemaTable.Columns.IndexOf("TABLE_NAME");
                for (int i = 0; i < n; i++)
                {
                    DataRow m_DataRow = shemaTable.Rows[i];
                    strTable[i] = m_DataRow.ItemArray.GetValue(m).ToString();
                }

                return strTable;
            }
            catch
            {
                return strTable;
            }
        }

        /// <summary>
        /// 连接ACCESS数据库
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>数据表表名</returns>
        public string[] ACSconection(string filePath)
        {
            string[] strTable = null;

            try
            {
                acsCon = new OleDbConnection(@"Provider=Microsoft.Jet.OleDb.4.0; Data Source= " + filePath);
                acsCon.Open();
                DataTable shemaTable = acsCon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                int n = shemaTable.Rows.Count;
                strTable = new string[n];
                int m = shemaTable.Columns.IndexOf("TABLE_NAME");
                for (int i = 0; i < n; i++)
                {
                    DataRow m_DataRow = shemaTable.Rows[i];
                    strTable[i] = m_DataRow.ItemArray.GetValue(m).ToString();
                }

                return strTable;
            }
            catch
            {
                return strTable;
            }
        }

        public void Reconnection(string DBType)
        {
            switch (DBType)
            {
                case "Access":
                    acsCon.Close();
                    acsCon.Open();
                    break;
                case "SQL Server":
                    sqlCon.Close();
                    sqlCon.Open();
                    break;
                default:
                    break;
            }
        }

        public ArrayList[] GetDBinfo(string DBType, string TableName)
        {
            int count = 0;
            DataSet DS = new DataSet();
            string sql = "select * from [" + TableName + "]";
            switch (DBType)
            {
                case "Access":

                    OleDbDataAdapter adapter_acs = new OleDbDataAdapter(sql, acsCon);
                    adapter_acs.Fill(DS, "T1");
                    break;
                case "SQL Server":
                    SqlDataAdapter adapter_sql = new SqlDataAdapter(sql, sqlCon);
                    adapter_sql.Fill(DS,"T1");
                    break;
                default:
                    break;
            }

            DataTable DT = DS.Tables["T1"];
            DBInfo[0].Add(DT.Rows.Count);
            DBInfo[1].Add(DT.Columns.Count - 1);
            for (int i = 1; i < DT.Columns.Count; i++) //从1开始，排除ID字段
            {
                if (DT.Columns[i].DataType.ToString() == "System.String")
                {
                    DBInfo[4].Add(DT.Columns[i].ToString());
                    count++;
                }
                DBInfo[2].Add(DT.Columns[i].ToString());
            }
            DBInfo[3].Add(count);
            return DBInfo;

        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <param name="TableName">表名</param>
        /// <param name="DBType">数据库类型</param>
        /// <param name="field">字段名</param>
        /// <returns>数据</returns>
        public string getData(int row, int col, string TableName, string DBType, ArrayList field)
        {
            string info = "0";
            string NewCol;
            int NewRow = row + 1;
            NewCol = field[col].ToString();

            string sql = "select " + NewCol + " from " + TableName + " where id = " + NewRow;

            if (DBType.Equals("SQL Server"))
            {
                SqlCommand cmd = new SqlCommand(sql, sqlCon);

                SqlDataReader SDR = cmd.ExecuteReader();

                    if (SDR.Read())
                    {
                        info = SDR.GetValue(0).ToString();
                    }
                    else
                    {
                        SDR.Close();
                        return info;
                    }

                    SDR.Close();

                    int len = info.Length - 1;
                    //去掉不可见字符
                    while ((int)info[len] == 8204 || (int)info[len] == 8205)
                    {
                        info = info.Remove(len);
                        len--;
                    }

                    bool flag = false;
                    int temp;
                    flag = int.TryParse(info, out temp);
                    if (flag == true)
                        return info;
                    else
                    {
                        int sum = 0;
                        for (int j = 0; j < info.Length; j++)
                            sum += (int)info[j];
                        info = sum.ToString();
                        return info;
                    }
            }

            if (DBType.Equals("Access"))
            {
                OleDbCommand command = new OleDbCommand(sql, acsCon);
                try
                {
                    OleDbDataReader reader = command.ExecuteReader();   
                    if (reader.Read())
                    {
                        info = reader.GetValue(0).ToString();
                    }
                    else
                    {
                        reader.Close();
                        return info;
                    }
                    reader.Close();
                }
                catch
                {
                    Reconnection("Access");
                    OleDbDataReader reader = command.ExecuteReader(); 
                    if (reader.Read())
                    {
                        info = reader.GetValue(0).ToString();
                    }
                    else
                    {
                        reader.Close();
                        return info;
                    }
                    reader.Close();
                }
              
                int len = info.Length - 1;
                //去掉不可见字符
                while ((int)info[len] == 8204 || (int)info[len] == 8205)
                {
                    info = info.Remove(len);
                    len--;
                }

                bool flag = false;
                int temp;
                flag = int.TryParse(info, out temp);
                if (flag == true)
                    return info;
                else
                {
                    int sum = 0;
                    for (int j = 0; j < info.Length; j++)
                        sum += (int)info[j];
                    info = sum.ToString();
                    return info;
                }
            }
            else
            {
                return info;
            }
        }

        /// <summary>
        /// 加入图片水印
        /// </summary>
        /// <param name="mark">水印序列</param>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <param name="TableName">表名</param>
        /// <param name="DBType">数据库类型</param>
        /// <param name="field">字段名</param>
        public void AddPicInfo(char mark, int row, int col, string TableName, string DBType, ArrayList field)
        {
            string info = "";
            string NewCol;
            int NewRow = row + 1;
            NewCol = field[col].ToString();
            string sql = "select " + NewCol + " from " + TableName + " where id = " + NewRow;

            if (DBType.Equals("SQL Server"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = sqlCon;
                SqlDataReader SDR = cmd.ExecuteReader();

                if (SDR.Read())
                {
                    info = SDR.GetValue(0).ToString();
                }
                else
                {
                    return;
                }
                SDR.Close();

                int len = info.Length - 1;
                //去掉不可见字符
                while ((int)info[len] == 8204 || (int)info[len] == 8205)
                {
                    info = info.Remove(len);
                    len--;
                }

                char s;
                if (mark == '0')
                    s = (char)8204;
                else
                    s = (char)8205;

                info += s;

                string sql2 = "update " + TableName + " set [" + NewCol + "] = N'" + info + "' where [id] = " + NewRow;

                SqlCommand cmd1 = new SqlCommand();
                cmd1.CommandText = sql2.ToString();
                cmd1.Connection = sqlCon;
                cmd1.ExecuteNonQuery();
                
            }

            if (DBType.Equals("Access"))
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandText = sql;
                cmd.Connection = acsCon;
                OleDbDataReader ODR = cmd.ExecuteReader();

                if (ODR.Read())
                {
                    info = ODR.GetValue(0).ToString();
                }
                else
                {
                    return;
                }

                ODR.Close();

                int len = info.Length - 1;
                //去掉不可见字符
                while ((int)info[len] == 8204 || (int)info[len] == 8205)
                {
                    info = info.Remove(len);
                    len--;
                }

                char s;
                if (mark == '0')
                    s = (char)8204;
                else
                    s = (char)8205;

                info += s;

                string sql2 = "update " + TableName + " set [" + NewCol + "] = '" + info + "' where [id] = " + NewRow;

                OleDbCommand cmd1 = new OleDbCommand();
                cmd1.CommandText = sql2;
                cmd1.Connection = acsCon;
                cmd1.ExecuteNonQuery();
            }
        }


        public void DelOldMark(int row, int col, string TableName, string DBType, ArrayList field)
        {
            string info = "";
            string NewCol;
            int NewRow = row + 1;
            NewCol = field[col].ToString();
            string sql = "select " + NewCol + " from " + TableName + " where id = " + NewRow;
            if (DBType.Equals("SQL Server"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = sqlCon;
                SqlDataReader SDR = cmd.ExecuteReader();

                if (SDR.Read())
                {
                    info = SDR.GetValue(0).ToString();
                }
                else
                {
                    return;
                }
                SDR.Close();

                int len = info.Length - 1;
                //去掉不可见字符
                if ((int)info[len] == 8204 || (int)info[len] == 8205)
                {
                    info = info.Remove(len);
                }
                else
                {
                    return;
                }
            
                if (info.Contains("\""))
                {
                    info = info.Replace("\"", "\"\"");
                }

                string sql2 = "update " + TableName + " set [" + NewCol + "] = \"" + info + "\" where [id] = " + NewRow;

                SqlCommand cmd1 = new SqlCommand();
                cmd1.CommandText = sql2;
                cmd1.Connection = sqlCon;
                cmd1.ExecuteNonQuery();

            }

            if (DBType.Equals("Access"))
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandText = sql;
                cmd.Connection = acsCon;
                OleDbDataReader ODR = cmd.ExecuteReader();

                if (ODR.Read())
                {
                    info = ODR.GetValue(0).ToString();
                }
                else
                {
                    return;
                }

                ODR.Close();

                int len = info.Length - 1;
                //去掉不可见字符
                if ((int)info[len] == 8204 || (int)info[len] == 8205)
                {
                    info = info.Remove(len);
                }
                else
                {
                    return;
                }

                if (info.Contains("\""))
                {
                    info = info.Replace("\"", "\"\"");
                }

                string sql2 = "update " + TableName + " set [" + NewCol + "] = \"" + info + "\" where [id] = " + NewRow;

                OleDbCommand cmd1 = new OleDbCommand();
                cmd1.CommandText = sql2;
                cmd1.Connection = acsCon;
                cmd1.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 得到图片水印
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="col">列</param>
        /// <param name="TableName">表名</param>
        /// <param name="DBType">数据库类型</param>
        /// <param name="field">字段名</param>
        /// <returns>图片水印信息</returns>
        public string GetPicInfo(int row, int col, string TableName, string DBType, ArrayList field)
        {
            string info = "0";
            string NewCol;
            int NewRow = row + 1;
            NewCol = field[col].ToString();
            string sql = "select " + NewCol + " from " + TableName + " where id = " + NewRow;

            if (DBType.Equals("SQL Server"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = sqlCon;

                    SqlDataReader SDR = cmd.ExecuteReader();

                    if (SDR.Read())
                    {
                        info = SDR.GetValue(0).ToString();
                    }
                    else
                    {
                        SDR.Close();
                        return info;
                    }
                    SDR.Close();

            }

            if (DBType.Equals("Access"))
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandText = sql;
                cmd.Connection = acsCon;

                try
                {
                    OleDbDataReader ODR = cmd.ExecuteReader();

                    if (ODR.Read())
                    {
                        info = ODR.GetValue(0).ToString();
                    }
                    else
                    {
                        ODR.Close();
                        return info;
                    }
                    ODR.Close();
                }
                catch
                {
                    Reconnection("Access");
                    OleDbDataReader ODR = cmd.ExecuteReader();

                    if (ODR.Read())
                    {
                        info = ODR.GetValue(0).ToString();
                    }
                    else
                    {
                        ODR.Close();
                        return info;
                    }
                    ODR.Close();
                }
                
            }

            return info;
        }

        /// <summary>
        /// 遍历数据库
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="DBTpe">数据库类型</param>
        /// <returns>数据库数据信息</returns>
        public DataTable Read(string TableName, string DBType)
        {
            DataTable DT = new DataTable();
            string sql = "select * from " + TableName;
            
            if (DBType.Equals("SQL Server"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = sqlCon;
                SqlDataReader SDR = cmd.ExecuteReader();
                DT.Load(SDR);
                SDR.Close();
                return DT;
            }

            if (DBType.Equals("Access"))
            {
                OleDbCommand command = new OleDbCommand(sql, acsCon);
                OleDbDataReader reader = command.ExecuteReader();
                DT.Load(reader);
                reader.Close();
                return DT;
            }
            return DT;
        }

        /// <summary>
        /// 添加攻击
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void AddAttack(string TableName, int from, int to, string DBType)
        {
            int i;
            for (i = from; i <= to; i++)
            {
                Random r1 = new Random();
                Random r2 = new Random();
                Random r3 = new Random();
                Random r4 = new Random();
                Random r5 = new Random();
                Random r6 = new Random();

                int n1 = r1.Next(801, 329145);
                int n2 = r2.Next(0, 32150);
                int n3 = r3.Next(456, 131230);
                int n4 = r4.Next(10000, 89621);
                int n5 = r5.Next(1456, 13530);
                int n6 = r6.Next(10000, 89641);

                if (DBType.Equals("SQL Server"))
                {
                    string sql = "insert into " + TableName + " values(" + n1 + "," + n2 + "," + n3 + "," + n4 + "," + n5 + "," + n6 + ")";
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = sqlCon;
                    cmd.ExecuteNonQuery();
                }
                if (DBType.Equals("Access"))
                {
                    string sql = "insert into " + TableName + " values(" + i +","+ n1 + "," + n2 + "," + n3 + "," + n4 + "," + n5 + "," + n6 + ")";
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = acsCon;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// 删除攻击
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void DelAttack(string TableName, int from, int to, string DBType)
        {
            int i;
            for (i = from; i <= to; i++)
            {
                string sql = "delete from " + TableName + " where [id] = " + i;

                if (DBType.Equals("SQL Server"))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = sqlCon;
                    cmd.ExecuteNonQuery();
                }
                if (DBType.Equals("Access"))
                {
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = acsCon;
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// 更改攻击
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        /// <param name="id"></param>
        public void UpdAttack(string myTableName, string field, string value, int id, string DBType)
        {

            string sql = "update " + myTableName + " set [" + field + "] = \"" + value + "\" where [id] = " + id;

            if (DBType.Equals("SQL Server"))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = sqlCon;
                cmd.ExecuteNonQuery();
            }
            if (DBType.Equals("Access"))
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandText = sql;
                cmd.Connection = acsCon;
                cmd.ExecuteNonQuery();
            }
        }

    }
}
