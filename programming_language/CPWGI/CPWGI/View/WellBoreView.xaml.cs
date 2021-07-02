using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CPWGI.Model;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CPWGI.View
{
    /// <summary>
    /// 井身结构的编辑及网格划分
    /// 通过wellBoreGrid属性取得网格划分结果
    /// </summary>
    public partial class WellBoreView : UserControl
    {
        public WellBoreClass wellBore { get; set; }

        public WellBoreView()
        {
            InitializeComponent();
            wellBore = new WellBoreClass();
            wellBoreListView.ItemsSource = wellBore.wellSectionList;
            //以下这三个属性实在textbox的lostFocus事件里改变的，如果用户选择了默认值，
            //则这三个值为空，后果很严重！所以要设置初值
            wellBore.meshLength = 10;
            wellBore.groundTemperature = 25;
            wellBore.temperaturePer100Meter = 2.19;
        }

        //井段的编辑功能
        #region 
        private void btnAddSection_Click(object sender, RoutedEventArgs e)
        {
            txbLength.Text = "0";
            txbOutD.Text = "0";
            txbInD.Text = "0";
            panelAddSection.Visibility = Visibility.Visible;
        }

        private void btnDeleteSection_Click(object sender, RoutedEventArgs e)
        {
            int index = wellBoreListView.SelectedIndex;
            if (index != -1)
                wellBore.wellSectionList.RemoveAt(index);
        }

        private void btnClearSection_Click(object sender, RoutedEventArgs e)
        {
            wellBore.wellSectionList.Clear();
        }

        //完成添加井段功能，并进行数据验证
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            double length, outD, inD;
            try
            {
                outD = Convert.ToDouble(txbOutD.Text);
                length = Convert.ToDouble(txbLength.Text);
                inD = Convert.ToDouble(txbInD.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确的数字", "参数错误！",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            wellBore.addSection(length, outD, inD);
            panelAddSection.Visibility = Visibility.Hidden;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            panelAddSection.Visibility = Visibility.Hidden;
        }
        #endregion
        
        //井身的网格划分
        private void btnMeshWell_Click(object sender, RoutedEventArgs e)
        {
            wellBore.meshWell();
            MessageBox.Show("网格划分已完成！","成功",MessageBoxButton.OK,MessageBoxImage.Asterisk);
        }
        
        //温度、地温梯度、网格长度的输入及验证
        //此段更优雅的方式使用数据绑定及数据验证实现
        #region
        private void txbdTdH_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;
            try
            {
                value = Convert.ToDouble(txbdTdH.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确的数字", "参数错误！", MessageBoxButton.OK, MessageBoxImage.Error);
                txbdTdH.Text = "0";
                txbdTdH.Focus();
                return;
            }
            wellBore.temperaturePer100Meter = value;
        }

        private void txbGroundT_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;
            try
            {
                value = Convert.ToDouble(txbGroundT.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确的数字", "参数错误！", MessageBoxButton.OK, MessageBoxImage.Error);
                txbGroundT.Text = "0";
                txbGroundT.Focus();
                return;
            }
            wellBore.groundTemperature = value;
        }

        private void txbGridLength_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;
            try
            {
                value = Convert.ToDouble(txbGridLength.Text);
                if (value <= 0)
                    throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确的正整数", "参数错误！", MessageBoxButton.OK, MessageBoxImage.Error);
                txbGridLength.Text = "10";
                txbGridLength.Focus();
                return;
            }
            wellBore.meshLength = value;
        }
        #endregion
    }

    //定义数据验证功能，暂时未使用
    public class myValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                Convert.ToDouble(value);
            }
            catch (Exception)
            {

                return new ValidationResult(false, "请输入正确的数字");
            }
            return new ValidationResult(true, null);
        }
    }
}
