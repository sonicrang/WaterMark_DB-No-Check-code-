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

namespace WaterMark_DB1._6
{
    /// <summary>
    /// ViewForm.xaml 的交互逻辑
    /// </summary>
    public partial class ViewForm : Window
    {
        private string TableName;
        private string DBType;

        public ViewForm(string TableName, string DBType)
        {
            InitializeComponent();
            this.TableName = TableName;
            this.DBType = DBType;
            lbTitle.Content = TableName;
            DBlink mylink = new DBlink();
            dgvDBView.ItemsSource = mylink.Read(TableName, DBType).DefaultView;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            DBlink myLink = new DBlink();
            dgvDBView.ItemsSource = myLink.Read(TableName, DBType).DefaultView;
        }
    }
}
